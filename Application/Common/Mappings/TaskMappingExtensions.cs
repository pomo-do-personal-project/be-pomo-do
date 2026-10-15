using be_pomo_do.Application.DTOs.Tasks;
using be_pomo_do.Core.Entities;
using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Application.Common.Mappings;

/// <summary>Extension methods for mapping Task and Subtask entities to DTOs.</summary>
public static class TaskMappingExtensions
{
    // Chuyển đổi TaskItem entity sang TaskDto
    public static TaskDto ToDto(this TaskItem task)
    {
        return new TaskDto
        {
            Id = task.Id,
            UserId = task.UserId,
            Title = task.Title,
            Priority = task.Priority,
            Status = task.Status,
            EstimatedPomodoros = task.EstimatedPomodoros,
            EstimatedMinutes = task.EstimatedMinutes,
            CompletedPomodoros = task.FocusSessions.Count(f => f.Mode == FocusSessionMode.Pomo),
            DueDate = task.DueDate,
            IsQuickAdd = task.IsQuickAdd,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            Subtasks = task.Subtasks.Select(st => st.ToDto()).ToList(),
            Tags = task.TaskTags
                .Where(tt => tt.Tag != null)
                .Select(tt => new TagDto
                {
                    Id = tt.Tag!.Id,
                    Name = tt.Tag.Name,
                    ColorHex = tt.Tag.ColorHex
                }).ToList()
        };
    }

    // Chuyển đổi Subtask entity sang SubtaskDto
    public static SubtaskDto ToDto(this Subtask subtask)
    {
        return new SubtaskDto
        {
            Id = subtask.Id,
            TaskId = subtask.TaskId,
            Title = subtask.Title,
            IsCompleted = subtask.IsCompleted,
            OrderIndex = subtask.OrderIndex
        };
    }
}
