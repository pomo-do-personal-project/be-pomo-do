using be_pomo_do.Application.DTOs.Tasks;
using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Common.Interfaces;

/// <summary>Repository interface for Task and Subtask database operations.</summary>
public interface ITaskRepository
{
    /// <summary>Retrieves filtered tasks belonging to the specified user.</summary>
    Task<List<TaskItem>> GetUserTasksAsync(Guid userId, TaskFilterParams filterParams);

    /// <summary>Retrieves a single task with tags, subtasks, and completed pomodoros by ID and User ID.</summary>
    Task<TaskItem?> GetTaskByIdAsync(Guid taskId, Guid userId, bool includeDeleted = false);

    /// <summary>Adds a new task to persistence.</summary>
    Task AddTaskAsync(TaskItem task);

    /// <summary>Retrieves a subtask by ID within a specific task.</summary>
    Task<Subtask?> GetSubtaskByIdAsync(Guid subtaskId, Guid taskId);

    /// <summary>Adds a subtask to persistence.</summary>
    Task AddSubtaskAsync(Subtask subtask);

    /// <summary>Removes a subtask permanently.</summary>
    void RemoveSubtask(Subtask subtask);

    /// <summary>Removes a task permanently (hard delete).</summary>
    void RemoveTask(TaskItem task);

    /// <summary>Retrieves valid tags owned by the user matching the given IDs.</summary>
    Task<List<Tag>> GetUserTagsByIdsAsync(Guid userId, List<Guid> tagIds);

    /// <summary>Persists all tracked changes to the database.</summary>
    Task SaveChangesAsync();
}
