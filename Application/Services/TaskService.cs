using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.Common.Mappings;
using be_pomo_do.Application.DTOs.Tasks;
using be_pomo_do.Core.Entities;
using be_pomo_do.Core.Exceptions;
using be_pomo_do.Core.ValueObjects;
using TaskStatus = be_pomo_do.Core.ValueObjects.TaskStatus;

namespace be_pomo_do.Application.Services;

/// <summary>Application service implementing task and subtask business operations.</summary>
public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
    }

    /// <inheritdoc/>
    public async Task<List<TaskDto>> GetTasksAsync(Guid userId, TaskFilterParams filterParams)
    {
        var tasks = await _taskRepository.GetUserTasksAsync(userId, filterParams);
        return tasks.Select(t => t.ToDto()).ToList();
    }

    /// <inheritdoc/>
    public async Task<TaskDto> GetTaskByIdAsync(Guid taskId, Guid userId)
    {
        var task = await _taskRepository.GetTaskByIdAsync(taskId, userId, includeDeleted: false);
        if (task == null)
        {
            throw new DomainException("Công việc không tồn tại hoặc bạn không có quyền truy cập.");
        }
        return task.ToDto();
    }

    /// <inheritdoc/>
    public async Task<TaskDto> CreateTaskAsync(Guid userId, CreateTaskRequest request)
    {
        var task = new TaskItem
        {
            UserId = userId,
            Title = request.Title.Trim(),
            Priority = request.Priority,
            Status = TaskStatus.Todo,
            EstimatedPomodoros = request.EstimatedPomodoros,
            EstimatedMinutes = request.EstimatedMinutes,
            DueDate = request.DueDate,
            IsQuickAdd = request.IsQuickAdd,
            CreatedAt = DateTime.UtcNow
        };

        if (request.InitialSubtasks != null && request.InitialSubtasks.Count > 0)
        {
            int order = 0;
            foreach (var subtaskTitle in request.InitialSubtasks.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                task.Subtasks.Add(new Subtask
                {
                    TaskId = task.Id,
                    Title = subtaskTitle.Trim(),
                    OrderIndex = order++,
                    IsCompleted = false
                });
            }
        }

        if (request.TagIds != null && request.TagIds.Count > 0)
        {
            var validTags = await _taskRepository.GetUserTagsByIdsAsync(userId, request.TagIds);
            foreach (var tag in validTags)
            {
                task.TaskTags.Add(new TaskTag { TaskId = task.Id, TagId = tag.Id, Tag = tag });
            }
        }

        await _taskRepository.AddTaskAsync(task);
        await _taskRepository.SaveChangesAsync();
        return task.ToDto();
    }

    /// <inheritdoc/>
    public async Task<TaskDto> UpdateTaskAsync(Guid taskId, Guid userId, UpdateTaskRequest request)
    {
        var task = await _taskRepository.GetTaskByIdAsync(taskId, userId, includeDeleted: false);
        if (task == null)
        {
            throw new DomainException("Công việc không tồn tại hoặc bạn không có quyền chỉnh sửa.");
        }

        task.Title = request.Title.Trim();
        task.Priority = request.Priority;
        task.Status = request.Status;
        task.EstimatedPomodoros = request.EstimatedPomodoros;
        task.EstimatedMinutes = request.EstimatedMinutes;
        task.DueDate = request.DueDate;
        task.IsQuickAdd = request.IsQuickAdd;
        task.UpdatedAt = DateTime.UtcNow;

        if (request.TagIds != null)
        {
            task.TaskTags.Clear();
            var validTags = await _taskRepository.GetUserTagsByIdsAsync(userId, request.TagIds);
            foreach (var tag in validTags)
            {
                task.TaskTags.Add(new TaskTag { TaskId = task.Id, TagId = tag.Id, Tag = tag });
            }
        }

        await _taskRepository.SaveChangesAsync();
        return task.ToDto();
    }

    /// <inheritdoc/>
    public async Task<TaskDto> UpdateTaskStatusAsync(Guid taskId, Guid userId, TaskStatus status)
    {
        var task = await _taskRepository.GetTaskByIdAsync(taskId, userId, includeDeleted: false);
        if (task == null)
        {
            throw new DomainException("Công việc không tồn tại hoặc bạn không có quyền chỉnh sửa.");
        }

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.SaveChangesAsync();
        return task.ToDto();
    }

    /// <inheritdoc/>
    public async Task SoftDeleteTaskAsync(Guid taskId, Guid userId)
    {
        var task = await _taskRepository.GetTaskByIdAsync(taskId, userId, includeDeleted: false);
        if (task == null)
        {
            throw new DomainException("Công việc không tồn tại hoặc đã bị xóa.");
        }

        task.IsDeleted = true;
        task.DeletedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task HardDeleteTaskAsync(Guid taskId, Guid userId)
    {
        var task = await _taskRepository.GetTaskByIdAsync(taskId, userId, includeDeleted: true);
        if (task == null)
        {
            throw new DomainException("Công việc không tồn tại hoặc bạn không có quyền xóa.");
        }

        _taskRepository.RemoveTask(task);
        await _taskRepository.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<TaskDto> RestoreTaskAsync(Guid taskId, Guid userId)
    {
        var task = await _taskRepository.GetTaskByIdAsync(taskId, userId, includeDeleted: true);
        if (task == null || !task.IsDeleted)
        {
            throw new DomainException("Không tìm thấy công việc này trong thùng rác để khôi phục.");
        }

        task.IsDeleted = false;
        task.DeletedAt = null;
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.SaveChangesAsync();
        return task.ToDto();
    }

    /// <inheritdoc/>
    public async Task<SubtaskDto> AddSubtaskAsync(Guid taskId, Guid userId, CreateSubtaskRequest request)
    {
        var task = await _taskRepository.GetTaskByIdAsync(taskId, userId, includeDeleted: false)
            ?? throw new DomainException("Công việc cha không tồn tại hoặc bạn không có quyền thêm subtask.");

        var subtask = new Subtask
        {
            TaskId = taskId,
            Title = request.Title.Trim(),
            OrderIndex = request.OrderIndex,
            IsCompleted = false
        };

        await _taskRepository.AddSubtaskAsync(subtask);
        await _taskRepository.SaveChangesAsync();
        return subtask.ToDto();
    }

    /// <inheritdoc/>
    public async Task<SubtaskDto> ToggleSubtaskAsync(Guid taskId, Guid subtaskId, Guid userId)
    {
        var task = await _taskRepository.GetTaskByIdAsync(taskId, userId, includeDeleted: false)
            ?? throw new DomainException("Công việc không tồn tại hoặc bạn không có quyền thao tác.");

        var subtask = await _taskRepository.GetSubtaskByIdAsync(subtaskId, taskId)
            ?? throw new DomainException("Subtask không tồn tại trong công việc này.");

        subtask.IsCompleted = !subtask.IsCompleted;
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.SaveChangesAsync();
        return subtask.ToDto();
    }

    /// <inheritdoc/>
    public async Task<SubtaskDto> UpdateSubtaskAsync(Guid taskId, Guid subtaskId, Guid userId, UpdateSubtaskRequest request)
    {
        var task = await _taskRepository.GetTaskByIdAsync(taskId, userId, includeDeleted: false)
            ?? throw new DomainException("Công việc không tồn tại hoặc bạn không có quyền thao tác.");

        var subtask = await _taskRepository.GetSubtaskByIdAsync(subtaskId, taskId)
            ?? throw new DomainException("Subtask không tồn tại trong công việc này.");

        subtask.Title = request.Title.Trim();
        subtask.IsCompleted = request.IsCompleted;
        subtask.OrderIndex = request.OrderIndex;
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.SaveChangesAsync();
        return subtask.ToDto();
    }

    /// <inheritdoc/>
    public async Task DeleteSubtaskAsync(Guid taskId, Guid subtaskId, Guid userId)
    {
        var task = await _taskRepository.GetTaskByIdAsync(taskId, userId, includeDeleted: false)
            ?? throw new DomainException("Công việc không tồn tại hoặc bạn không có quyền thao tác.");

        var subtask = await _taskRepository.GetSubtaskByIdAsync(subtaskId, taskId)
            ?? throw new DomainException("Subtask không tồn tại trong công việc này.");

        _taskRepository.RemoveSubtask(subtask);
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.SaveChangesAsync();
    }
}
