using System.Security.Claims;
using be_pomo_do.Application.DTOs.Admin;
using be_pomo_do.Application.Services;
using be_pomo_do.Core.Exceptions;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be_pomo_do.Presentation.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminCommunicationsController : ControllerBase
{
    private readonly IAdminService _adminService;

    // Khởi tạo AdminCommunicationsController với IAdminService (tuân thủ Clean Architecture)
    public AdminCommunicationsController(IAdminService adminService)
    {
        _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
    }

    // Lấy danh sách nhật ký hành động quản trị phân trang
    [HttpGet("audit-logs")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminAuditLogDto>>>> GetAuditLogs(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.GetAuditLogsAsync(page, pageSize);
        return Ok(ApiResponse<PagedResult<AdminAuditLogDto>>.Ok(result, "Lấy nhật ký thành công."));
    }

    // Gửi email trực tiếp tới người dùng với cơ chế xử lý ngầm tức thời (< 100ms)
    [HttpPost("send-email")]
    public async Task<ActionResult<ApiResponse<string>>> SendEmail(
        [FromBody] AdminSendEmailRequest request)
    {
        var adminId = GetCurrentAdminId();
        try
        {
            var message = await _adminService.SendDirectEmailAsync(adminId, request);
            return Ok(ApiResponse<string>.Ok(message, message));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    // Lấy danh sách thông báo đã lên lịch đang chờ gửi
    [HttpGet("scheduled")]
    public async Task<ActionResult<ApiResponse<List<AdminScheduledNotificationDto>>>> GetScheduled()
    {
        var list = await _adminService.GetScheduledNotificationsAsync();
        return Ok(ApiResponse<List<AdminScheduledNotificationDto>>.Ok(list, "Lấy danh sách lên lịch thành công."));
    }

    // Tạo mới một lịch thông báo tự động (kích hoạt cờ hiệu in-memory)
    [HttpPost("scheduled")]
    public async Task<ActionResult<ApiResponse<AdminScheduledNotificationDto>>> CreateScheduled(
        [FromBody] CreateScheduledNotificationRequest request)
    {
        var adminId = GetCurrentAdminId();
        try
        {
            var dto = await _adminService.CreateScheduledNotificationAsync(adminId, request);
            return Ok(ApiResponse<AdminScheduledNotificationDto>.Ok(dto, "Lên lịch thông báo thành công."));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<AdminScheduledNotificationDto>.Fail(ex.Message));
        }
    }

    // Hủy một lịch thông báo chưa gửi
    [HttpDelete("scheduled/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteScheduled(Guid id)
    {
        var success = await _adminService.DeleteScheduledNotificationAsync(id);
        if (!success)
        {
            return NotFound(ApiResponse<bool>.Fail("Không tìm thấy thông báo lên lịch."));
        }

        return Ok(ApiResponse<bool>.Ok(true, "Hủy lịch thông báo thành công."));
    }

    // Trích xuất ID admin hiện tại từ claim
    private Guid GetCurrentAdminId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("id")?.Value;

        if (!string.IsNullOrEmpty(idClaim) && Guid.TryParse(idClaim, out var adminId))
        {
            return adminId;
        }

        throw new UnauthorizedAccessException("Không xác định được danh tính Admin từ token.");
    }
}
