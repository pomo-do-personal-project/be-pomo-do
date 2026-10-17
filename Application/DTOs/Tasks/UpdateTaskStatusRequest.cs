using System.ComponentModel.DataAnnotations;
using TaskStatus = be_pomo_do.Core.ValueObjects.TaskStatus;

namespace be_pomo_do.Application.DTOs.Tasks;

// Request payload for quick status change of a task.
public class UpdateTaskStatusRequest
{
    // New status for the task.
    [Required]
    public TaskStatus Status { get; set; }
}
