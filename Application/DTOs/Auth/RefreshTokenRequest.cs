namespace be_pomo_do.Application.DTOs.Auth;

// Represents the optional payload for refreshing an expired access token or revoking a session.
public record RefreshTokenRequest(
    string? RefreshToken
);

