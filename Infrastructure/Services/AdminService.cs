using System.Diagnostics;
using be_pomo_do.Application.Common;
using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.DTOs.Admin;
using be_pomo_do.Application.Services;
using be_pomo_do.Core.Entities;
using be_pomo_do.Core.Exceptions;
using be_pomo_do.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace be_pomo_do.Infrastructure.Services;

// Dịch vụ quản trị hệ thống, xử lý nghiệp vụ admin và tương tác cơ sở dữ liệu tách biệt khỏi Presentation Layer
public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly IConnectionMultiplexer? _redis;
    private readonly IConfiguration _configuration;
    private readonly ScheduledNotificationSignal _signal;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(2) };

    public AdminService(
        AppDbContext context,
        IConfiguration configuration,
        ScheduledNotificationSignal signal,
        IServiceScopeFactory serviceScopeFactory,
        IServiceProvider serviceProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _signal = signal ?? throw new ArgumentNullException(nameof(signal));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _redis = serviceProvider.GetService<IConnectionMultiplexer>();
    }

    // Lấy số liệu vận hành tổng quan cho Admin Dashboard
    public async Task<AdminMetricsDto> GetMetricsAsync()
    {
        var totalUsers = await _context.Users.CountAsync(u => !u.IsAdmin);
        var suspendedUsers = await _context.Users.CountAsync(u => !u.IsAdmin && u.IsSuspended);

        var todayUtc = DateTime.UtcNow.Date;
        var activeUsersToday = await _context.FocusSessions
            .Where(f => f.StartedAt >= todayUtc)
            .Select(f => f.UserId)
            .Distinct()
            .CountAsync();

        var activeRate = totalUsers > 0
            ? Math.Round(((double)activeUsersToday / totalUsers) * 100.0, 1)
            : 0.0;

        var googleUsers = await _context.Users.CountAsync(u => u.Provider == Core.ValueObjects.UserProvider.Google);
        var localUsers = await _context.Users.CountAsync(u => u.Provider == Core.ValueObjects.UserProvider.Local);

        return new AdminMetricsDto(
            TotalUsers: totalUsers,
            ActiveUsersToday: activeUsersToday,
            SuspendedUsers: suspendedUsers,
            ActiveRatePercentage: activeRate,
            GoogleUsersCount: googleUsers,
            LocalUsersCount: localUsers
        );
    }

    // Kiểm tra sức khỏe và độ trễ của các dịch vụ hạ tầng
    public async Task<AdminSystemHealthDto> GetSystemHealthAsync()
    {
        var apiStatus = new ServiceStatusDto("Backend API", "healthy", 1, "ASP.NET Core (.NET 10.0)");

        ServiceStatusDto dbStatus;
        try
        {
            var sw = Stopwatch.StartNew();
            var canConnect = await _context.Database.CanConnectAsync();
            sw.Stop();
            dbStatus = canConnect
                ? new ServiceStatusDto("Supabase PostgreSQL", "healthy", (int)sw.ElapsedMilliseconds, "Pooler kết nối ổn định")
                : new ServiceStatusDto("Supabase PostgreSQL", "degraded", (int)sw.ElapsedMilliseconds, "Không phản hồi");
        }
        catch (Exception ex)
        {
            dbStatus = new ServiceStatusDto("Supabase PostgreSQL", "offline", 0, ex.Message);
        }

        ServiceStatusDto redisStatus;
        try
        {
            if (_redis != null && _redis.IsConnected)
            {
                var sw = Stopwatch.StartNew();
                await _redis.GetDatabase().PingAsync();
                sw.Stop();
                redisStatus = new ServiceStatusDto("Upstash Redis", "healthy", (int)sw.ElapsedMilliseconds, "Streams & Cache sẵn sàng");
            }
            else
            {
                redisStatus = new ServiceStatusDto("Upstash Redis", "degraded", 0, "Chưa kết nối");
            }
        }
        catch (Exception ex)
        {
            redisStatus = new ServiceStatusDto("Upstash Redis", "offline", 0, ex.Message);
        }

        ServiceStatusDto aiStatus;
        try
        {
            var sw = Stopwatch.StartNew();
            var aiBaseUrl = _configuration["AiService:BaseUrl"] ?? "http://localhost:8001/";
            var res = await _httpClient.GetAsync(aiBaseUrl.TrimEnd('/') + "/");
            sw.Stop();
            aiStatus = res.IsSuccessStatusCode
                ? new ServiceStatusDto("AI Vision (FastAPI)", "healthy", (int)sw.ElapsedMilliseconds, "Camera & YOLO v8 sẵn sàng")
                : new ServiceStatusDto("AI Vision (FastAPI)", "degraded", (int)sw.ElapsedMilliseconds, $"Mã phản hồi: {res.StatusCode}");
        }
        catch (Exception ex)
        {
            aiStatus = new ServiceStatusDto("AI Vision (FastAPI)", "offline", 0, ex.Message);
        }

        return new AdminSystemHealthDto(apiStatus, dbStatus, redisStatus, aiStatus);
    }

    // Lấy thông báo hệ thống đang phát sóng
    public SystemBroadcastDto GetBroadcast()
    {
        return new SystemBroadcastDto(
            SystemBroadcastState.Message,
            SystemBroadcastState.Type,
            SystemBroadcastState.IsActive,
            SystemBroadcastState.UpdatedAt
        );
    }

    // Cập nhật thông báo hệ thống và ghi nhận kiểm toán
    public async Task<SystemBroadcastDto> UpdateBroadcastAsync(Guid adminId, UpdateBroadcastRequest request)
    {
        var msg = request.Message?.Trim() ?? string.Empty;
        var type = string.IsNullOrWhiteSpace(request.Type) ? "info" : request.Type.Trim();
        var isActive = request.IsActive && !string.IsNullOrWhiteSpace(msg);
        SystemBroadcastState.Update(msg, type, isActive);

        var audit = new AdminActionHistory
        {
            AdminId = adminId,
            TargetUserId = adminId,
            Action = "UPDATE_BROADCAST",
            Reason = SystemBroadcastState.Type,
            Details = $"Cập nhật thông báo hệ thống: {SystemBroadcastState.Message} (Kích hoạt: {SystemBroadcastState.IsActive})",
            CreatedAt = DateTime.UtcNow
        };
        _context.AdminActionHistories.Add(audit);
        await _context.SaveChangesAsync();

        return new SystemBroadcastDto(
            SystemBroadcastState.Message,
            SystemBroadcastState.Type,
            SystemBroadcastState.IsActive,
            SystemBroadcastState.UpdatedAt
        );
    }

    // Lấy danh sách người dùng phân trang kèm bộ lọc
    public async Task<PagedResult<AdminUserSummaryDto>> GetUsersAsync(string? search, bool? isSuspended, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Users.AsNoTracking().Where(u => !u.IsAdmin);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var clean = search.Trim().ToLower();
            query = query.Where(u => u.Email.ToLower().Contains(clean) || u.DisplayName.ToLower().Contains(clean));
        }

        if (isSuspended.HasValue)
        {
            query = query.Where(u => u.IsSuspended == isSuspended.Value);
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserSummaryDto(
                u.Id,
                u.Email,
                u.DisplayName,
                u.AvatarUrl,
                u.CreatedAt,
                u.IsAdmin,
                u.IsSuspended,
                u.SuspendedReason,
                u.SuspendedAt,
                u.FocusSessions.OrderByDescending(f => f.StartedAt).Select(f => (DateTime?)f.StartedAt).FirstOrDefault()
            ))
            .ToListAsync();

        return new PagedResult<AdminUserSummaryDto>(users, totalCount, page, pageSize);
    }

    // Lấy thông tin chi tiết một người dùng
    public async Task<AdminUserSummaryDto?> GetUserByIdAsync(Guid id)
    {
        return await _context.Users.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new AdminUserSummaryDto(
                u.Id,
                u.Email,
                u.DisplayName,
                u.AvatarUrl,
                u.CreatedAt,
                u.IsAdmin,
                u.IsSuspended,
                u.SuspendedReason,
                u.SuspendedAt,
                u.FocusSessions.OrderByDescending(f => f.StartedAt).Select(f => (DateTime?)f.StartedAt).FirstOrDefault()
            ))
            .FirstOrDefaultAsync();
    }

    // Cập nhật trạng thái tạm khóa hoặc mở khóa tài khoản người dùng
    public async Task<string> UpdateUserStatusAsync(Guid adminId, Guid targetUserId, UpdateUserStatusRequest request)
    {
        if (adminId == targetUserId && request.IsSuspended)
        {
            throw new DomainException("Không thể tự khóa tài khoản quản trị của chính mình.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
        if (user is null)
        {
            throw new KeyNotFoundException("Không tìm thấy người dùng được chỉ định.");
        }

        if (user.IsAdmin && request.IsSuspended)
        {
            throw new DomainException("Không thể khóa tài khoản của quản trị viên khác.");
        }

        user.IsSuspended = request.IsSuspended;
        user.SuspendedReason = request.IsSuspended ? request.Reason?.Trim() : null;
        user.SuspendedAt = request.IsSuspended ? DateTime.UtcNow : null;

        if (request.IsSuspended)
        {
            await _context.RefreshTokens
                .Where(r => r.UserId == targetUserId && !r.IsRevoked)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsRevoked, true));
        }

        await _context.SaveChangesAsync();

        return request.IsSuspended
            ? $"Đã khóa tài khoản {user.Email} thành công."
            : $"Đã mở khóa tài khoản {user.Email} thành công.";
    }

    // Lấy danh sách nhật ký hành động quản trị
    public async Task<PagedResult<AdminAuditLogDto>> GetAuditLogsAsync(int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.AdminActionHistories.AsNoTracking().AsQueryable();
        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new AdminAuditLogDto(
                a.Id, a.AdminId,
                a.AdminUser != null ? a.AdminUser.Email : "admin@pomodo.internal",
                a.AdminUser != null ? a.AdminUser.DisplayName : "Hệ thống",
                a.TargetUserId,
                a.TargetUser != null ? a.TargetUser.Email : "Người dùng",
                a.TargetUser != null ? a.TargetUser.DisplayName : "Người dùng",
                a.Action, a.Reason, a.Details, a.CreatedAt
            )).ToListAsync();

        return new PagedResult<AdminAuditLogDto>(items, totalCount, page, pageSize);
    }

    // Gửi email trực tiếp tới người dùng với cơ chế xử lý ngầm tức thời và rút gọn nội dung kiểm toán
    public async Task<string> SendDirectEmailAsync(Guid adminId, AdminSendEmailRequest request)
    {
        var userIds = request.UserIds ?? new List<Guid>();
        var explicitEmails = (request.Emails ?? new List<string>())
            .Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim().ToLower()).Distinct().ToList();

        if (userIds.Count == 0 && explicitEmails.Count == 0)
        {
            throw new DomainException("Vui lòng chọn hoặc nhập ít nhất 1 người nhận.");
        }

        if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Content))
        {
            throw new DomainException("Tiêu đề và nội dung không được để trống.");
        }

        var targetUsers = await _context.Users
            .Where(u => userIds.Contains(u.Id) || explicitEmails.Contains(u.Email.ToLower())).ToListAsync();
        var existingEmails = targetUsers.Select(u => u.Email.ToLower()).ToHashSet();
        var externalEmails = explicitEmails.Where(e => !existingEmails.Contains(e)).ToList();
        var htmlBody = MarkdownEmailFormatter.FormatToSafeHtml(request.Content);
        var allRecipientEmails = new List<string>();

        // Tối ưu hóa kiểm toán & bảo mật PII: Chỉ lưu 100 ký tự tóm tắt thay vì toàn văn email
        var rawContent = request.Content.Trim();
        var contentSnippet = rawContent.Length > 100
            ? string.Concat(rawContent.AsSpan(0, 100), "...")
            : rawContent;

        foreach (var user in targetUsers)
        {
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                allRecipientEmails.Add(user.Email);
                _context.AdminActionHistories.Add(new AdminActionHistory
                {
                    AdminId = adminId,
                    TargetUserId = user.Id,
                    Action = "SEND_EMAIL",
                    Reason = request.Subject.Trim(),
                    Details = $"Gửi email cho {user.Email}: {request.Subject.Trim()} - {contentSnippet}",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        foreach (var extEmail in externalEmails)
        {
            allRecipientEmails.Add(extEmail);
            _context.AdminActionHistories.Add(new AdminActionHistory
            {
                AdminId = adminId,
                TargetUserId = adminId,
                Action = "SEND_EMAIL",
                Reason = request.Subject.Trim(),
                Details = $"Gửi email cho {extEmail}: {request.Subject.Trim()} - {contentSnippet}",
                CreatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        var recipientsToSend = allRecipientEmails.Distinct().ToList();
        var subjectToSend = request.Subject.Trim();

        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var logger = scope.ServiceProvider.GetService<ILogger<AdminService>>();

                // Gửi theo từng lô 3 email để bảo vệ kết nối SMTP và tránh bị Gmail chặn rate-limit
                for (int i = 0; i < recipientsToSend.Count; i += 3)
                {
                    var batch = recipientsToSend.Skip(i).Take(3).ToList();
                    await Task.WhenAll(batch.Select(async email =>
                    {
                        try
                        {
                            await emailService.SendEmailReportAsync(email, subjectToSend, htmlBody);
                        }
                        catch (Exception ex)
                        {
                            logger?.LogError(ex, "Thất bại khi gửi email tới {Email}", email);
                        }
                    }));

                    if (i + 3 < recipientsToSend.Count)
                    {
                        await Task.Delay(250);
                    }
                }
            }
            catch (Exception ex)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var logger = scope.ServiceProvider.GetService<ILogger<AdminService>>();
                logger?.LogError(ex, "Lỗi xảy ra trong quá trình xử lý gửi email ngầm hàng loạt.");
            }
        });

        return $"Đã gửi email thành công tới {recipientsToSend.Count} người nhận.";
    }

    // Lấy danh sách thông báo đã lên lịch đang chờ gửi
    public async Task<List<AdminScheduledNotificationDto>> GetScheduledNotificationsAsync()
    {
        return await _context.ScheduledNotifications.AsNoTracking()
            .Where(s => !s.IsSent)
            .OrderBy(s => s.ScheduledAt)
            .Select(s => new AdminScheduledNotificationDto(
                s.Id, s.AdminId, s.Title, s.Content, s.Category, s.Scope, s.RecipientEmails,
                s.ScheduledAt, s.IsSent, s.SentAt, s.CreatedAt
            )).ToListAsync();
    }

    // Tạo mới một lịch thông báo tự động (kích hoạt cờ hiệu in-memory)
    public async Task<AdminScheduledNotificationDto> CreateScheduledNotificationAsync(Guid adminId, CreateScheduledNotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
        {
            throw new DomainException("Tiêu đề và nội dung không được để trống.");
        }

        var emails = request.RecipientEmails != null && request.RecipientEmails.Count > 0
            ? string.Join(",", request.RecipientEmails.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()))
            : null;

        var entity = new ScheduledNotification
        {
            AdminId = adminId,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "info" : request.Category.Trim().ToLower(),
            Scope = string.IsNullOrWhiteSpace(request.Scope) ? "direct" : request.Scope.Trim().ToLower(),
            RecipientEmails = emails,
            ScheduledAt = request.ScheduledAtUtc.ToUniversalTime(),
            IsSent = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.ScheduledNotifications.Add(entity);
        await _context.SaveChangesAsync();

        _signal.NotifyNewSchedule(entity.ScheduledAt);

        return new AdminScheduledNotificationDto(
            entity.Id, entity.AdminId, entity.Title, entity.Content, entity.Category,
            entity.Scope, entity.RecipientEmails, entity.ScheduledAt, entity.IsSent,
            entity.SentAt, entity.CreatedAt);
    }

    // Hủy một lịch thông báo chưa gửi
    public async Task<bool> DeleteScheduledNotificationAsync(Guid id)
    {
        var item = await _context.ScheduledNotifications.FindAsync(id);
        if (item == null) return false;

        _context.ScheduledNotifications.Remove(item);
        await _context.SaveChangesAsync();
        _signal.Trigger();

        return true;
    }
}
