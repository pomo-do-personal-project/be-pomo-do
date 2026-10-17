namespace be_pomo_do.Core.Entities;

public class AdminActionHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AdminId { get; set; }
    public Guid TargetUserId { get; set; }
    public string Action { get; set; } = string.Empty; // e.g., "SUSPEND_USER", "UNSUSPEND_USER"
    public string? Reason { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? AdminUser { get; set; }
    public User? TargetUser { get; set; }
}
