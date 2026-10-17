using be_pomo_do.Core.ValueObjects;
using TaskStatus = be_pomo_do.Core.ValueObjects.TaskStatus;

namespace be_pomo_do.Application.DTOs.Tasks;

// Query parameters for filtering, searching, and sorting tasks.
public class TaskFilterParams
{
    // Filter tasks by execution status (Todo, InProgress, Completed, Cancelled).
    public TaskStatus? Status { get; set; }

    // Filter tasks by Eisenhower matrix priority.
    public Priority? Priority { get; set; }

    // Filter tasks associated with a specific tag.
    public Guid? TagId { get; set; }

    // Filter tasks by whether they were quick-added during an active session.
    public bool? IsQuickAdd { get; set; }

    // Search keyword for task title.
    public string? Search { get; set; }

    // If true, queries only soft-deleted tasks in the trash bin.
    public bool InTrash { get; set; } = false;

    // Sort column: "createdAt", "dueDate", "priority", "title". Defaults to "createdAt".
    public string? SortBy { get; set; } = "createdAt";

    // Sort direction: true for descending, false for ascending. Defaults to true.
    public bool IsDescending { get; set; } = true;
}
