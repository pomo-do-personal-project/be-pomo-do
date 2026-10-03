namespace be_pomo_do.Application.DTOs.Tasks;

/// <summary>Represents a subtask item within a task.</summary>
public class SubtaskDto
{
    /// <summary>Unique identifier of the subtask.</summary>
    public Guid Id { get; set; }

    /// <summary>Identifier of the parent task.</summary>
    public Guid TaskId { get; set; }

    /// <summary>Title or description of the subtask.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Indicates whether the subtask is completed.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Ordering display index of the subtask.</summary>
    public int OrderIndex { get; set; }
}
