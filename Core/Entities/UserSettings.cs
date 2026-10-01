namespace be_pomo_do.Core.Entities;

public class UserSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public int FocusDuration { get; set; } = 25;
    public int ShortBreakDuration { get; set; } = 5;
    public int LongBreakDuration { get; set; } = 15;
    public bool AutoStartBreaks { get; set; } = false;
    public string ThemeMode { get; set; } = "light";

    // Navigation property
    public User? User { get; set; }
}
