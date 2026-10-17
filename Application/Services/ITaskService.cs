using be_pomo_do.Application.DTOs.Tasks;
using TaskStatus = be_pomo_do.Core.ValueObjects.TaskStatus;

namespace be_pomo_do.Application.Services;

// Service contract for managing task items, subtasks, and their associations.
public interface ITaskService
{
    // Retrieves a list of tasks for the user matching optional criteria.
    Task<List<TaskDto>> GetTasksAsync(Guid userId, TaskFilterParams filterParams);

    // Retrieves single task details by ID.
    Task<TaskDto> GetTaskByIdAsync(Guid taskId, Guid userId);

    // Creates a new task item for the user.
    Task<TaskDto> CreateTaskAsync(Guid userId, CreateTaskRequest request);

    // Updates an existing task item.
    Task<TaskDto> UpdateTaskAsync(Guid taskId, Guid userId, UpdateTaskRequest request);

    // Updates only the status of a task.
    Task<TaskDto> UpdateTaskStatusAsync(Guid taskId, Guid userId, TaskStatus status);

    // Soft deletes a task item (moves to trash).
    Task SoftDeleteTaskAsync(Guid taskId, Guid userId);

    // Permanently deletes a task item from database (hard delete).
    Task HardDeleteTaskAsync(Guid taskId, Guid userId);

    // Restores a soft-deleted task item from trash.
    Task<TaskDto> RestoreTaskAsync(Guid taskId, Guid userId);

    // Adds a new subtask to a parent task.
    Task<SubtaskDto> AddSubtaskAsync(Guid taskId, Guid userId, CreateSubtaskRequest request);

    // Toggles subtask completion status.
    Task<SubtaskDto> ToggleSubtaskAsync(Guid taskId, Guid subtaskId, Guid userId);

    // Updates a subtask's details or order index.
    Task<SubtaskDto> UpdateSubtaskAsync(Guid taskId, Guid subtaskId, Guid userId, UpdateSubtaskRequest request);

    // Permanently deletes a subtask.
    Task DeleteSubtaskAsync(Guid taskId, Guid subtaskId, Guid userId);
}
