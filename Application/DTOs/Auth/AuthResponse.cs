namespace be_pomo_do.Application.DTOs.Auth;

/// <summary>
/// Data transfer object returned upon successful authentication.
/// </summary>
/// <param name="User">The authenticated user's profile details.</param>
/// <param name="AccessToken">The short-lived JWT access token for authorization headers.</param>
/// <param name="RefreshToken">The long-lived refresh token for token renewal.</param>
/// <param name="ExpiresInSeconds">The remaining lifetime of the access token in seconds.</param>
public record AuthResponse(
    UserDto User,
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds
);
