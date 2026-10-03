namespace be_pomo_do.Application.DTOs.Pomodoro;

/// <summary>Summary metrics for Dashboard and Statistics panels.</summary>
public class PomodoroSummaryDto
{
    /// <summary>Total pomodoro sessions completed today.</summary>
    public int TotalPomosToday { get; set; }

    /// <summary>Total minutes of focus completed today.</summary>
    public int TotalFocusMinutesToday { get; set; }

    /// <summary>Total overtime minutes worked today beyond planned duration.</summary>
    public int TotalOvertimeMinutesToday { get; set; }

    /// <summary>Current active daily streak count.</summary>
    public int CurrentStreak { get; set; }

    /// <summary>Longest daily streak achieved so far.</summary>
    public int LongestStreak { get; set; }

    /// <summary>Date the streak was last recorded (UTC).</summary>
    public DateOnly? LastActiveDate { get; set; }
}
