using System.Security.Claims;
using be_pomo_do.Application.DTOs.Settings;
using be_pomo_do.Application.Services;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be_pomo_do.Presentation.Controllers;

// Endpoints for managing user preferences and Pomodoro timer settings.
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;
    private readonly Application.Common.Interfaces.IEmailReportService _emailReportService;

    // Khởi tạo SettingsController với ISettingsService và IEmailReportService
    public SettingsController(
        ISettingsService settingsService,
        Application.Common.Interfaces.IEmailReportService emailReportService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _emailReportService = emailReportService ?? throw new ArgumentNullException(nameof(emailReportService));
    }

    // Lấy cấu hình thời lượng Pomodoro và preferences của người dùng hiện tại
    [HttpGet]
    public async Task<ActionResult<ApiResponse<UserSettingsDto>>> GetSettings()
    {
        var userId = GetCurrentUserId();
        var settings = await _settingsService.GetSettingsAsync(userId);
        return Ok(ApiResponse<UserSettingsDto>.Ok(settings, "Lấy cài đặt thành công."));
    }

    // Cập nhật thời lượng Pomodoro và cấu hình nhận email báo cáo
    [HttpPut]
    public async Task<ActionResult<ApiResponse<UserSettingsDto>>> UpdateSettings([FromBody] UpdateUserSettingsRequest request)
    {
        var userId = GetCurrentUserId();
        var settings = await _settingsService.UpdateSettingsAsync(userId, request);
        return Ok(ApiResponse<UserSettingsDto>.Ok(settings, "Cập nhật cài đặt thành công."));
    }

    // Gửi thử nghiệm email báo cáo (Weekly hoặc Daily) ngay lập tức
    [HttpPost("email-report/test")]
    public async Task<ActionResult<ApiResponse<bool>>> SendTestEmailReport([FromBody] TestEmailReportRequest request)
    {
        var userId = GetCurrentUserId();
        var isWeekly = string.Equals(request.ReportType, "weekly", StringComparison.OrdinalIgnoreCase);
        var isDaily = string.Equals(request.ReportType, "daily", StringComparison.OrdinalIgnoreCase);

        if (!isWeekly && !isDaily)
        {
            throw new Core.Exceptions.DomainException("Loại báo cáo không hợp lệ. Vui lòng chọn 'weekly' hoặc 'daily'.");
        }

        if (isWeekly)
        {
            await _emailReportService.SendWeeklyReportAsync(userId, request.TargetEmail);
        }
        else
        {
            await _emailReportService.SendDailyDigestAsync(userId, request.TargetEmail);
        }

        var recipientNotice = string.IsNullOrWhiteSpace(request.TargetEmail) ? "hộp thư tài khoản của bạn" : request.TargetEmail;
        return Ok(ApiResponse<bool>.Ok(true, $"Đã gửi thử nghiệm báo cáo ({request.ReportType}) tới {recipientNotice} thành công."));
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
