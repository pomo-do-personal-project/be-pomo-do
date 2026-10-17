using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Settings;

// Request payload for updating user preferences and pomodoro durations.
public class UpdateUserSettingsRequest
{
    // Thời lượng phiên tập trung (phút), từ 1 đến 180 phút.
    [Range(1, 180, ErrorMessage = "Thời lượng tập trung phải từ 1 đến 180 phút.")]
    public int FocusDuration { get; set; } = 25;

    // Thời lượng nghỉ ngắn (phút), từ 1 đến 60 phút.
    [Range(1, 60, ErrorMessage = "Thời lượng nghỉ ngắn phải từ 1 đến 60 phút.")]
    public int ShortBreakDuration { get; set; } = 5;

    // Thời lượng nghỉ dài (phút), từ 1 đến 60 phút.
    [Range(1, 60, ErrorMessage = "Thời lượng nghỉ dài phải từ 1 đến 60 phút.")]
    public int LongBreakDuration { get; set; } = 15;

    // Tự động chuyển tiếp sang phiên nghỉ.
    public bool AutoStartBreaks { get; set; } = false;

    // Cấu hình giao diện (light, dark, system)
    [MaxLength(50)]
    public string ThemeMode { get; set; } = "light";

    // Nhận báo cáo năng suất tuần qua email vào sáng Thứ Hai
    public bool EmailWeeklyReport { get; set; } = false;

    // Nhận email tóm tắt công việc đầu ngày lúc 07:00 sáng
    public bool EmailDailyDigest { get; set; } = false;

    // Hộp thư nhận báo cáo (nếu để trống sẽ dùng email tài khoản)
    [EmailAddress(ErrorMessage = "Email nhận báo cáo không đúng định dạng.")]
    [MaxLength(255)]
    public string? ReportEmail { get; set; }

    // Khung giờ gửi tóm tắt công việc đầu ngày (định dạng HH:mm, ví dụ 07:00)
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Khung giờ gửi tóm tắt không hợp lệ (định dạng HH:mm).")]
    public string? DailyDigestTime { get; set; }

    // Khung giờ gửi báo cáo tuần vào sáng Thứ Hai (định dạng HH:mm, ví dụ 08:00)
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Khung giờ gửi báo cáo tuần không hợp lệ (định dạng HH:mm).")]
    public string? WeeklyReportTime { get; set; }
}

