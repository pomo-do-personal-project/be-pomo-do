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
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly AppDbContext _context;

    // Khởi tạo AdminUsersController với DbContext
    public AdminUsersController(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Lấy danh sách người dùng phân trang kèm bộ lọc tìm kiếm và trạng thái
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminUserSummaryDto>>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] bool? isSuspended,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15)
    {
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

        var result = new PagedResult<AdminUserSummaryDto>(users, totalCount, page, pageSize);
        return Ok(ApiResponse<PagedResult<AdminUserSummaryDto>>.Ok(result, "Lấy danh sách người dùng thành công."));
    }

    // Lấy thông tin chi tiết của một người dùng theo ID
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminUserSummaryDto>>> GetUserById([FromRoute] Guid id)
    {
        var user = await _context.Users.AsNoTracking()
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

        if (adminId == id && request.IsSuspended)
        {
            return BadRequest(ApiResponse<string>.Fail("Không thể tự khóa tài khoản quản trị của chính mình."));
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound(ApiResponse<string>.Fail("Không tìm thấy người dùng được chỉ định."));
        }

        user.IsSuspended = request.IsSuspended;
        user.SuspendedReason = request.IsSuspended ? request.Reason?.Trim() : null;
        user.SuspendedAt = request.IsSuspended ? DateTime.UtcNow : null;

        if (request.IsSuspended)
        {
            await _context.RefreshTokens
                .Where(r => r.UserId == id && !r.IsRevoked)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsRevoked, true));
        }

        await _context.SaveChangesAsync();

        var msg = request.IsSuspended
            ? $"Đã khóa tài khoản {user.Email} thành công."
            : $"Đã mở khóa tài khoản {user.Email} thành công.";

        return Ok(ApiResponse<string>.Ok(msg, msg));
    }

    // Trích xuất ID admin hiện tại từ claim hoặc fallback an toàn
    private Guid GetCurrentAdminId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("id")?.Value;

        if (!string.IsNullOrEmpty(idClaim) && Guid.TryParse(idClaim, out var adminId))
        {
            return adminId;
        }

        var fallbackAdmin = _context.Users.FirstOrDefault(u => u.IsAdmin);
        if (fallbackAdmin != null)
        {
            return fallbackAdmin.Id;
        }

        throw new UnauthorizedAccessException("Không xác định được danh tính Admin từ token.");
    }
}
