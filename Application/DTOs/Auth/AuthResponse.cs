namespace be_pomo_do.Application.DTOs.Auth;

// Data transfer object returned upon successful authentication
public record AuthResponse(
    UserDto User,
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds
);
