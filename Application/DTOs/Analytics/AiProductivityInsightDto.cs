namespace be_pomo_do.Application.DTOs.Analytics;

// DTO lưu trữ thông số phân tích nhịp sinh học và khung giờ vàng từ AI
public class AiProductivityInsightDto
{
    // Khung giờ vàng tập trung sâu nhất (vd: "08:30 - 11:00")
    public string GoldenHourWindow { get; set; } = "Chưa có dữ liệu";

    // Điểm tập trung ước tính trong khung giờ vàng (0-100%)
    public int GoldenHourScore { get; set; }

    // Khung giờ có nguy cơ xao nhãng hoặc hủy phiên cao nhất (vd: "14:00 - 15:30")
    public string VulnerableWindow { get; set; } = "Chưa có dữ liệu";

    // Nguyên nhân xao nhãng chính (vd: "Tỷ lệ hủy phiên cao hoặc mỏi mắt")
    public string VulnerableReason { get; set; } = string.Empty;

    // Hồ sơ nhịp sinh học cá nhân hóa (Morning Achiever / Night Owl / Balanced Sprinter)
    public string CircadianProfile { get; set; } = "Balanced";

    // Tổng số phiên phân tích
    public int TotalSessionsAnalyzed { get; set; }

    // Tỷ lệ phần trăm thời gian đạt trạng thái Deep Flow
    public int DeepFlowPercentage { get; set; }

    // Lời khuyên hành động thông minh từ AI
    public string ActionableTip { get; set; } = string.Empty;
}
