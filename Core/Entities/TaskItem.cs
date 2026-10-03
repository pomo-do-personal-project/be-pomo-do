using be_pomo_do.Core.ValueObjects;
using TaskStatus = be_pomo_do.Core.ValueObjects.TaskStatus;

namespace be_pomo_do.Core.Entities;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Priority Priority { get; set; } = Priority.ImportantNotUrgent;
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public int EstimatedPomodoros { get; set; } = 1;
    public int EstimatedMinutes { get; set; } = 25;
    public DateTime? DueDate { get; set; }
    public bool IsQuickAdd { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ICollection<Subtask> Subtasks { get; set; } = [];
    public ICollection<TaskTag> TaskTags { get; set; } = [];
    public ICollection<FocusSession> FocusSessions { get; set; } = [];
}
