using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Pomodoro;

/// <summary>Request payload for canceling or early stopping a focus session.</summary>
public class CancelSessionRequest
{
    /// <summary>Actual seconds elapsed before canceling.</summary>
    [Range(0, 86400, ErrorMessage = "Thời gian thực tế phải từ 0 đến 86400 giây.")]
    public int ActualSeconds { get; set; } = 0;
}
