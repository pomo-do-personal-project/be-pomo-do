using be_pomo_do.Application.DTOs.Tasks;
using TaskStatus = be_pomo_do.Core.ValueObjects.TaskStatus;

namespace be_pomo_do.Application.Services;

/// <summary>Service contract for managing task items, subtasks, and their associations.</summary>
public interface ITaskService
{
    /// <summary>Retrieves a list of tasks for the user matching optional criteria.</summary>
    Task<List<TaskDto>> GetTasksAsync(Guid userId, TaskFilterParams filterParams, CancellationToken ct = default);

    /// <summary>Retrieves single task details by ID.</summary>
    Task<TaskDto> GetTaskByIdAsync(Guid taskId, Guid userId, CancellationToken ct = default);

    /// <summary>Creates a new task item for the user.</summary>
    Task<TaskDto> CreateTaskAsync(Guid userId, CreateTaskRequest request, CancellationToken ct = default);

    /// <summary>Updates an existing task item.</summary>
    Task<TaskDto> UpdateTaskAsync(Guid taskId, Guid userId, UpdateTaskRequest request, CancellationToken ct = default);

    /// <summary>Updates only the status of a task.</summary>
    Task<TaskDto> UpdateTaskStatusAsync(Guid taskId, Guid userId, TaskStatus status, CancellationToken ct = default);

    /// <summary>Soft deletes a task item (moves to trash).</summary>
    Task SoftDeleteTaskAsync(Guid taskId, Guid userId, CancellationToken ct = default);

    /// <summary>Restores a soft-deleted task item from trash.</summary>
    Task<TaskDto> RestoreTaskAsync(Guid taskId, Guid userId, CancellationToken ct = default);

    /// <summary>Adds a new subtask to a parent task.</summary>
    Task<SubtaskDto> AddSubtaskAsync(Guid taskId, Guid userId, CreateSubtaskRequest request, CancellationToken ct = default);

    /// <summary>Toggles subtask completion status.</summary>
    Task<SubtaskDto> ToggleSubtaskAsync(Guid taskId, Guid subtaskId, Guid userId, CancellationToken ct = default);

    /// <summary>Updates a subtask's details or order index.</summary>
    Task<SubtaskDto> UpdateSubtaskAsync(Guid taskId, Guid subtaskId, Guid userId, UpdateSubtaskRequest request, CancellationToken ct = default);

    /// <summary>Permanently deletes a subtask.</summary>
    Task DeleteSubtaskAsync(Guid taskId, Guid subtaskId, Guid userId, CancellationToken ct = default);
}
