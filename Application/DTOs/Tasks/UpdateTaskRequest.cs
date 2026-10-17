using System.ComponentModel.DataAnnotations;
using be_pomo_do.Core.ValueObjects;
using TaskStatus = be_pomo_do.Core.ValueObjects.TaskStatus;

namespace be_pomo_do.Application.DTOs.Tasks;

// Request payload for updating an existing task.
public class UpdateTaskRequest
{
    // Updated title of the task.
    [Required(ErrorMessage = "Tiêu đề công việc không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tiêu đề công việc tối đa 255 ký tự.")]
    public string Title { get; set; } = string.Empty;

    // Updated priority classification.
    public Priority Priority { get; set; }

    // Updated completion status.
    public TaskStatus Status { get; set; }

    // Updated estimated pomodoros count.
    [Range(1, 100, ErrorMessage = "Số phiên Pomodoro ước tính phải từ 1 đến 100.")]
    public int EstimatedPomodoros { get; set; }

    // Updated estimated minutes.
    [Range(1, 3600, ErrorMessage = "Thời gian ước tính phải từ 1 đến 3600 phút.")]
    public int EstimatedMinutes { get; set; }

    // Updated deadline date in UTC.
    public DateTime? DueDate { get; set; }

    // Indicates whether this task is marked as a quick-add item.
    public bool IsQuickAdd { get; set; }

    // Updated list of tag IDs.
    public List<Guid>? TagIds { get; set; }
}
