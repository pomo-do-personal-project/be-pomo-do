using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Application.Common.Interfaces;

// Quản lý sinh mã OTP đăng ký, token đặt lại mật khẩu và xác thực
public interface IOtpService
{
    // Sinh và lưu mã OTP 6 số đăng ký (hạn 5 phút, cooldown 60s)
    Task<string> GenerateAndSaveOtpAsync(string email, OtpPurpose purpose = OtpPurpose.Register, CancellationToken ct = default);

    // Kiểm tra mã OTP 6 số người dùng nhập vào
    Task<bool> VerifyOtpAsync(string email, string otpCode, OtpPurpose purpose = OtpPurpose.Register, CancellationToken ct = default);

    // Lấy số giây còn lại cần chờ trước khi được gửi lại OTP
    Task<int> GetRemainingCooldownSecondsAsync(string email, OtpPurpose purpose = OtpPurpose.Register, CancellationToken ct = default);

    // Sinh token 64 ký tự cho nút bấm đặt lại mật khẩu (hạn 15 phút, cooldown 60s)
    Task<string> GeneratePasswordResetTokenAsync(string email, CancellationToken ct = default);

    // Kiểm tra token đặt lại mật khẩu từ link email
    Task<bool> VerifyPasswordResetTokenAsync(string email, string token, CancellationToken ct = default);

    // Lấy số giây còn lại cần chờ trước khi được gửi lại link đặt lại mật khẩu
    Task<int> GetPasswordResetCooldownSecondsAsync(string email, CancellationToken ct = default);
}
