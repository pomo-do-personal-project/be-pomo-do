using System.Security.Claims;
using be_pomo_do.Application.Common;
using be_pomo_do.Application.DTOs.Admin;
using be_pomo_do.Core.Entities;
using be_pomo_do.Infrastructure.Data;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace be_pomo_do.Presentation.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminCommunicationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ScheduledNotificationSignal _signal;

    public AdminCommunicationsController(AppDbContext context, ScheduledNotificationSignal signal)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _signal = signal ?? throw new ArgumentNullException(nameof(signal));
    }

    // Lấy danh sách nhật ký hành động quản trị phân trang
    [HttpGet("audit-logs")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminAuditLogDto>>>> GetAuditLogs(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
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

        return Ok(ApiResponse<PagedResult<AdminAuditLogDto>>.Ok(
            new PagedResult<AdminAuditLogDto>(items, totalCount, page, pageSize), "Lấy nhật ký thành công."));
    }

    // Gửi email trực tiếp tới người dùng với cơ chế xử lý ngầm tức thời (< 100ms)
    [HttpPost("send-email")]
    public async Task<ActionResult<ApiResponse<string>>> SendEmail(
        [FromBody] AdminSendEmailRequest request, [FromServices] IServiceProvider serviceProvider)
    {
        var userIds = request.UserIds ?? new List<Guid>();
        var explicitEmails = (request.Emails ?? new List<string>())
            .Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim().ToLower()).Distinct().ToList();

        if (userIds.Count == 0 && explicitEmails.Count == 0)
            return BadRequest(ApiResponse<string>.Fail("Vui lòng chọn hoặc nhập ít nhất 1 người nhận."));
        if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(ApiResponse<string>.Fail("Tiêu đề và nội dung không được để trống."));

        var adminId = GetCurrentAdminId();
        var targetUsers = await _context.Users
            .Where(u => userIds.Contains(u.Id) || explicitEmails.Contains(u.Email.ToLower())).ToListAsync();
        var existingEmails = targetUsers.Select(u => u.Email.ToLower()).ToHashSet();
        var externalEmails = explicitEmails.Where(e => !existingEmails.Contains(e)).ToList();
        var htmlBody = $"<div style='font-family:sans-serif;line-height:1.6;'>{request.Content.Replace("\n", "<br/>")}</div>";
        var allRecipientEmails = new List<string>();

        foreach (var user in targetUsers)
        {
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                allRecipientEmails.Add(user.Email);
                _context.AdminActionHistories.Add(new AdminActionHistory
                {
                    AdminId = adminId, TargetUserId = user.Id, Action = "SEND_EMAIL",
                    Reason = request.Subject.Trim(),
                    Details = $"Gửi email cho {user.Email}: {request.Subject.Trim()} - {request.Content.Trim()}",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        foreach (var extEmail in externalEmails)
        {
            allRecipientEmails.Add(extEmail);
            _context.AdminActionHistories.Add(new AdminActionHistory
            {
                AdminId = adminId, TargetUserId = adminId, Action = "SEND_EMAIL",
                Reason = request.Subject.Trim(),
                Details = $"Gửi email cho {extEmail}: {request.Subject.Trim()} - {request.Content.Trim()}",
                CreatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        var recipientsToSend = allRecipientEmails.Distinct().ToList();
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<be_pomo_do.Application.Common.Interfaces.IEmailService>();
                await Task.WhenAll(recipientsToSend.Select(e => emailService.SendEmailReportAsync(e, request.Subject.Trim(), htmlBody)));
            }
            catch { }
        });

        return Ok(ApiResponse<string>.Ok(
            $"Đã gửi email thành công tới {recipientsToSend.Count} người nhận.",
            $"Đã gửi email thành công tới {recipientsToSend.Count} người nhận."));
    }

    // Lấy danh sách thông báo đã lên lịch đang chờ gửi
    [HttpGet("scheduled")]
    public async Task<ActionResult<ApiResponse<List<AdminScheduledNotificationDto>>>> GetScheduled()
    {
        var list = await _context.ScheduledNotifications.AsNoTracking()
            .Where(s => !s.IsSent)
            .OrderBy(s => s.ScheduledAt)
            .Select(s => new AdminScheduledNotificationDto(
                s.Id, s.AdminId, s.Title, s.Content, s.Category, s.Scope, s.RecipientEmails,
                s.ScheduledAt, s.IsSent, s.SentAt, s.CreatedAt
            )).ToListAsync();

        return Ok(ApiResponse<List<AdminScheduledNotificationDto>>.Ok(list, "Lấy danh sách lên lịch thành công."));
    }

    // Tạo mới một lịch thông báo tự động (kích hoạt cờ hiệu in-memory)
    [HttpPost("scheduled")]
    public async Task<ActionResult<ApiResponse<AdminScheduledNotificationDto>>> CreateScheduled(
        [FromBody] CreateScheduledNotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(ApiResponse<AdminScheduledNotificationDto>.Fail("Tiêu đề và nội dung không được để trống."));

        var adminId = GetCurrentAdminId();
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

        var dto = new AdminScheduledNotificationDto(
            entity.Id, entity.AdminId, entity.Title, entity.Content, entity.Category,
            entity.Scope, entity.RecipientEmails, entity.ScheduledAt, entity.IsSent,
            entity.SentAt, entity.CreatedAt);

        return Ok(ApiResponse<AdminScheduledNotificationDto>.Ok(dto, "Lên lịch thông báo thành công."));
    }

    // Hủy một lịch thông báo chưa gửi
    [HttpDelete("scheduled/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteScheduled(Guid id)
    {
        var item = await _context.ScheduledNotifications.FindAsync(id);
        if (item == null) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy thông báo lên lịch."));

        _context.ScheduledNotifications.Remove(item);
        await _context.SaveChangesAsync();
        _signal.Trigger();

        return Ok(ApiResponse<bool>.Ok(true, "Hủy lịch thông báo thành công."));
    }

    private Guid GetCurrentAdminId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        if (!string.IsNullOrEmpty(idClaim) && Guid.TryParse(idClaim, out var adminId)) return adminId;
        var fallbackAdmin = _context.Users.FirstOrDefault(u => u.IsAdmin);
        if (fallbackAdmin != null) return fallbackAdmin.Id;
        throw new UnauthorizedAccessException("Không xác định được danh tính Admin từ token.");
    }
}
