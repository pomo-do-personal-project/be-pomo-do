using System.Security.Claims;
using be_pomo_do.Application.DTOs.Settings;
using be_pomo_do.Application.Services;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be_pomo_do.Presentation.Controllers;

/// <summary>Endpoints for managing user preferences and Pomodoro timer settings.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    /// <summary>Initializes a new instance of <see cref="SettingsController"/>.</summary>
    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    /// <summary>Lấy cấu hình thời lượng Pomodoro và preferences của người dùng hiện tại.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<UserSettingsDto>>> GetSettings()
    {
        var userId = GetCurrentUserId();
        var settings = await _settingsService.GetSettingsAsync(userId);
        return Ok(ApiResponse<UserSettingsDto>.Ok(settings, "Lấy cài đặt thành công."));
    }

    /// <summary>Cập nhật thời lượng Pomodoro (Focus, Short Break, Long Break) và cấu hình cá nhân.</summary>
    [HttpPut]
    public async Task<ActionResult<ApiResponse<UserSettingsDto>>> UpdateSettings([FromBody] UpdateUserSettingsRequest request)
    {
        var userId = GetCurrentUserId();
        var settings = await _settingsService.UpdateSettingsAsync(userId, request);
        return Ok(ApiResponse<UserSettingsDto>.Ok(settings, "Cập nhật cài đặt thành công."));
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
