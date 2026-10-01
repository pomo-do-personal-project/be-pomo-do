namespace be_pomo_do.Core.Entities;

public class DailyStreak
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public int CurrentStreak { get; set; } = 0;
    public int LongestStreak { get; set; } = 0;
    public DateOnly? LastActiveDate { get; set; }

    // Navigation property
    public User? User { get; set; }
}
