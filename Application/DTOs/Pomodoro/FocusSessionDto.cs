using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Application.DTOs.Pomodoro;

// Represents details of a recorded focus or stopwatch session.
public class FocusSessionDto
{
    // Unique identifier of the session.
    public Guid Id { get; set; }

    // User who completed this session.
    public Guid UserId { get; set; }

    // Optional linked task identifier.
    public Guid? TaskId { get; set; }

    // Title of the linked task if available.
    public string? TaskTitle { get; set; }

    // Session mode: Pomo (1) or Stopwatch (2).
    public FocusSessionMode Mode { get; set; }

    // Planned duration in minutes (e.g. 25).
    public int DurationMinutes { get; set; }

    // Actual focus duration achieved in seconds.
    public int ActualSeconds { get; set; }

    // Actual focus duration achieved in minutes.
    public int ActualMinutes => (int)Math.Round((double)ActualSeconds / 60);

    // Overtime seconds worked beyond planned duration.
    public int OvertimeSeconds { get; set; }

    // Overtime minutes worked beyond planned duration.
    public int OvertimeMinutes => (int)Math.Max(0, Math.Round((double)OvertimeSeconds / 60));

    // Indicates whether the session was finished normally or canceled.
    public bool IsCompleted { get; set; }

    // UTC start timestamp.
    public DateTime StartedAt { get; set; }

    // UTC completion timestamp.
    public DateTime? CompletedAt { get; set; }

    // Optional notes recorded for this session.
    public string? Note { get; set; }
}
