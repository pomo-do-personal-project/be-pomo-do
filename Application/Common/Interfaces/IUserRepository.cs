using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Common.Interfaces;

// Defines the repository contract for user persistence, retrieval, and refresh token lifecycle operations.
public interface IUserRepository
{
    // Finds a user entity by their unique email address.
    Task<User?> GetByEmailAsync(string email);

    // Finds a user entity by their unique identifier.
    Task<User?> GetByIdAsync(Guid id);

    // Determines whether an account with the specified email address already exists.
    Task<bool> ExistsByEmailAsync(string email);

    // Adds a new user entity to the persistence store.
    Task AddAsync(User user);

    // Adds a new refresh token entity associated with a user to the persistence store.
    Task AddRefreshTokenAsync(RefreshToken refreshToken);

    // Removes a stale or revoked refresh token entity.
    void RemoveRefreshToken(RefreshToken refreshToken);

    // Retrieves a refresh token record along with its associated user entity.
    Task<RefreshToken?> GetRefreshTokenWithUserAsync(string token);

    // Saves all pending entity state changes to the database.
    Task SaveChangesAsync();
}
