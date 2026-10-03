using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Application.DTOs.Pomodoro;

/// <summary>Represents details of a recorded focus or stopwatch session.</summary>
public class FocusSessionDto
{
    /// <summary>Unique identifier of the session.</summary>
    public Guid Id { get; set; }

    /// <summary>User who completed this session.</summary>
    public Guid UserId { get; set; }

    /// <summary>Optional linked task identifier.</summary>
    public Guid? TaskId { get; set; }

    /// <summary>Title of the linked task if available.</summary>
    public string? TaskTitle { get; set; }

    /// <summary>Session mode: Pomo (1) or Stopwatch (2).</summary>
    public FocusSessionMode Mode { get; set; }

    /// <summary>Planned duration in minutes (e.g. 25).</summary>
    public int DurationMinutes { get; set; }

    /// <summary>Actual focus duration achieved in seconds.</summary>
    public int ActualSeconds { get; set; }

    /// <summary>Actual focus duration achieved in minutes.</summary>
    public int ActualMinutes => (int)Math.Round((double)ActualSeconds / 60);

    /// <summary>Overtime seconds worked beyond planned duration.</summary>
    public int OvertimeSeconds { get; set; }

    /// <summary>Overtime minutes worked beyond planned duration.</summary>
    public int OvertimeMinutes => (int)Math.Max(0, Math.Round((double)OvertimeSeconds / 60));

    /// <summary>Indicates whether the session was finished normally or canceled.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>UTC start timestamp.</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>UTC completion timestamp.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Optional notes recorded for this session.</summary>
    public string? Note { get; set; }
}
