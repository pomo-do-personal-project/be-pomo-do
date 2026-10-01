using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Core.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public UserProvider Provider { get; set; } = UserProvider.Local;
    public string? ProviderId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public UserSettings? Settings { get; set; }
    public DailyStreak? Streak { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = [];
    public ICollection<Tag> Tags { get; set; } = [];
    public ICollection<FocusSession> FocusSessions { get; set; } = [];
}
