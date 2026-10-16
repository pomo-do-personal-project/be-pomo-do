using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Settings;

// DTO yêu cầu gửi thử nghiệm email báo cáo (Weekly hoặc Daily)
public class TestEmailReportRequest
{
    // Loại báo cáo cần gửi thử ("weekly" hoặc "daily")
    [Required(ErrorMessage = "Vui lòng chỉ định loại báo cáo cần gửi thử (weekly hoặc daily).")]
    public string ReportType { get; set; } = "weekly";

    // Hộp thư nhận email thử nghiệm (tùy chọn, mặc định lấy email tài khoản)
    [EmailAddress(ErrorMessage = "Email nhận không đúng định dạng.")]
    public string? TargetEmail { get; set; }
}
