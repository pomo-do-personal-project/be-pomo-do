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

    // Admin & Account Status
    public bool IsAdmin { get; set; } = false;
    public bool IsSuspended { get; set; } = false;
    public string? SuspendedReason { get; set; }
    public DateTime? SuspendedAt { get; set; }

    // Navigation properties
    public UserSettings? Settings { get; set; }
    public DailyStreak? Streak { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = [];
    public ICollection<Tag> Tags { get; set; } = [];
    public ICollection<FocusSession> FocusSessions { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<AdminActionHistory> PerformedAdminActions { get; set; } = [];
    public ICollection<AdminActionHistory> ReceivedAdminActions { get; set; } = [];
    public ICollection<ScheduledNotification> ScheduledNotifications { get; set; } = [];
}
