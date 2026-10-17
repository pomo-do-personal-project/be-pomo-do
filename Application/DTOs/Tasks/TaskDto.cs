using be_pomo_do.Core.ValueObjects;
using TaskStatus = be_pomo_do.Core.ValueObjects.TaskStatus;

namespace be_pomo_do.Application.DTOs.Tasks;

// Represents a full task item payload including subtasks and assigned tags.
public class TaskDto
{
    // Unique identifier of the task.
    public Guid Id { get; set; }

    // Identifier of the user owning the task.
    public Guid UserId { get; set; }

    // Title of the task.
    public string Title { get; set; } = string.Empty;

    // Eisenhower matrix priority level.
    public Priority Priority { get; set; }

    // Current completion status.
    public TaskStatus Status { get; set; }

    // Estimated pomodoro intervals planned for this task.
    public int EstimatedPomodoros { get; set; }

    // Total estimated minutes planned for this task.
    public int EstimatedMinutes { get; set; }

    // Total completed Pomodoro sessions for this task.
    public int CompletedPomodoros { get; set; }

    // Optional deadline date and time in UTC.
    public DateTime? DueDate { get; set; }

    // Indicates whether this task was created via quick-add during a focus session.
    public bool IsQuickAdd { get; set; }

    // Creation timestamp in UTC.
    public DateTime CreatedAt { get; set; }

    // Last updated timestamp in UTC.
    public DateTime? UpdatedAt { get; set; }

    // List of child subtasks.
    public List<SubtaskDto> Subtasks { get; set; } = [];

    // List of tags associated with this task.
    public List<TagDto> Tags { get; set; } = [];
}
