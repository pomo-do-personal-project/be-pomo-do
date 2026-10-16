using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace be_pomo_do.Infrastructure.BackgroundServices;

// Background worker tiêu thụ sự kiện từ Redis Streams và điều phối gửi email (XREADGROUP & XACK)
public class EmailReportConsumerWorker : BackgroundService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailReportConsumerWorker> _logger;
    private readonly string _consumerName = $"worker-{Environment.MachineName}-{Guid.NewGuid().ToString("N")[..6]}";

    // Khởi tạo EmailReportConsumerWorker với traditional constructor
    public EmailReportConsumerWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<EmailReportConsumerWorker> logger,
        IConnectionMultiplexer? redis = null)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _redis = redis;
    }

    // Vòng lặp chính xử lý hàng đợi Redis Streams
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_redis == null || !_redis.IsConnected)
        {
            _logger.LogWarning("Redis is not configured or connected. EmailReportConsumerWorker will stay idle.");
            return;
        }

        var db = _redis.GetDatabase();

        // Khởi tạo Consumer Group nếu chưa tồn tại
        try
        {
            await db.StreamCreateConsumerGroupAsync(
                RedisEmailReportQueue.StreamKey,
                RedisEmailReportQueue.ConsumerGroup,
                "$",
                createStream: true);
            _logger.LogInformation("Created Redis Stream consumer group '{Group}' on key '{Key}'",
                RedisEmailReportQueue.ConsumerGroup, RedisEmailReportQueue.StreamKey);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Consumer group đã tồn tại, tiếp tục lắng nghe bình thường
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create or verify Redis Stream consumer group.");
        }

        _logger.LogInformation("EmailReportConsumerWorker started with Consumer Name: {Name}", _consumerName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var entries = await db.StreamReadGroupAsync(
                    RedisEmailReportQueue.StreamKey,
                    RedisEmailReportQueue.ConsumerGroup,
                    _consumerName,
                    ">",
                    count: 5);

                if (entries == null || entries.Length == 0)
                {
                    await Task.Delay(2000, stoppingToken);
                    continue;
                }

                foreach (var entry in entries)
                {
                    await ProcessStreamEntryAsync(db, entry, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while polling or processing Redis Streams email report queue.");
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("EmailReportConsumerWorker has stopped gracefully.");
    }

    // Xử lý từng thông điệp trong Stream và xác nhận hoàn tất (XACK)
    private async Task ProcessStreamEntryAsync(IDatabase db, StreamEntry entry, CancellationToken ct)
    {
        var dict = entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());

        dict.TryGetValue("UserId", out var userIdStr);
        dict.TryGetValue("ReportType", out var reportType);
        dict.TryGetValue("TargetEmail", out var targetEmail);

        if (!Guid.TryParse(userIdStr, out var userId))
        {
            _logger.LogWarning("Invalid UserId in stream entry {Id}: {RawUserId}", entry.Id, userIdStr);
            await db.StreamAcknowledgeAsync(RedisEmailReportQueue.StreamKey, RedisEmailReportQueue.ConsumerGroup, entry.Id);
            return;
        }

        _logger.LogInformation("Processing report job {Id}: Type={Type}, User={UserId}", entry.Id, reportType, userId);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var reportService = scope.ServiceProvider.GetRequiredService<IEmailReportService>();

            if (string.Equals(reportType, "Weekly", StringComparison.OrdinalIgnoreCase))
            {
                await reportService.SendWeeklyReportAsync(userId, targetEmail, ct);
            }
            else if (string.Equals(reportType, "Daily", StringComparison.OrdinalIgnoreCase))
            {
                await reportService.SendDailyDigestAsync(userId, targetEmail, ct);
            }
            else
            {
                _logger.LogWarning("Unknown report type '{ReportType}' in stream entry {Id}", reportType, entry.Id);
            }

            // Gửi xác nhận đã xử lý thành công (XACK)
            await db.StreamAcknowledgeAsync(RedisEmailReportQueue.StreamKey, RedisEmailReportQueue.ConsumerGroup, entry.Id);
            _logger.LogInformation("Acknowledged (XACK) stream entry {Id} for User {UserId}", entry.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process email report job {Id} for User {UserId}", entry.Id, userId);
        }
    }
}
