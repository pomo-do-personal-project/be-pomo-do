using System.Security.Claims;
using be_pomo_do.Application.DTOs.Pomodoro;
using be_pomo_do.Application.Services;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be_pomo_do.Presentation.Controllers;

// Endpoints for Pomodoro timer sessions, overtime tracking, and daily streaks.
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PomodoroController : ControllerBase
{
    private readonly IPomodoroService _pomodoroService;

    // Initializes a new instance of PomodoroController with traditional constructor dependency injection.
    public PomodoroController(IPomodoroService pomodoroService)
    {
        _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));
    }

    // Starts a new Pomodoro or Stopwatch session.
    [HttpPost("start")]
    public async Task<ActionResult<ApiResponse<FocusSessionDto>>> StartSession([FromBody] StartSessionRequest request)
    {
        var userId = GetCurrentUserId();
        var session = await _pomodoroService.StartSessionAsync(userId, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<FocusSessionDto>.Ok(session, "Bắt đầu phiên tập trung thành công."));
    }

    // Completes an ongoing focus session, records overtime, and updates user daily streak.
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ApiResponse<FocusSessionDto>>> CompleteSession(
        [FromRoute] Guid id,
        [FromBody] CompleteSessionRequest request)
    {
        var userId = GetCurrentUserId();
        var session = await _pomodoroService.CompleteSessionAsync(id, userId, request);
        return Ok(ApiResponse<FocusSessionDto>.Ok(session, "Hoàn thành phiên tập trung thành công."));
    }

    // Cancels an ongoing session and records elapsed duration.
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<FocusSessionDto>>> CancelSession(
        [FromRoute] Guid id,
        [FromBody] CancelSessionRequest request)
    {
        var userId = GetCurrentUserId();
        var session = await _pomodoroService.CancelSessionAsync(id, userId, request);
        return Ok(ApiResponse<FocusSessionDto>.Ok(session, "Đã hủy phiên tập trung."));
    }

    // Retrieves focus session history within an optional UTC date range.
    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<List<FocusSessionDto>>>> GetHistory(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc)
    {
        var userId = GetCurrentUserId();
        var history = await _pomodoroService.GetHistoryAsync(userId, fromUtc, toUtc);
        return Ok(ApiResponse<List<FocusSessionDto>>.Ok(history, "Lấy lịch sử phiên tập trung thành công."));
    }

    // Retrieves summary metrics for today (pomos, total minutes, overtime minutes, and streak).
    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<PomodoroSummaryDto>>> GetSummary()
    {
        var userId = GetCurrentUserId();
        var summary = await _pomodoroService.GetSummaryAsync(userId);
        return Ok(ApiResponse<PomodoroSummaryDto>.Ok(summary, "Lấy thống kê tập trung thành công."));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");
        }

        return userId;
    }
}
