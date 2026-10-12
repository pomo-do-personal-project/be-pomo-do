using be_pomo_do.Application.DTOs.Auth;

namespace be_pomo_do.Application.Services;

// Nghiệp vụ xác thực người dùng, đăng ký, đăng nhập và quản lý phiên
public interface IAuthService
{
    // Đăng ký tài khoản mới kèm mã OTP
    Task<UserDto> RegisterAsync(RegisterRequest request);

    // Đăng nhập tài khoản và cấp cặp token JWT
    Task<AuthResponse> LoginAsync(LoginRequest request);

    // Làm mới access token bằng refresh token
    Task<AuthResponse> RefreshTokenAsync(string refreshTokenString);

    // Lấy thông tin tài khoản của người dùng hiện tại
    Task<UserDto> GetMeAsync(Guid userId);

    // Gửi mã OTP 6 số xác thực đăng ký tài khoản
    Task SendOtpAsync(SendOtpRequest request);

    // Gửi email chứa nút bấm xác nhận đặt lại mật khẩu
    Task ForgotPasswordAsync(ForgotPasswordRequest request);

    // Đặt lại mật khẩu mới bằng token từ link email
    Task ResetPasswordAsync(ResetPasswordRequest request);

    // Đăng xuất và thu hồi refresh token
    Task LogoutAsync(string? refreshTokenString);
}
