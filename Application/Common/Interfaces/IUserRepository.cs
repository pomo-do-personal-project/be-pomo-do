using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Common.Interfaces;

/// <summary>
/// Provides access to user entity persistence operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Finds a user by email address asynchronously.
    /// </summary>
    /// <param name="email">The user email to search for.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the operation, containing the user if found; otherwise, <see langword="null"/>.</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a user by their unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the operation, containing the user if found; otherwise, <see langword="null"/>.</returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a user with the specified email already exists.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns><see langword="true"/> if the email exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user entity to the persistence store.
    /// </summary>
    /// <param name="user">The user entity to add.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous add operation.</returns>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new refresh token entity directly to the persistence store.
    /// </summary>
    /// <param name="refreshToken">The refresh token entity to add.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous add operation.</returns>
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all pending changes to the underlying database.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
