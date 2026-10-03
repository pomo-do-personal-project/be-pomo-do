using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Pomodoro;

/// <summary>Request payload for successfully completing a focus session.</summary>
public class CompleteSessionRequest
{
    /// <summary>Actual total seconds spent on focus.</summary>
    [Range(0, 86400, ErrorMessage = "Thời gian thực tế phải từ 0 đến 86400 giây.")]
    public int ActualSeconds { get; set; }

    /// <summary>Extra overtime seconds worked beyond planned duration.</summary>
    [Range(0, 86400, ErrorMessage = "Thời gian overtime phải từ 0 đến 86400 giây.")]
    public int OvertimeSeconds { get; set; } = 0;

    /// <summary>Optional reflection note for this session.</summary>
    [MaxLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
    public string? Note { get; set; }
}
