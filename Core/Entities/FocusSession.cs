using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Core.Entities;

public class FocusSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? TaskId { get; set; }
    public FocusSessionMode Mode { get; set; } = FocusSessionMode.Pomo;
    public int OvertimeSeconds { get; set; } = 0;

    // Navigation properties
    public User? User { get; set; }
    public TaskItem? Task { get; set; }
}
