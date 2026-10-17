using be_pomo_do.Application.Common;
using be_pomo_do.Application.DTOs.Admin;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace be_pomo_do.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    // Retrieve the current active broadcast announcement for all app users.
    [HttpGet("broadcast")]
    public ActionResult<ApiResponse<SystemBroadcastDto>> GetActiveBroadcast()
    {
        var dto = new SystemBroadcastDto(
            SystemBroadcastState.Message,
            SystemBroadcastState.Type,
            SystemBroadcastState.IsActive,
            SystemBroadcastState.UpdatedAt
        );

        return Ok(ApiResponse<SystemBroadcastDto>.Ok(dto, "Lấy thông báo hệ thống thành công."));
    }
}
