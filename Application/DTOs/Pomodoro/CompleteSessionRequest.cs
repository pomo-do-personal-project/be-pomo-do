using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Pomodoro;

// Request payload for successfully completing a focus session.
public class CompleteSessionRequest
{
    // Actual total seconds spent on focus.
    [Range(0, 86400, ErrorMessage = "Thời gian thực tế phải từ 0 đến 86400 giây.")]
    public int ActualSeconds { get; set; }

    // Extra overtime seconds worked beyond planned duration.
    [Range(0, 86400, ErrorMessage = "Thời gian overtime phải từ 0 đến 86400 giây.")]
    public int OvertimeSeconds { get; set; } = 0;

    // Optional reflection note for this session.
    [MaxLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
    public string? Note { get; set; }

    // Optional flag to mark the associated task as completed.
    public bool? IsTaskCompleted { get; set; }
}
