namespace be_pomo_do.Application.Common.Interfaces;

// Cryptographic password hashing and verification services
public interface IPasswordHasher
{
    // Hashes plain-text password using BCrypt
    string HashPassword(string password);

    // Verifies whether plain-text password matches stored hash
    bool VerifyPassword(string password, string passwordHash);
}
