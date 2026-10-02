namespace be_pomo_do.Application.DTOs.Auth;

/// <summary>Represents the optional payload for refreshing an expired access token or revoking a session.</summary>
public record RefreshTokenRequest(
    string? RefreshToken
);

