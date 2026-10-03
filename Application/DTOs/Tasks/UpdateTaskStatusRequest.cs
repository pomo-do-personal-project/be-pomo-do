using System.ComponentModel.DataAnnotations;
using TaskStatus = be_pomo_do.Core.ValueObjects.TaskStatus;

namespace be_pomo_do.Application.DTOs.Tasks;

/// <summary>Request payload for quick status change of a task.</summary>
public class UpdateTaskStatusRequest
{
    /// <summary>New status for the task.</summary>
    [Required]
    public TaskStatus Status { get; set; }
}
