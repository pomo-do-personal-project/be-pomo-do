using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Core.Entities;

public class FocusSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? TaskId { get; set; }
    public FocusSessionMode Mode { get; set; } = FocusSessionMode.Pomo;
    public int DurationMinutes { get; set; } = 25;
    public int ActualSeconds { get; set; } = 0;
    public int OvertimeSeconds { get; set; } = 0;
    public bool IsCompleted { get; set; } = true;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? Note { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public TaskItem? Task { get; set; }
}
