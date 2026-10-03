namespace be_pomo_do.Application.DTOs.Settings;

/// <summary>Data transfer object for user settings preferences.</summary>
public class UserSettingsDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int FocusDuration { get; set; }
    public int ShortBreakDuration { get; set; }
    public int LongBreakDuration { get; set; }
    public bool AutoStartBreaks { get; set; }
    public string ThemeMode { get; set; } = "light";
}
