using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Common.Interfaces;

/// <summary>
/// Provides operations for generating JSON Web Tokens and refresh tokens.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates a signed JWT access token for the specified user.
    /// </summary>
    /// <param name="user">The user entity whose claims are embedded in the token.</param>
    /// <returns>A signed JWT access token string.</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a cryptographically secure random refresh token.
    /// </summary>
    /// <returns>A base64-encoded random refresh token string.</returns>
    string GenerateRefreshToken();
}
