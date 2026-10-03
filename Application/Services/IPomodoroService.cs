using be_pomo_do.Application.DTOs.Pomodoro;

namespace be_pomo_do.Application.Services;

/// <summary>Service contract for executing Pomodoro focus sessions and calculating daily streaks.</summary>
public interface IPomodoroService
{
    /// <summary>Starts a new timer session.</summary>
    Task<FocusSessionDto> StartSessionAsync(Guid userId, StartSessionRequest request, CancellationToken ct = default);

    /// <summary>Completes an ongoing session with overtime calculations and updates user streak.</summary>
    Task<FocusSessionDto> CompleteSessionAsync(Guid sessionId, Guid userId, CompleteSessionRequest request, CancellationToken ct = default);

    /// <summary>Cancels an ongoing session.</summary>
    Task<FocusSessionDto> CancelSessionAsync(Guid sessionId, Guid userId, CancelSessionRequest request, CancellationToken ct = default);

    /// <summary>Retrieves session history within an optional UTC date range.</summary>
    Task<List<FocusSessionDto>> GetHistoryAsync(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken ct = default);

    /// <summary>Retrieves summary statistics for today (pomos, focus minutes, overtime minutes, current streak).</summary>
    Task<PomodoroSummaryDto> GetSummaryAsync(Guid userId, CancellationToken ct = default);
}
