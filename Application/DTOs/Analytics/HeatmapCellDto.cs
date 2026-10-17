namespace be_pomo_do.Application.DTOs.Analytics;

// DTO đại diện cho 1 ô trên ma trận bản đồ nhiệt năng suất 7x24
public class HeatmapCellDto
{
    // Thứ trong tuần (0: Thứ Hai, 1: Thứ Ba, ..., 6: Chủ Nhật)
    public int DayOfWeek { get; set; }

    // Tên thứ hiển thị rút gọn (T2, T3, ..., CN)
    public string DayName { get; set; } = string.Empty;

    // Khung giờ trong ngày (0 đến 23)
    public int Hour { get; set; }

    // Nhãn khung giờ (vd: "09:00 - 10:00")
    public string HourLabel { get; set; } = string.Empty;

    // Tổng số phút tập trung tích lũy trong khung giờ này
    public int TotalMinutes { get; set; }

    // Số phiên Pomodoro hoàn thành trọn vẹn
    public int CompletedCount { get; set; }

    // Số phiên bị hủy giữa chừng
    public int CancelledCount { get; set; }

    // Điểm tập trung AI trung bình từ camera (0-100%)
    public int AverageAiScore { get; set; }

    // Cấp độ nhiệt (0: Trống, 1: Nhẹ, 2: Khá, 3: Sâu, 4: Đỉnh cao)
    public int Level { get; set; }
}
