using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Common.Interfaces;

/// <summary>Defines the repository contract for user persistence, retrieval, and refresh token lifecycle operations.</summary>
public interface IUserRepository
{
    /// <summary>Finds a user entity by their unique email address.</summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>Finds a user entity by their unique identifier.</summary>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>Determines whether an account with the specified email address already exists.</summary>
    Task<bool> ExistsByEmailAsync(string email);

    /// <summary>Adds a new user entity to the persistence store.</summary>
    Task AddAsync(User user);

    /// <summary>Adds a new refresh token entity associated with a user to the persistence store.</summary>
    Task AddRefreshTokenAsync(RefreshToken refreshToken);

    /// <summary>Retrieves a refresh token record along with its associated user entity.</summary>
    Task<RefreshToken?> GetRefreshTokenWithUserAsync(string token);

    /// <summary>Saves all pending entity state changes to the database.</summary>
    Task SaveChangesAsync();
}
