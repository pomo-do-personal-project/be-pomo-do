using System.Diagnostics;
using System.Security.Claims;
using be_pomo_do.Application.Common;
using be_pomo_do.Application.DTOs.Admin;
using be_pomo_do.Core.Entities;
using be_pomo_do.Infrastructure.Data;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace be_pomo_do.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConnectionMultiplexer? _redis;
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(2) };

    // Khởi tạo AdminController với DbContext và Redis
    public AdminController(AppDbContext context, IServiceProvider serviceProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _redis = serviceProvider.GetService<IConnectionMultiplexer>();
    }

    // Lấy số liệu vận hành tổng quan cho Admin Dashboard
    [HttpGet("metrics")]
    public async Task<ActionResult<ApiResponse<AdminMetricsDto>>> GetMetrics()
    {
        var totalUsers = await _context.Users.CountAsync(u => !u.IsAdmin);
        var suspendedUsers = await _context.Users.CountAsync(u => !u.IsAdmin && u.IsSuspended);

        var todayUtc = DateTime.UtcNow.Date;
        var activeUsersToday = await _context.FocusSessions
            .Where(f => f.StartedAt >= todayUtc)
            .Select(f => f.UserId)
            .Distinct()
            .CountAsync();

        var activeRate = totalUsers > 0
            ? Math.Round(((double)activeUsersToday / totalUsers) * 100.0, 1)
            : 0.0;

        var googleUsers = await _context.Users.CountAsync(u => u.Provider == Core.ValueObjects.UserProvider.Google);
        var localUsers = await _context.Users.CountAsync(u => u.Provider == Core.ValueObjects.UserProvider.Local);

        var metrics = new AdminMetricsDto(
            TotalUsers: totalUsers,
            ActiveUsersToday: activeUsersToday,
            SuspendedUsers: suspendedUsers,
            ActiveRatePercentage: activeRate,
            GoogleUsersCount: googleUsers,
            LocalUsersCount: localUsers
        );

        return Ok(ApiResponse<AdminMetricsDto>.Ok(metrics, "Lấy số liệu thống kê thành công."));
    }

    // Kiểm tra sức khỏe và độ trễ của các dịch vụ hạ tầng
    [HttpGet("health")]
    public async Task<ActionResult<ApiResponse<AdminSystemHealthDto>>> GetSystemHealth()
    {
        // 1. API Backend
        var apiStatus = new ServiceStatusDto("Backend API", "healthy", 1, "ASP.NET Core (.NET 10.0)");

        // 2. Database Supabase
        ServiceStatusDto dbStatus;
        try
        {
            var sw = Stopwatch.StartNew();
            var canConnect = await _context.Database.CanConnectAsync();
            sw.Stop();
            dbStatus = canConnect
                ? new ServiceStatusDto("Supabase PostgreSQL", "healthy", (int)sw.ElapsedMilliseconds, "Pooler kết nối ổn định")
                : new ServiceStatusDto("Supabase PostgreSQL", "degraded", (int)sw.ElapsedMilliseconds, "Không phản hồi");
        }
        catch (Exception ex)
        {
            dbStatus = new ServiceStatusDto("Supabase PostgreSQL", "offline", 0, ex.Message);
        }

        // 3. Upstash Redis
        ServiceStatusDto redisStatus;
        try
        {
            if (_redis != null && _redis.IsConnected)
            {
                var sw = Stopwatch.StartNew();
                await _redis.GetDatabase().PingAsync();
                sw.Stop();
                redisStatus = new ServiceStatusDto("Upstash Redis", "healthy", (int)sw.ElapsedMilliseconds, "Streams & Cache sẵn sàng");
            }
            else
            {
                redisStatus = new ServiceStatusDto("Upstash Redis", "degraded", 0, "Chưa kết nối");
            }
        }
        catch (Exception ex)
        {
            redisStatus = new ServiceStatusDto("Upstash Redis", "offline", 0, ex.Message);
        }

        // 4. AI Vision Service
        ServiceStatusDto aiStatus;
        try
        {
            var sw = Stopwatch.StartNew();
            var res = await _httpClient.GetAsync("http://localhost:8000/");
            sw.Stop();
            aiStatus = res.IsSuccessStatusCode
                ? new ServiceStatusDto("AI Vision (FastAPI)", "healthy", (int)sw.ElapsedMilliseconds, "Camera & YOLO v8 sẵn sàng")
                : new ServiceStatusDto("AI Vision (FastAPI)", "degraded", (int)sw.ElapsedMilliseconds, $"Mã phản hồi: {res.StatusCode}");
        }
        catch (Exception ex)
        {
            aiStatus = new ServiceStatusDto("AI Vision (FastAPI)", "offline", 0, ex.Message);
        }

        var health = new AdminSystemHealthDto(apiStatus, dbStatus, redisStatus, aiStatus);
        return Ok(ApiResponse<AdminSystemHealthDto>.Ok(health, "Lấy trạng thái hệ thống thành công."));
    }

    // Lấy thông báo hệ thống đang phát sóng
    [HttpGet("broadcast")]
    public ActionResult<ApiResponse<SystemBroadcastDto>> GetBroadcast()
    {
        var dto = new SystemBroadcastDto(
            SystemBroadcastState.Message,
            SystemBroadcastState.Type,
            SystemBroadcastState.IsActive,
            SystemBroadcastState.UpdatedAt
        );
        return Ok(ApiResponse<SystemBroadcastDto>.Ok(dto, "Lấy thông báo phát sóng thành công."));
    }

    // Cập nhật thông báo hệ thống và ghi nhận kiểm toán
    [HttpPost("broadcast")]
    public async Task<ActionResult<ApiResponse<SystemBroadcastDto>>> UpdateBroadcast([FromBody] UpdateBroadcastRequest request)
    {
        var msg = request.Message?.Trim() ?? string.Empty;
        var type = string.IsNullOrWhiteSpace(request.Type) ? "info" : request.Type.Trim();
        var isActive = request.IsActive && !string.IsNullOrWhiteSpace(msg);
        SystemBroadcastState.Update(msg, type, isActive);

        var adminId = GetCurrentAdminId();
        var audit = new AdminActionHistory
        {
            AdminId = adminId,
            TargetUserId = adminId,
            Action = "UPDATE_BROADCAST",
            Reason = SystemBroadcastState.Type,
            Details = $"Cập nhật thông báo hệ thống: {SystemBroadcastState.Message} (Kích hoạt: {SystemBroadcastState.IsActive})",
            CreatedAt = DateTime.UtcNow
        };
        _context.AdminActionHistories.Add(audit);
        await _context.SaveChangesAsync();

        var dto = new SystemBroadcastDto(
            SystemBroadcastState.Message,
            SystemBroadcastState.Type,
            SystemBroadcastState.IsActive,
            SystemBroadcastState.UpdatedAt
        );
        return Ok(ApiResponse<SystemBroadcastDto>.Ok(dto, "Cập nhật thông báo phát sóng thành công."));
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
