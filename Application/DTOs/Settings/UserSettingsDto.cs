namespace be_pomo_do.Application.DTOs.Settings;

// Data transfer object cho cấu hình người dùng
public class UserSettingsDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int FocusDuration { get; set; }
    public int ShortBreakDuration { get; set; }
    public int LongBreakDuration { get; set; }
    public bool AutoStartBreaks { get; set; }
    public string ThemeMode { get; set; } = "light";
    public bool EmailWeeklyReport { get; set; }
    public bool EmailDailyDigest { get; set; }
    public string? ReportEmail { get; set; }
    public string DailyDigestTime { get; set; } = "07:00";
    public string WeeklyReportTime { get; set; } = "08:00";
}

