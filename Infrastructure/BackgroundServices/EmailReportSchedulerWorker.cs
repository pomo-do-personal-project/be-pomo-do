using be_pomo_do.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace be_pomo_do.Infrastructure.BackgroundServices;

// Background worker lập lịch tự động kích hoạt đẩy job Daily Digest (07:00 AM) và Weekly Report (Thứ Hai) vào Redis Streams
public class EmailReportSchedulerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailReportSchedulerWorker> _logger;

    // Khởi tạo EmailReportSchedulerWorker qua Traditional Constructor
    public EmailReportSchedulerWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<EmailReportSchedulerWorker> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Vòng lặp kiểm tra định kỳ mỗi 60 giây
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailReportSchedulerWorker initialized. Monitoring scheduled email dispatch times...");

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));

        try
        {
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    var vnTime = TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.UtcNow,
                        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                    var today = DateOnly.FromDateTime(vnTime);
                    var currentTime = vnTime.ToString("HH:mm");

                    // 1. Quét và đẩy các job Daily Digest theo khung giờ tùy chỉnh của từng user
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var reportService = scope.ServiceProvider.GetRequiredService<IEmailReportService>();
                        await reportService.EnqueueScheduledDailyDigestsAsync(currentTime, stoppingToken);
                    }

                    // 2. Vào ngày Thứ Hai: Quét và đẩy các job Weekly Report theo khung giờ tùy chỉnh của từng user
                    if (vnTime.DayOfWeek == DayOfWeek.Monday)
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var reportService = scope.ServiceProvider.GetRequiredService<IEmailReportService>();
                            await reportService.EnqueueScheduledWeeklyReportsAsync(currentTime, stoppingToken);
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in EmailReportSchedulerWorker loop.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Host is shutting down, gracefully exit without throwing unhandled exception
        }

        _logger.LogInformation("EmailReportSchedulerWorker has stopped gracefully.");
    }
}
