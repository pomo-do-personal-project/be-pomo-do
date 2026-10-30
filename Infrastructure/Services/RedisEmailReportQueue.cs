using be_pomo_do.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace be_pomo_do.Infrastructure.Services;

// Triển khai hàng đợi Redis Streams (XADD) phục vụ đẩy tác vụ gửi email báo cáo
public class RedisEmailReportQueue : IEmailReportQueue
{
    public const string StreamKey = "pomodo:stream:email-reports";
    public const string ConsumerGroup = "email-report-workers";

    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisEmailReportQueue> _logger;

    // Khởi tạo hàng đợi Redis Streams với IConnectionMultiplexer và Logger
    public RedisEmailReportQueue(
        ILogger<RedisEmailReportQueue> logger,
        IConnectionMultiplexer? redis = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _redis = redis;
    }

    // Đẩy thông điệp yêu cầu gửi email báo cáo vào Redis Stream (XADD)
    public async Task EnqueueReportAsync(Guid userId, string reportType, string? targetEmail = null, CancellationToken ct = default)
    {
        if (_redis == null || !_redis.IsConnected)
        {
            _logger.LogWarning("Redis is not connected. Skipping enqueue for UserId: {UserId}, ReportType: {ReportType}", userId, reportType);
            return;
        }

        try
        {
            var db = _redis.GetDatabase();

            // Chống đẩy trùng lặp nhiều lần trong cùng khoảng thời gian (TTL 2 giờ)
            var dedupKey = $"pomodo:inflight:{reportType.ToLower()}:{userId}:{DateTime.UtcNow:yyyyMMdd}";
            var acquired = await db.StringSetAsync(dedupKey, "queued", TimeSpan.FromHours(2), When.NotExists);
            if (!acquired)
            {
                _logger.LogDebug("Job {ReportType} cho User {UserId} đã được xếp hàng hôm nay. Bỏ qua để tránh gửi lặp.", reportType, userId);
                return;
            }

            var entries = new NameValueEntry[]
            {
                new("UserId", userId.ToString()),
                new("ReportType", reportType),
                new("TargetEmail", targetEmail ?? string.Empty),
                new("QueuedAt", DateTime.UtcNow.ToString("O"))
            };

            var messageId = await db.StreamAddAsync(StreamKey, entries);
            _logger.LogInformation("Enqueued email report job to Redis Stream: ID {MessageId}, Type: {ReportType}, User: {UserId}",
                messageId, reportType, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue email report job to Redis Stream for UserId: {UserId}", userId);
            throw;
        }
    }
}
