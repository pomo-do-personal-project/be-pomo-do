namespace be_pomo_do.Application.Common.Interfaces;

/// <summary>
/// Provides cryptographic password hashing and verification services.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes the specified plain-text password using a salted cryptographic algorithm.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>The secure hashed password string.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies whether a plain-text password matches a hashed password.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="passwordHash">The stored password hash.</param>
    /// <returns><see langword="true"/> if the password matches; otherwise, <see langword="false"/>.</returns>
    bool VerifyPassword(string password, string passwordHash);
}
