using be_pomo_do.Core.ValueObjects;
using TaskStatus = be_pomo_do.Core.ValueObjects.TaskStatus;

namespace be_pomo_do.Application.DTOs.Tasks;

/// <summary>Represents a full task item payload including subtasks and assigned tags.</summary>
public class TaskDto
{
    /// <summary>Unique identifier of the task.</summary>
    public Guid Id { get; set; }

    /// <summary>Identifier of the user owning the task.</summary>
    public Guid UserId { get; set; }

    /// <summary>Title of the task.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Eisenhower matrix priority level.</summary>
    public Priority Priority { get; set; }

    /// <summary>Current completion status.</summary>
    public TaskStatus Status { get; set; }

    /// <summary>Estimated pomodoro intervals planned for this task.</summary>
    public int EstimatedPomodoros { get; set; }

    /// <summary>Total estimated minutes planned for this task.</summary>
    public int EstimatedMinutes { get; set; }

    /// <summary>Total completed Pomodoro sessions for this task.</summary>
    public int CompletedPomodoros { get; set; }

    /// <summary>Optional deadline date and time in UTC.</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Creation timestamp in UTC.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Last updated timestamp in UTC.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>List of child subtasks.</summary>
    public List<SubtaskDto> Subtasks { get; set; } = [];

    /// <summary>List of tags associated with this task.</summary>
    public List<TagDto> Tags { get; set; } = [];
}
