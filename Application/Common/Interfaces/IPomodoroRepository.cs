using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Common.Interfaces;

/// <summary>Repository interface for FocusSession and DailyStreak data access.</summary>
public interface IPomodoroRepository
{
    /// <summary>Retrieves a focus session by ID and user ID.</summary>
    Task<FocusSession?> GetSessionByIdAsync(Guid sessionId, Guid userId, CancellationToken ct = default);

    /// <summary>Retrieves session history for a user within an optional UTC date range.</summary>
    Task<List<FocusSession>> GetSessionHistoryAsync(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken ct = default);

    /// <summary>Adds a new focus session to persistence.</summary>
    Task AddSessionAsync(FocusSession session, CancellationToken ct = default);

    /// <summary>Retrieves daily streak entity for a user.</summary>
    Task<DailyStreak?> GetDailyStreakByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Adds a daily streak record if one does not exist.</summary>
    Task AddDailyStreakAsync(DailyStreak dailyStreak, CancellationToken ct = default);

    /// <summary>Persists all tracked changes to the database.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
