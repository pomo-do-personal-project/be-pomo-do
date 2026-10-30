using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Infrastructure.Templates;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace be_pomo_do.Infrastructure.Services;

// Sends transactional OTP verification emails via Gmail SMTP with modern HTML templates and console logging.
public class GmailEmailService : IEmailService
{
    private readonly SmtpOptions _options;
    private readonly ILogger<GmailEmailService> _logger;

    // Khởi tạo GmailEmailService với SMTP options và logger
    public GmailEmailService(IOptions<SmtpOptions> options, ILogger<GmailEmailService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Gửi email chứa mã OTP 6 số đăng ký tài khoản
    public async Task SendOtpEmailAsync(string toEmail, string otpCode, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(otpCode);

        if (string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "\n==================================================\n" +
                "📢 [DEV ONLY] [POMO-DO REGISTER OTP DISPATCH]\n" +
                "To: {ToEmail}\n" +
                "OTP Code: >>> {OtpCode} <<<\n" +
                "Expires: 5 minutes\n" +
                "==================================================",
                toEmail, otpCode);
        }
        else
        {
            _logger.LogInformation("📢 [POMO-DO REGISTER OTP DISPATCH] Dispatching registration OTP email to {ToEmail}", toEmail);
        }

        const string subject = "[Pomo-Do] Mã xác thực đăng ký tài khoản";
        const string actionText = "hoàn tất đăng ký tài khoản mới trên Pomo-Do";
        var htmlBody = EmailTemplates.BuildOtpEmailHtml(otpCode, actionText);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.ReplyTo.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = $"Mã xác thực Pomo-Do của bạn là: {otpCode}. Mã có hiệu lực trong 5 phút. Vui lòng không chia sẻ mã này cho bất kỳ ai."
        };
        message.Body = bodyBuilder.ToMessageBody();

        await SendViaSmtpAsync(message, toEmail, "Register OTP", ct);
    }

    // Gửi email chứa nút bấm xác nhận đặt lại mật khẩu
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(resetToken);

        var frontendBase = string.IsNullOrWhiteSpace(_options.FrontendUrl) ? "https://localhost:3000" : _options.FrontendUrl.TrimEnd('/');
        var resetLink = $"{frontendBase}/reset-password?email={Uri.EscapeDataString(toEmail)}&token={resetToken}";

        if (string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "\n==================================================\n" +
                "📢 [DEV ONLY] [POMO-DO PASSWORD RESET ACTION BUTTON]\n" +
                "To: {ToEmail}\n" +
                "Reset Link: >>> {ResetLink} <<<\n" +
                "Token: >>> {ResetToken} <<<\n" +
                "Expires: 15 minutes\n" +
                "==================================================",
                toEmail, resetLink, resetToken);
        }
        else
        {
            _logger.LogInformation("📢 [POMO-DO PASSWORD RESET] Dispatching password reset email to {ToEmail}", toEmail);
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.ReplyTo.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
        message.Subject = "[Pomo-Do] Xác nhận đặt lại mật khẩu của bạn";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = EmailTemplates.BuildPasswordResetHtml(resetLink),
            TextBody = $"Để đặt lại mật khẩu cho tài khoản Pomo-Do của bạn, vui lòng mở liên kết sau: {resetLink}\nLiên kết có hiệu lực trong 15 phút."
        };
        message.Body = bodyBuilder.ToMessageBody();

        await SendViaSmtpAsync(message, toEmail, "Password Reset", ct);
    }

    // Gửi email báo cáo định kỳ với nội dung HTML tùy chỉnh
    public async Task SendEmailReportAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(htmlBody);

        _logger.LogInformation(
            "\n==================================================\n" +
            "📢 [POMO-DO EMAIL REPORT DISPATCH]\n" +
            "To: {ToEmail}\n" +
            "Subject: {Subject}\n" +
            "==================================================",
            toEmail, subject);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.ReplyTo.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = "Vui lòng mở ứng dụng hoặc xem trên trình duyệt hỗ trợ HTML để đọc báo cáo chi tiết."
        };
        message.Body = bodyBuilder.ToMessageBody();

        await SendViaSmtpAsync(message, toEmail, subject, ct);
    }

    // Gửi message qua SmtpClient kết nối Gmail với cơ chế retry và báo lỗi rõ ràng
    private async Task SendViaSmtpAsync(MimeMessage message, string toEmail, string context, CancellationToken ct)
    {
        var isDev = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(_options.Username) || string.IsNullOrWhiteSpace(_options.Password))
        {
            if (isDev)
            {
                _logger.LogWarning("SMTP credentials not configured. [DEV ONLY] Email was logged to console only.");
                return;
            }

            throw new InvalidOperationException("Cấu hình tài khoản SMTP chưa được thiết lập trên môi trường Production.");
        }

        Exception? lastException = null;
        const int maxRetries = 3;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var client = new SmtpClient();
                var secureSocketOption = _options.EnableSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.Auto;

                await client.ConnectAsync(_options.Server, _options.Port, secureSocketOption, ct);
                await client.AuthenticateAsync(_options.Username, _options.Password, ct);
                await client.SendAsync(message, ct);
                await client.DisconnectAsync(true, ct);

                _logger.LogInformation("Email ({Context}) successfully sent to {ToEmail} via Gmail SMTP (Attempt {Attempt}).", context, toEmail, attempt);
                return;
            }
            catch (Exception ex) when (attempt < maxRetries && !ct.IsCancellationRequested)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Lỗi gửi email ({Context}) lần {Attempt} tới {ToEmail}. Thử lại sau {Delay}ms...", context, attempt, toEmail, attempt * 500);
                await Task.Delay(attempt * 500, ct);
            }
            catch (Exception ex)
            {
                lastException = ex;
                break;
            }
        }

        _logger.LogError(lastException, "Failed to send email ({Context}) to {ToEmail} via SMTP after {MaxRetries} attempts.", context, toEmail, maxRetries);
        throw new InvalidOperationException($"Không thể gửi email tới {toEmail} qua SMTP: {lastException?.Message}", lastException);
    }
}
