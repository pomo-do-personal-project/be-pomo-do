namespace be_pomo_do.Application.DTOs.Auth;

/// <summary>Represents the data transfer object returned upon successful authentication or token rotation.</summary>
public record AuthResponse(
    UserDto User,
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds
);

