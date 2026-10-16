namespace be_pomo_do.Application.Common.Interfaces;

// Hàng đợi phân tán gửi email báo cáo qua Redis Streams
public interface IEmailReportQueue
{
    // Đẩy yêu cầu gửi email báo cáo vào Redis Stream (XADD)
    Task EnqueueReportAsync(Guid userId, string reportType, string? targetEmail = null, CancellationToken ct = default);
}
