using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Infrastructure.Templates;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace be_pomo_do.Infrastructure.Services;

/// <summary>Sends transactional OTP verification emails via Gmail SMTP with modern HTML templates and console logging.</summary>
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

        _logger.LogInformation(
            "\n==================================================\n" +
            "📢 [POMO-DO REGISTER OTP DISPATCH]\n" +
            "To: {ToEmail}\n" +
            "OTP Code: >>> {OtpCode} <<<\n" +
            "Expires: 5 minutes\n" +
            "==================================================",
            toEmail, otpCode);

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

        await SendViaSmtpAsync(message, toEmail, $"OTP: {otpCode}", ct);
    }

    // Gửi email chứa nút bấm xác nhận đặt lại mật khẩu
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(resetToken);

        var frontendBase = string.IsNullOrWhiteSpace(_options.FrontendUrl) ? "https://localhost:3000" : _options.FrontendUrl.TrimEnd('/');
        var resetLink = $"{frontendBase}/reset-password?email={Uri.EscapeDataString(toEmail)}&token={resetToken}";

        _logger.LogInformation(
            "\n==================================================\n" +
            "📢 [POMO-DO PASSWORD RESET ACTION BUTTON]\n" +
            "To: {ToEmail}\n" +
            "Reset Link: >>> {ResetLink} <<<\n" +
            "Token: >>> {ResetToken} <<<\n" +
            "Expires: 15 minutes\n" +
            "==================================================",
            toEmail, resetLink, resetToken);

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

    // Gửi message qua SmtpClient kết nối Gmail
    private async Task SendViaSmtpAsync(MimeMessage message, string toEmail, string context, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_options.Username) || string.IsNullOrWhiteSpace(_options.Password))
            {
                _logger.LogWarning("SMTP credentials not fully configured. Email was logged to console only.");
                return;
            }

            using var client = new SmtpClient();
            var secureSocketOption = _options.EnableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

            await client.ConnectAsync(_options.Server, _options.Port, secureSocketOption, ct);
            await client.AuthenticateAsync(_options.Username, _options.Password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation("Email ({Context}) successfully sent to {ToEmail} via Gmail SMTP.", context, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email ({Context}) to {ToEmail} via SMTP.", context, toEmail);
        }
    }
}
