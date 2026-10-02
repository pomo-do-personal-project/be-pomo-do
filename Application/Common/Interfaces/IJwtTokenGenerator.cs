using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Common.Interfaces;

// Token generation operations for access and refresh tokens
public interface IJwtTokenGenerator
{
    // Generates a signed JWT access token for the specified user
    string GenerateAccessToken(User user);

    // Generates a cryptographically secure random refresh token (64 bytes)
    string GenerateRefreshToken();
}
