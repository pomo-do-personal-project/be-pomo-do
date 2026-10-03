using System.ComponentModel.DataAnnotations;
using be_pomo_do.Core.ValueObjects;
using TaskStatus = be_pomo_do.Core.ValueObjects.TaskStatus;

namespace be_pomo_do.Application.DTOs.Tasks;

/// <summary>Request payload for updating an existing task.</summary>
public class UpdateTaskRequest
{
    /// <summary>Updated title of the task.</summary>
    [Required(ErrorMessage = "Tiêu đề công việc không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tiêu đề công việc tối đa 255 ký tự.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Updated priority classification.</summary>
    public Priority Priority { get; set; }

    /// <summary>Updated completion status.</summary>
    public TaskStatus Status { get; set; }

    /// <summary>Updated estimated pomodoros count.</summary>
    [Range(1, 100, ErrorMessage = "Số phiên Pomodoro ước tính phải từ 1 đến 100.")]
    public int EstimatedPomodoros { get; set; }

    /// <summary>Updated estimated minutes.</summary>
    [Range(1, 3600, ErrorMessage = "Thời gian ước tính phải từ 1 đến 3600 phút.")]
    public int EstimatedMinutes { get; set; }

    /// <summary>Updated deadline date in UTC.</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Updated list of tag IDs.</summary>
    public List<Guid>? TagIds { get; set; }
}
