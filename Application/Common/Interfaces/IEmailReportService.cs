namespace be_pomo_do.Application.Common.Interfaces;

// Dịch vụ tổng hợp số liệu và điều phối gửi email báo cáo năng suất
public interface IEmailReportService
{
    // Gửi trực tiếp báo cáo tuần cho người dùng (hoặc email chỉ định)
    Task SendWeeklyReportAsync(Guid userId, string? overrideEmail = null, CancellationToken ct = default);

    // Gửi trực tiếp tóm tắt công việc đầu ngày cho người dùng (hoặc email chỉ định)
    Task SendDailyDigestAsync(Guid userId, string? overrideEmail = null, CancellationToken ct = default);

    // Quét các user bật nhận Daily Digest với khung giờ phù hợp để đẩy vào hàng đợi Redis Streams
    Task EnqueueScheduledDailyDigestsAsync(string currentTime, CancellationToken ct = default);

    // Quét các user bật nhận Weekly Report với khung giờ phù hợp để đẩy vào hàng đợi Redis Streams
    Task EnqueueScheduledWeeklyReportsAsync(string currentTime, CancellationToken ct = default);
}
