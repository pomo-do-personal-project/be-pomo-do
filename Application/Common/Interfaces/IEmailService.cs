using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Application.Common.Interfaces;

// Dịch vụ gửi email thông báo và mã xác thực
public interface IEmailService
{
    // Gửi email chứa mã OTP 6 số đăng ký tài khoản
    Task SendOtpEmailAsync(string toEmail, string otpCode, CancellationToken ct = default);

    // Gửi email chứa nút bấm xác nhận đặt lại mật khẩu
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default);
}
