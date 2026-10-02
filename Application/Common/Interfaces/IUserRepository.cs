using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Common.Interfaces;

// Repository contract for user persistence and retrieval
public interface IUserRepository
{
    // Finds a user by email address
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    // Finds a user by unique identifier
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Checks whether an email address is already registered
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    // Adds a new user entity to the database
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    // Adds a new refresh token entity directly to the database
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    // Saves all pending changes to the database
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
