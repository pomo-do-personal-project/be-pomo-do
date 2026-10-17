using be_pomo_do.Application.DTOs.Analytics;

namespace be_pomo_do.Application.Common.Interfaces;

// Giao diện dịch vụ phân tích dữ liệu tập trung và bản đồ nhiệt năng suất
public interface IAnalyticsService
{
    // Lấy dữ liệu ma trận bản đồ nhiệt 7x24 và insight AI trong khoảng ngày chỉ định
    Task<FocusHeatmapResponse> GetFocusHeatmapAsync(Guid userId, int rangeDays = 30, CancellationToken ct = default);
}
