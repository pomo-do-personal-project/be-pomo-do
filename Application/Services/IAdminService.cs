using be_pomo_do.Application.DTOs.Admin;

namespace be_pomo_do.Application.Services;

// Giao diện nghiệp vụ điều hành và quản trị hệ thống Pomo-do
public interface IAdminService
{
    // Thống kê số liệu vận hành
    Task<AdminMetricsDto> GetMetricsAsync();

    // Kiểm tra sức khỏe hệ sinh thái dịch vụ
    Task<AdminSystemHealthDto> GetSystemHealthAsync();

    // Quản lý thông báo phát sóng hệ thống
    SystemBroadcastDto GetBroadcast();
    Task<SystemBroadcastDto> UpdateBroadcastAsync(Guid adminId, UpdateBroadcastRequest request);

    // Quản lý danh sách người dùng và trạng thái tài khoản
    Task<PagedResult<AdminUserSummaryDto>> GetUsersAsync(string? search, bool? isSuspended, int page, int pageSize);
    Task<AdminUserSummaryDto?> GetUserByIdAsync(Guid id);
    Task<string> UpdateUserStatusAsync(Guid adminId, Guid targetUserId, UpdateUserStatusRequest request);

    // Nhật ký kiểm toán và truyền thông
    Task<PagedResult<AdminAuditLogDto>> GetAuditLogsAsync(int page, int pageSize);
    Task<string> SendDirectEmailAsync(Guid adminId, AdminSendEmailRequest request);
    Task<List<AdminScheduledNotificationDto>> GetScheduledNotificationsAsync();
    Task<AdminScheduledNotificationDto> CreateScheduledNotificationAsync(Guid adminId, CreateScheduledNotificationRequest request);
    Task<bool> DeleteScheduledNotificationAsync(Guid id);
}
