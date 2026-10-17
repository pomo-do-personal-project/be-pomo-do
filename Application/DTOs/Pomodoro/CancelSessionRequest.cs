using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Pomodoro;

// Request payload for canceling or early stopping a focus session.
public class CancelSessionRequest
{
    // Actual seconds elapsed before canceling.
    [Range(0, 86400, ErrorMessage = "Thời gian thực tế phải từ 0 đến 86400 giây.")]
    public int ActualSeconds { get; set; } = 0;
}
