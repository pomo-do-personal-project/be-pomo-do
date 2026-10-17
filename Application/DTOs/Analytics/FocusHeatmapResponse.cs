namespace be_pomo_do.Application.DTOs.Analytics;

// Response payload cho endpoint lấy bản đồ nhiệt năng suất và phân tích AI
public class FocusHeatmapResponse
{
    // Ma trận 168 ô nhiệt (7 ngày x 24 giờ)
    public List<HeatmapCellDto> Cells { get; set; } = new();

    // Thông tin phân tích thông minh từ AI
    public AiProductivityInsightDto Insight { get; set; } = new();

    // Mốc thời gian bắt đầu khoảng lọc (UTC)
    public DateTime FromUtc { get; set; }

    // Mốc thời gian kết thúc khoảng lọc (UTC)
    public DateTime ToUtc { get; set; }

    // Tổng số phút tập trung trong toàn bộ khoảng thời gian
    public int TotalFocusMinutes { get; set; }

    // Tổng số phiên hoàn thành
    public int TotalCompletedSessions { get; set; }
}
