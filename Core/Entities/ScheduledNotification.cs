namespace be_pomo_do.Core.Entities;

public class ScheduledNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AdminId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "info";
    public string Scope { get; set; } = "direct";
    public string? RecipientEmails { get; set; }
    public DateTime ScheduledAt { get; set; }
    public bool IsSent { get; set; } = false;
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User? AdminUser { get; set; }
}
