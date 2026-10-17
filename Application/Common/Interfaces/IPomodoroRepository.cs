using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Common.Interfaces;

// Repository interface for FocusSession and DailyStreak data access.
public interface IPomodoroRepository
{
    // Retrieves a focus session by ID and user ID.
    Task<FocusSession?> GetSessionByIdAsync(Guid sessionId, Guid userId);

    // Retrieves session history for a user within an optional UTC date range.
    Task<List<FocusSession>> GetSessionHistoryAsync(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null);

    // Adds a new focus session to persistence.
    Task AddSessionAsync(FocusSession session);

    // Retrieves daily streak entity for a user.
    Task<DailyStreak?> GetDailyStreakByUserIdAsync(Guid userId);

    // Adds a daily streak record if one does not exist.
    Task AddDailyStreakAsync(DailyStreak dailyStreak);

    // Persists all tracked changes to the database.
    Task SaveChangesAsync();
}
