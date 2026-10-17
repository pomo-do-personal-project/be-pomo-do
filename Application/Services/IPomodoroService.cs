using be_pomo_do.Application.DTOs.Pomodoro;

namespace be_pomo_do.Application.Services;

// Service contract for executing Pomodoro focus sessions and calculating daily streaks.
public interface IPomodoroService
{
    // Starts a new timer session.
    Task<FocusSessionDto> StartSessionAsync(Guid userId, StartSessionRequest request);

    // Completes an ongoing session with overtime calculations and updates user streak.
    Task<FocusSessionDto> CompleteSessionAsync(Guid sessionId, Guid userId, CompleteSessionRequest request);

    // Cancels an ongoing session.
    Task<FocusSessionDto> CancelSessionAsync(Guid sessionId, Guid userId, CancelSessionRequest request);

    // Retrieves session history within an optional UTC date range.
    Task<List<FocusSessionDto>> GetHistoryAsync(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null);

    // Retrieves summary statistics for today (pomos, focus minutes, overtime minutes, current streak).
    Task<PomodoroSummaryDto> GetSummaryAsync(Guid userId);
}
