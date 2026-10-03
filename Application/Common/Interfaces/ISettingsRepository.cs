using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Common.Interfaces;

/// <summary>Repository contract for querying and persisting user settings.</summary>
public interface ISettingsRepository
{
    /// <summary>Retrieves user settings by user identifier.</summary>
    Task<UserSettings?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Adds initial settings for a user.</summary>
    Task AddAsync(UserSettings settings, CancellationToken ct = default);

    /// <summary>Saves all pending changes to the database context.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
