using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Settings;

/// <summary>Request payload for updating user preferences and pomodoro durations.</summary>
public class UpdateUserSettingsRequest
{
    /// <summary>Thời lượng phiên tập trung (phút), từ 1 đến 180 phút.</summary>
    [Range(1, 180, ErrorMessage = "Thời lượng tập trung phải từ 1 đến 180 phút.")]
    public int FocusDuration { get; set; } = 25;

    /// <summary>Thời lượng nghỉ ngắn (phút), từ 1 đến 60 phút.</summary>
    [Range(1, 60, ErrorMessage = "Thời lượng nghỉ ngắn phải từ 1 đến 60 phút.")]
    public int ShortBreakDuration { get; set; } = 5;

    /// <summary>Thời lượng nghỉ dài (phút), từ 1 đến 60 phút.</summary>
    [Range(1, 60, ErrorMessage = "Thời lượng nghỉ dài phải từ 1 đến 60 phút.")]
    public int LongBreakDuration { get; set; } = 15;

    /// <summary>Tự động chuyển tiếp sang phiên nghỉ.</summary>
    public bool AutoStartBreaks { get; set; } = false;

    /// <summary>Chế độ giao diện (light, dark, system).</summary>
    [MaxLength(50)]
    public string ThemeMode { get; set; } = "light";
}
