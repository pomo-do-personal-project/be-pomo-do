using System.Security.Claims;
using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.DTOs.Analytics;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be_pomo_do.Presentation.Controllers;

// Controller cung cấp API bản đồ nhiệt năng suất 7x24 và thông số phân tích AI
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    // Khởi tạo AnalyticsController qua Traditional Constructor
    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
    }

    // Lấy dữ liệu ma trận bản đồ nhiệt năng suất 7x24 và các chỉ số AI
    [HttpGet("heatmap")]
    public async Task<ActionResult<ApiResponse<FocusHeatmapResponse>>> GetFocusHeatmap([FromQuery] int rangeDays = 30)
    {
        var userId = GetCurrentUserId();
        var response = await _analyticsService.GetFocusHeatmapAsync(userId, rangeDays);
        return Ok(ApiResponse<FocusHeatmapResponse>.Ok(response, "Lấy bản đồ nhiệt năng suất thành công."));
    }

    // Lấy thông số phân tích nhịp sinh học và khung giờ vàng
    [HttpGet("insights")]
    public async Task<ActionResult<ApiResponse<AiProductivityInsightDto>>> GetProductivityInsights([FromQuery] int rangeDays = 30)
    {
        var userId = GetCurrentUserId();
        var response = await _analyticsService.GetFocusHeatmapAsync(userId, rangeDays);
        return Ok(ApiResponse<AiProductivityInsightDto>.Ok(response.Insight, "Lấy phân tích năng suất AI thành công."));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Không thể xác thực thông tin người dùng.");
        }

        return userId;
    }
}
