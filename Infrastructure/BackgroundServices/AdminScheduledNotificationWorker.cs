using be_pomo_do.Application.Common;
using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Core.Entities;
using be_pomo_do.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace be_pomo_do.Infrastructure.BackgroundServices;

// Background worker tự động gửi thông báo theo lịch với cơ chế Zero-Polling thông minh
public class AdminScheduledNotificationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ScheduledNotificationSignal _signal;
    private readonly ILogger<AdminScheduledNotificationWorker> _logger;

    public AdminScheduledNotificationWorker(
        IServiceScopeFactory scopeFactory,
        ScheduledNotificationSignal signal,
        ILogger<AdminScheduledNotificationWorker> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _signal = signal ?? throw new ArgumentNullException(nameof(signal));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AdminScheduledNotificationWorker (Zero-Polling) đã khởi động.");

        // 1. Kiểm tra ban đầu đúng 1 lần khi khởi động máy chủ
        await RefreshNextDueTimeAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nextDue = _signal.NextDueUtc;
                var now = DateTime.UtcNow;
                TimeSpan waitTime;

                if (!nextDue.HasValue)
                {
                    // Không có thư nào -> Ngủ yên tuyệt đối (0 câu lệnh DB, 0 gọi mạng Supabase)
                    _logger.LogInformation("Không có lịch hẹn nào. Worker chuyển sang chế độ ngủ yên (Zero-Polling)...");
                    waitTime = Timeout.InfiniteTimeSpan;
                }
                else if (nextDue.Value <= now)
                {
                    // Đã tới hạn gửi -> Xử lý ngay
                    await DispatchDueNotificationsAsync(stoppingToken);
                    await RefreshNextDueTimeAsync(stoppingToken);
                    continue;
                }
                else
                {
                    // Có thư trong tương lai -> Ngủ chính xác đến thời điểm đó
                    waitTime = nextDue.Value - now;
                    _logger.LogInformation("Worker ngủ chính xác {TotalSeconds:F1}s đến mốc hẹn tiếp theo: {DueUtc:u}",
                        waitTime.TotalSeconds, nextDue.Value);
                }

                // Chờ tín hiệu hoặc hết thời gian chờ
                var signaled = await _signal.WaitAsync(waitTime, stoppingToken);
                if (signaled)
                {
                    _logger.LogInformation("Nhận tín hiệu cờ hiệu thông minh. Cập nhật mốc hẹn mới...");
                    await RefreshNextDueTimeAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong tiến trình AdminScheduledNotificationWorker. Thử lại sau 15 giây.");
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }

        _logger.LogInformation("AdminScheduledNotificationWorker đã dừng an toàn.");
    }

    // Tải mốc thời gian hẹn gửi kế tiếp gần nhất từ CSDL
    private async Task RefreshNextDueTimeAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var earliest = await db.ScheduledNotifications
                .Where(s => !s.IsSent)
                .OrderBy(s => s.ScheduledAt)
                .Select(s => (DateTime?)s.ScheduledAt)
                .FirstOrDefaultAsync(stoppingToken);

            _signal.SetNextDueUtc(earliest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể cập nhật mốc hẹn kế tiếp từ CSDL.");
        }
    }

    // Xử lý gửi các thông báo đã đến hạn
    private async Task DispatchDueNotificationsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var now = DateTime.UtcNow;
        var dueItems = await db.ScheduledNotifications
            .Where(s => !s.IsSent && s.ScheduledAt <= now)
            .OrderBy(s => s.ScheduledAt)
            .ToListAsync(stoppingToken);

        if (dueItems.Count == 0) return;

        _logger.LogInformation("Bắt đầu xử lý {Count} thông báo đến hạn gửi...", dueItems.Count);

        foreach (var item in dueItems)
        {
            try
            {
                if (item.Scope.Equals("broadcast", StringComparison.OrdinalIgnoreCase))
                {
                    SystemBroadcastState.Update(item.Content, item.Category, true);
                }
                else
                {
                    var emails = (item.RecipientEmails ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Distinct().ToList();

                    var htmlBody = $"<div style='font-family:sans-serif;line-height:1.6;'>{item.Content.Replace("\n", "<br/>")}</div>";

                    // Gửi theo từng lô 5 email để bảo vệ Gmail SMTP không bị đánh dấu spam
                    for (int i = 0; i < emails.Count; i += 5)
                    {
                        var batch = emails.Skip(i).Take(5).ToList();
                        await Task.WhenAll(batch.Select(e => emailService.SendEmailReportAsync(e, item.Title, htmlBody)));
                        if (i + 5 < emails.Count) await Task.Delay(200, stoppingToken);
                    }
                }

                item.IsSent = true;
                item.SentAt = DateTime.UtcNow;

                var detailsText = item.Scope.Equals("broadcast", StringComparison.OrdinalIgnoreCase)
                    ? $"Cập nhật thông báo hệ thống: {item.Title} - {item.Content}"
                    : $"Gửi email cho {item.RecipientEmails}: {item.Title} - {item.Content}";

                db.AdminActionHistories.Add(new AdminActionHistory
                {
                    AdminId = item.AdminId,
                    TargetUserId = item.AdminId,
                    Action = item.Scope.Equals("broadcast", StringComparison.OrdinalIgnoreCase) ? "SCHEDULED_BROADCAST" : "SCHEDULED_EMAIL",
                    Reason = item.Title,
                    Details = detailsText,
                    CreatedAt = DateTime.UtcNow
                });

                await db.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Gửi thành công thông báo hẹn giờ: {Title} ({Id})", item.Title, item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Thất bại khi gửi thông báo hẹn giờ {Id}", item.Id);
            }
        }
    }
}
