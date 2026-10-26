using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.DTOs.Assets;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be_pomo_do.Presentation.Controllers;

[ApiController]
public class SystemAssetsController : ControllerBase
{
    private readonly ISystemAssetService _assetService;

    public SystemAssetsController(ISystemAssetService assetService)
    {
        _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
    }

    // Lấy danh sách tài nguyên kích hoạt cho người dùng (có thể lọc theo loại và thể loại)
    [HttpGet("api/assets")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SystemAssetDto>>>> GetActiveAssets(
        [FromQuery] string? type,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        var result = await _assetService.GetActiveAssetsAsync(type, category, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SystemAssetDto>>.Ok(result, "Lấy danh sách tài nguyên thành công."));
    }

    // Admin: Lấy toàn bộ danh sách tài nguyên kèm phân trang và tìm kiếm
    [HttpGet("api/admin/assets")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> GetAdminAssets(
        [FromQuery] string? type,
        [FromQuery] string? category,
        [FromQuery] bool? isActive,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _assetService.GetAdminAssetsAsync(type, category, isActive, search, page, pageSize, cancellationToken);
        var response = new
        {
            items,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
        return Ok(ApiResponse<object>.Ok(response, "Lấy danh sách tài nguyên quản trị thành công."));
    }

    // Admin: Xem chi tiết tài nguyên
    [HttpGet("api/admin/assets/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SystemAssetDto>>> GetAssetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _assetService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<SystemAssetDto>.Ok(result, "Lấy thông tin tài nguyên thành công."));
    }

    // Admin: Thêm mới tài nguyên vào kho
    [HttpPost("api/admin/assets")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SystemAssetDto>>> CreateAsset([FromBody] CreateAssetRequest request, CancellationToken cancellationToken)
    {
        var result = await _assetService.CreateAssetAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAssetById), new { id = result.Id }, ApiResponse<SystemAssetDto>.Ok(result, "Thêm tài nguyên mới thành công."));
    }

    // Admin: Cập nhật tài nguyên
    [HttpPut("api/admin/assets/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SystemAssetDto>>> UpdateAsset(Guid id, [FromBody] UpdateAssetRequest request, CancellationToken cancellationToken)
    {
        var result = await _assetService.UpdateAssetAsync(id, request, cancellationToken);
        return Ok(ApiResponse<SystemAssetDto>.Ok(result, "Cập nhật tài nguyên thành công."));
    }

    // Admin: Xóa tài nguyên
    [HttpDelete("api/admin/assets/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsset(Guid id, CancellationToken cancellationToken)
    {
        var result = await _assetService.DeleteAssetAsync(id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Xóa tài nguyên thành công."));
    }

    // Admin: Bật/tắt nhanh trạng thái khả dụng
    [HttpPatch("api/admin/assets/{id:guid}/toggle-active")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SystemAssetDto>>> ToggleActive(Guid id, CancellationToken cancellationToken)
    {
        var result = await _assetService.ToggleActiveAsync(id, cancellationToken);
        return Ok(ApiResponse<SystemAssetDto>.Ok(result, "Thay đổi trạng thái tài nguyên thành công."));
    }

    // Admin: Đặt làm tài nguyên mặc định
    [HttpPatch("api/admin/assets/{id:guid}/set-default")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SystemAssetDto>>> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        var result = await _assetService.SetDefaultAsync(id, cancellationToken);
        return Ok(ApiResponse<SystemAssetDto>.Ok(result, "Đặt tài nguyên làm mặc định thành công."));
    }
}
