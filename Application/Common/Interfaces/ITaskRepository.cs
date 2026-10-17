using be_pomo_do.Application.DTOs.Tasks;
using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Common.Interfaces;

// Repository interface for Task and Subtask database operations.
public interface ITaskRepository
{
    // Retrieves filtered tasks belonging to the specified user.
    Task<List<TaskItem>> GetUserTasksAsync(Guid userId, TaskFilterParams filterParams);

    // Retrieves a single task with tags, subtasks, and completed pomodoros by ID and User ID.
    Task<TaskItem?> GetTaskByIdAsync(Guid taskId, Guid userId, bool includeDeleted = false);

    // Adds a new task to persistence.
    Task AddTaskAsync(TaskItem task);

    // Retrieves a subtask by ID within a specific task.
    Task<Subtask?> GetSubtaskByIdAsync(Guid subtaskId, Guid taskId);

    // Adds a subtask to persistence.
    Task AddSubtaskAsync(Subtask subtask);

    // Removes a subtask permanently.
    void RemoveSubtask(Subtask subtask);

    // Removes a task permanently (hard delete).
    void RemoveTask(TaskItem task);

    // Retrieves valid tags owned by the user matching the given IDs.
    Task<List<Tag>> GetUserTagsByIdsAsync(Guid userId, List<Guid> tagIds);

    // Persists all tracked changes to the database.
    Task SaveChangesAsync();
}
