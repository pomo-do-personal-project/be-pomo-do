namespace be_pomo_do.Application.DTOs.Pomodoro;

// Summary metrics for Dashboard and Statistics panels.
public class PomodoroSummaryDto
{
    // Total pomodoro sessions completed today.
    public int TotalPomosToday { get; set; }

    // Total minutes of focus completed today.
    public int TotalFocusMinutesToday { get; set; }

    // Total overtime minutes worked today beyond planned duration.
    public int TotalOvertimeMinutesToday { get; set; }

    // Current active daily streak count.
    public int CurrentStreak { get; set; }

    // Longest daily streak achieved so far.
    public int LongestStreak { get; set; }

    // Date the streak was last recorded (UTC).
    public DateOnly? LastActiveDate { get; set; }
}
