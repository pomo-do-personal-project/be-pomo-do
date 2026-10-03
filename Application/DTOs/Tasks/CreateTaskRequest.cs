using System.ComponentModel.DataAnnotations;
using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Application.DTOs.Tasks;

/// <summary>Request payload for creating a new task.</summary>
public class CreateTaskRequest
{
    /// <summary>Title of the task.</summary>
    [Required(ErrorMessage = "Tiêu đề công việc không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tiêu đề công việc tối đa 255 ký tự.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Eisenhower priority matrix classification.</summary>
    public Priority Priority { get; set; } = Priority.ImportantNotUrgent;

    /// <summary>Estimated pomodoro intervals.</summary>
    [Range(1, 100, ErrorMessage = "Số phiên Pomodoro ước tính phải từ 1 đến 100.")]
    public int EstimatedPomodoros { get; set; } = 1;

    /// <summary>Estimated minutes.</summary>
    [Range(1, 3600, ErrorMessage = "Thời gian ước tính phải từ 1 đến 3600 phút.")]
    public int EstimatedMinutes { get; set; } = 25;

    /// <summary>Optional deadline date in UTC.</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Indicates whether this task was added rapidly during an active focus session (parked thought).</summary>
    public bool IsQuickAdd { get; set; } = false;

    /// <summary>Optional list of tag IDs to assign.</summary>
    public List<Guid>? TagIds { get; set; }

    /// <summary>Optional initial list of subtasks.</summary>
    public List<string>? InitialSubtasks { get; set; }
}
