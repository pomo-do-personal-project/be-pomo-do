using System.Security.Claims;
using be_pomo_do.Application.DTOs.Admin;
using be_pomo_do.Application.Services;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be_pomo_do.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    // Khởi tạo AdminController với IAdminService (tuân thủ Clean Architecture)
    public AdminController(IAdminService adminService)
    {
        _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
    }

    // Lấy số liệu vận hành tổng quan cho Admin Dashboard
    [HttpGet("metrics")]
    public async Task<ActionResult<ApiResponse<AdminMetricsDto>>> GetMetrics()
    {
        var metrics = await _adminService.GetMetricsAsync();
        return Ok(ApiResponse<AdminMetricsDto>.Ok(metrics, "Lấy số liệu thống kê thành công."));
    }

    // Kiểm tra sức khỏe và độ trễ của các dịch vụ hạ tầng
    [HttpGet("health")]
    public async Task<ActionResult<ApiResponse<AdminSystemHealthDto>>> GetSystemHealth()
    {
        var health = await _adminService.GetSystemHealthAsync();
        return Ok(ApiResponse<AdminSystemHealthDto>.Ok(health, "Lấy trạng thái hệ thống thành công."));
    }

    // Lấy thông báo hệ thống đang phát sóng
    [HttpGet("broadcast")]
    public ActionResult<ApiResponse<SystemBroadcastDto>> GetBroadcast()
    {
        var dto = _adminService.GetBroadcast();
        return Ok(ApiResponse<SystemBroadcastDto>.Ok(dto, "Lấy thông báo phát sóng thành công."));
    }

    // Cập nhật thông báo hệ thống và ghi nhận kiểm toán
    [HttpPost("broadcast")]
    public async Task<ActionResult<ApiResponse<SystemBroadcastDto>>> UpdateBroadcast([FromBody] UpdateBroadcastRequest request)
    {
        var adminId = GetCurrentAdminId();
        var dto = await _adminService.UpdateBroadcastAsync(adminId, request);
        return Ok(ApiResponse<SystemBroadcastDto>.Ok(dto, "Cập nhật thông báo phát sóng thành công."));
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
