using System.ComponentModel.DataAnnotations;
using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Application.DTOs.Tasks;

// Request payload for creating a new task.
public class CreateTaskRequest
{
    // Title of the task.
    [Required(ErrorMessage = "Tiêu đề công việc không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tiêu đề công việc tối đa 255 ký tự.")]
    public string Title { get; set; } = string.Empty;

    // Eisenhower priority matrix classification.
    public Priority Priority { get; set; } = Priority.ImportantNotUrgent;

    // Estimated pomodoro intervals.
    [Range(1, 100, ErrorMessage = "Số phiên Pomodoro ước tính phải từ 1 đến 100.")]
    public int EstimatedPomodoros { get; set; } = 1;

    // Estimated minutes.
    [Range(1, 3600, ErrorMessage = "Thời gian ước tính phải từ 1 đến 3600 phút.")]
    public int EstimatedMinutes { get; set; } = 25;

    // Optional deadline date in UTC.
    public DateTime? DueDate { get; set; }

    // Indicates whether this task was added rapidly during an active focus session (parked thought).
    public bool IsQuickAdd { get; set; } = false;

    // Optional list of tag IDs to assign.
    public List<Guid>? TagIds { get; set; }

    // Optional initial list of subtasks.
    public List<string>? InitialSubtasks { get; set; }
}
