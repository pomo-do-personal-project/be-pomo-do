using be_pomo_do.Core.ValueObjects;
using TaskStatus = be_pomo_do.Core.ValueObjects.TaskStatus;

namespace be_pomo_do.Application.DTOs.Tasks;

/// <summary>Query parameters for filtering, searching, and sorting tasks.</summary>
public class TaskFilterParams
{
    /// <summary>Filter tasks by execution status (Todo, InProgress, Completed, Cancelled).</summary>
    public TaskStatus? Status { get; set; }

    /// <summary>Filter tasks by Eisenhower matrix priority.</summary>
    public Priority? Priority { get; set; }

    /// <summary>Filter tasks associated with a specific tag.</summary>
    public Guid? TagId { get; set; }

    /// <summary>Search keyword for task title.</summary>
    public string? Search { get; set; }

    /// <summary>If true, queries only soft-deleted tasks in the trash bin.</summary>
    public bool InTrash { get; set; } = false;

    /// <summary>Sort column: "createdAt", "dueDate", "priority", "title". Defaults to "createdAt".</summary>
    public string? SortBy { get; set; } = "createdAt";

    /// <summary>Sort direction: true for descending, false for ascending. Defaults to true.</summary>
    public bool IsDescending { get; set; } = true;
}
