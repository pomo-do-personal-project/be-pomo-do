namespace be_pomo_do.Application.DTOs.Tasks;

// Represents a subtask item within a task.
public class SubtaskDto
{
    // Unique identifier of the subtask.
    public Guid Id { get; set; }

    // Identifier of the parent task.
    public Guid TaskId { get; set; }

    // Title or description of the subtask.
    public string Title { get; set; } = string.Empty;

    // Indicates whether the subtask is completed.
    public bool IsCompleted { get; set; }

    // Ordering display index of the subtask.
    public int OrderIndex { get; set; }
}
