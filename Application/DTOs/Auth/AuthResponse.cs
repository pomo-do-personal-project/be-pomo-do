namespace be_pomo_do.Application.DTOs.Auth;

// DTO trả về sau khi đăng nhập hoặc làm mới token thành công
public record AuthResponse(
    UserDto User,
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds
);

