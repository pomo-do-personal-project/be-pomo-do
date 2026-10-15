namespace be_pomo_do.Infrastructure.Templates;

/// <summary>HTML Email templates for OTP verification and password reset workflows.</summary>
public static class EmailTemplates
{
    // Tạo HTML template cho email chứa link đổi mật khẩu
    public static string BuildPasswordResetHtml(string resetLink)
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

    // Tạo HTML template cho email chứa mã OTP
    public static string BuildOtpEmailHtml(string otpCode, string actionText)
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
