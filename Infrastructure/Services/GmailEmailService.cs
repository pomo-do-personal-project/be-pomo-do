using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Core.ValueObjects;
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

        // 1. In mã OTP to rõ ràng ra Console để hỗ trợ kiểm thử trực tiếp cực nhanh
        _logger.LogInformation(
            "\n==================================================\n" +
            "📢 [POMO-DO REGISTER OTP DISPATCH]\n" +
            "To: {ToEmail}\n" +
            "OTP Code: >>> {OtpCode} <<<\n" +
            "Expires: 5 minutes\n" +
            "==================================================",
            toEmail, otpCode);

        // 2. Chuẩn bị nội dung email đăng ký tài khoản
        const string subject = "[Pomo-Do] Mã xác thực đăng ký tài khoản";
        const string actionText = "hoàn tất đăng ký tài khoản mới trên Pomo-Do";
        var htmlBody = BuildHtmlTemplate(otpCode, actionText);

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

        // 3. Gửi email qua Gmail SMTP
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

            _logger.LogInformation("OTP email successfully sent to {ToEmail} via Gmail SMTP.", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {ToEmail} via SMTP. Code is available in Console: {OtpCode}", toEmail, otpCode);
        }
    }

    // Gửi email chứa nút bấm xác nhận đặt lại mật khẩu
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(resetToken);

        var frontendBase = string.IsNullOrWhiteSpace(_options.FrontendUrl) ? "https://localhost:3000" : _options.FrontendUrl.TrimEnd('/');
        var resetLink = $"{frontendBase}/reset-password?email={Uri.EscapeDataString(toEmail)}&token={resetToken}";

        // 1. In liên kết và token ra Console để lập trình viên kiểm thử nhanh
        _logger.LogInformation(
            "\n==================================================\n" +
            "📢 [POMO-DO PASSWORD RESET ACTION BUTTON]\n" +
            "To: {ToEmail}\n" +
            "Reset Link: >>> {ResetLink} <<<\n" +
            "Token: >>> {ResetToken} <<<\n" +
            "Expires: 15 minutes\n" +
            "==================================================",
            toEmail, resetLink, resetToken);

        // 2. Soạn email chứa nút xác nhận
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.ReplyTo.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
        message.Subject = "[Pomo-Do] Xác nhận đặt lại mật khẩu của bạn";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = BuildPasswordResetHtmlTemplate(resetLink),
            TextBody = $"Để đặt lại mật khẩu cho tài khoản Pomo-Do của bạn, vui lòng mở liên kết sau: {resetLink}\nLiên kết có hiệu lực trong 15 phút."
        };
        message.Body = bodyBuilder.ToMessageBody();

        // 3. Gửi qua SMTP
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

            _logger.LogInformation("Password reset confirmation email successfully sent to {ToEmail} via Gmail SMTP.", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {ToEmail} via SMTP. Reset link is available in Console: {ResetLink}", toEmail, resetLink);
        }
    }

    private static string BuildPasswordResetHtmlTemplate(string resetLink)
    {
        return $$"""
        <!DOCTYPE html>
        <html lang="vi">
        <head>
          <meta charset="UTF-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <title>Đặt lại mật khẩu Pomo-Do</title>
        </head>
        <body style="margin: 0; padding: 0; background-color: #fafafa; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #171717; -webkit-font-smoothing: antialiased;">
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color: #fafafa; padding: 48px 16px;">
            <tr>
              <td align="center">
                <table role="presentation" width="100%" style="max-width: 480px; background-color: #ffffff; border-radius: 12px; border: 1px solid #e5e5e5; padding: 40px 32px; box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.05);">
                  
                  <tr>
                    <td style="padding-bottom: 24px;">
                      <span style="font-size: 16px; font-weight: 700; letter-spacing: -0.3px; color: #0a0a0a; text-transform: uppercase;">
                        pomo<span style="color: #e11d48;">-do</span>
                      </span>
                    </td>
                  </tr>

                  <tr>
                    <td style="padding-bottom: 20px;">
                      <h1 style="margin: 0 0 12px 0; font-size: 20px; font-weight: 600; color: #0a0a0a; letter-spacing: -0.4px;">
                        Yêu cầu đặt lại mật khẩu
                      </h1>
                      <p style="margin: 0; font-size: 14px; line-height: 22px; color: #525252;">
                        Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản Pomo-Do của bạn. Vui lòng bấm vào nút bên dưới để tiến hành thiết lập mật khẩu mới:
                      </p>
                    </td>
                  </tr>

                  <tr>
                    <td style="padding: 12px 0 28px 0; text-align: center;">
                      <a href="{{resetLink}}" target="_blank" style="display: inline-block; background-color: #e11d48; color: #ffffff; font-size: 15px; font-weight: 600; padding: 14px 28px; text-decoration: none; border-radius: 8px; box-shadow: 0 4px 6px -1px rgba(225, 29, 72, 0.25);">
                        Xác nhận đặt lại mật khẩu
                      </a>
                    </td>
                  </tr>

                  <tr>
                    <td style="padding-bottom: 24px; border-bottom: 1px solid #f4f4f5;">
                      <p style="margin: 0 0 8px 0; font-size: 13px; line-height: 20px; color: #71717a;">
                        Nếu nút trên không bấm được, bạn có thể copy và dán liên kết sau vào trình duyệt:
                      </p>
                      <p style="margin: 0; font-size: 12px; line-height: 18px; color: #2563eb; word-break: break-all;">
                        <a href="{{resetLink}}" style="color: #2563eb; text-decoration: underline;">{{resetLink}}</a>
                      </p>
                      <p style="margin: 12px 0 0 0; font-size: 12px; line-height: 18px; color: #a1a1aa;">
                        Liên kết này có hiệu lực trong vòng <strong>15 phút</strong>. Nếu bạn không yêu cầu, vui lòng bỏ qua email này.
                      </p>
                    </td>
                  </tr>

                  <tr>
                    <td style="padding-top: 20px;">
                      <p style="margin: 0; font-size: 12px; line-height: 18px; color: #a1a1aa;">
                        Pomo-Do &bull; Pomodoro &amp; Task Management System
                      </p>
                    </td>
                  </tr>

                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;
    }

    private static string BuildHtmlTemplate(string otpCode, string actionText)
    {
        return $$"""
        <!DOCTYPE html>
        <html lang="vi">
        <head>
          <meta charset="UTF-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <title>Mã xác thực Pomo-Do</title>
        </head>
        <body style="margin: 0; padding: 0; background-color: #fafafa; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #171717; -webkit-font-smoothing: antialiased;">
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color: #fafafa; padding: 48px 16px;">
            <tr>
              <td align="center">
                <table role="presentation" width="100%" style="max-width: 440px; background-color: #ffffff; border-radius: 12px; border: 1px solid #e5e5e5; padding: 40px 32px; box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.05);">
                  
                  <tr>
                    <td style="padding-bottom: 24px;">
                      <span style="font-size: 16px; font-weight: 700; letter-spacing: -0.3px; color: #0a0a0a; text-transform: uppercase;">
                        pomo<span style="color: #e11d48;">-do</span>
                      </span>
                    </td>
                  </tr>

                  <tr>
                    <td style="padding-bottom: 24px;">
                      <h1 style="margin: 0 0 12px 0; font-size: 20px; font-weight: 600; color: #0a0a0a; letter-spacing: -0.4px;">
                        Mã xác thực của bạn
                      </h1>
                      <p style="margin: 0; font-size: 14px; line-height: 22px; color: #525252;">
                        Dưới đây là mã xác thực dùng một lần (OTP) để {{actionText}}. Mã này có hiệu lực trong <strong>5 phút</strong>.
                      </p>
                    </td>
                  </tr>

                  <tr>
                    <td style="padding-bottom: 24px;">
                      <div style="background-color: #f4f4f5; border-radius: 8px; border: 1px solid #e4e4e7; padding: 18px 0; text-align: center;">
                        <span style="font-family: SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', 'Courier New', monospace; font-size: 32px; font-weight: 700; letter-spacing: 6px; color: #09090b; display: inline-block;">
                          {{otpCode}}
                        </span>
                      </div>
                    </td>
                  </tr>

                  <tr>
                    <td style="padding-bottom: 32px; border-bottom: 1px solid #f4f4f5;">
                      <p style="margin: 0; font-size: 13px; line-height: 20px; color: #71717a;">
                        Nếu bạn không thực hiện yêu cầu này, bạn có thể yên tâm bỏ qua email này. Vui lòng không chia sẻ mã cho bất kỳ ai để bảo vệ tài khoản.
                      </p>
                    </td>
                  </tr>

                  <tr>
                    <td style="padding-top: 24px;">
                      <p style="margin: 0; font-size: 12px; line-height: 18px; color: #a1a1aa;">
                        Pomo-Do &bull; Pomodoro &amp; Task Management System
                      </p>
                    </td>
                  </tr>

                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;
    }
}
