using System.Security.Claims;
using be_pomo_do.Application.DTOs.Admin;
using be_pomo_do.Application.Services;
using be_pomo_do.Core.Exceptions;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be_pomo_do.Presentation.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminService _adminService;

    // Khởi tạo AdminUsersController thông qua IAdminService (tuân thủ Clean Architecture)
    public AdminUsersController(IAdminService adminService)
    {
        _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
    }

    // Lấy danh sách người dùng phân trang kèm bộ lọc tìm kiếm và trạng thái
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminUserSummaryDto>>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] bool? isSuspended,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15)
    {
        var result = await _adminService.GetUsersAsync(search, isSuspended, page, pageSize);
        return Ok(ApiResponse<PagedResult<AdminUserSummaryDto>>.Ok(result, "Lấy danh sách người dùng thành công."));
    }

    // Lấy thông tin chi tiết của một người dùng theo ID
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminUserSummaryDto>>> GetUserById([FromRoute] Guid id)
    {
        var user = await _adminService.GetUserByIdAsync(id);
        if (user is null)
        {
            return NotFound(ApiResponse<AdminUserSummaryDto>.Fail("Không tìm thấy người dùng."));
        }

        return Ok(ApiResponse<AdminUserSummaryDto>.Ok(user, "Lấy thông tin người dùng thành công."));
    }

    // Tạm khóa hoặc mở khóa tài khoản người dùng và thu hồi refresh token
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateUserStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateUserStatusRequest request)
    {
        var adminId = GetCurrentAdminId();
        try
        {
            var msg = await _adminService.UpdateUserStatusAsync(adminId, id, request);
            return Ok(ApiResponse<string>.Ok(msg, msg));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
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
