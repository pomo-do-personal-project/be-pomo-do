using System.Security.Cryptography;
using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Core.Exceptions;
using be_pomo_do.Core.ValueObjects;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace be_pomo_do.Infrastructure.Services;

/// <summary>Redis implementation of <see cref="IOtpService"/> handling atomic OTP storage, cooldown enforcement, and brute-force mitigation.</summary>
public class RedisOtpService : IOtpService
{
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan CooldownDuration = TimeSpan.FromSeconds(60);
    private const int MaxAttempts = 5;

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisOtpService> _logger;

    /// <summary>Initializes a new instance of the <see cref="RedisOtpService"/> class.</summary>
    /// <param name="redis">The Redis connection multiplexer instance.</param>
    /// <param name="logger">The logger instance.</param>
    public RedisOtpService(IConnectionMultiplexer redis, ILogger<RedisOtpService> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private IDatabase Db => _redis.GetDatabase();

    /// <inheritdoc/>
    public async Task<string> GenerateAndSaveOtpAsync(string email, OtpPurpose purpose, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var otpKey = GetOtpKey(normalizedEmail, purpose);
        var cooldownKey = GetCooldownKey(normalizedEmail, purpose);
        var attemptsKey = GetAttemptsKey(normalizedEmail, purpose);

        // Kiểm tra cooldown
        var remainingCooldown = await GetRemainingCooldownSecondsAsync(normalizedEmail, purpose, ct);
        if (remainingCooldown > 0)
        {
            throw new DomainException($"Vui lòng chờ {remainingCooldown} giây trước khi yêu cầu mã OTP mới.");
        }

        // Sinh mã 6 chữ số ngẫu nhiên chuẩn mã hóa an toàn (Cryptographically secure)
        var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        // Lưu vào Redis: OTP (5 phút), Cooldown (60s), và xóa đếm lỗi cũ nếu có
        var db = Db;
        await db.StringSetAsync(otpKey, otpCode, OtpLifetime);
        await db.StringSetAsync(cooldownKey, "1", CooldownDuration);
        await db.KeyDeleteAsync(attemptsKey);

        _logger.LogInformation("Saved OTP for {Email} ({Purpose}) with TTL {Ttl}s into Redis.", normalizedEmail, purpose, OtpLifetime.TotalSeconds);
        return otpCode;
    }

    /// <inheritdoc/>
    public async Task<bool> VerifyOtpAsync(string email, string otpCode, OtpPurpose purpose, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(otpCode);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var otpKey = GetOtpKey(normalizedEmail, purpose);
        var attemptsKey = GetAttemptsKey(normalizedEmail, purpose);

        var db = Db;
        var storedOtp = await db.StringGetAsync(otpKey);
        if (storedOtp.IsNullOrEmpty)
        {
            throw new DomainException("Mã OTP không tồn tại hoặc đã hết hạn.");
        }

        // Tăng số lần thử nhập
        var attempts = await db.StringIncrementAsync(attemptsKey);
        if (attempts == 1)
        {
            await db.KeyExpireAsync(attemptsKey, OtpLifetime);
        }

        if (attempts > MaxAttempts)
        {
            // Hủy luôn mã OTP khi vượt quá 5 lần nhập sai
            await db.KeyDeleteAsync(otpKey);
            await db.KeyDeleteAsync(attemptsKey);
            throw new DomainException("Bạn đã nhập sai mã OTP quá 5 lần. Mã xác thực đã bị hủy vì lý do an toàn. Vui lòng yêu cầu mã mới.");
        }

        if (storedOtp.ToString() != otpCode.Trim())
        {
            var remaining = MaxAttempts - (int)attempts;
            throw new DomainException($"Mã OTP không chính xác. Bạn còn {remaining} lần thử.");
        }

        // Xác thực thành công: Xóa mã OTP và số lần thử khỏi Redis (Single-use)
        await db.KeyDeleteAsync(otpKey);
        await db.KeyDeleteAsync(attemptsKey);

        _logger.LogInformation("Successfully verified and consumed OTP for {Email} ({Purpose}).", normalizedEmail, purpose);
        return true;
    }

    /// <inheritdoc/>
    public async Task<int> GetRemainingCooldownSecondsAsync(string email, OtpPurpose purpose, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var cooldownKey = GetCooldownKey(normalizedEmail, purpose);

        var ttl = await Db.KeyTimeToLiveAsync(cooldownKey);
        if (ttl.HasValue && ttl.Value.TotalSeconds > 0)
        {
            return (int)Math.Ceiling(ttl.Value.TotalSeconds);
        }

        return 0;
    }

    private static string GetOtpKey(string email, OtpPurpose purpose) =>
        $"otp:{purpose.ToString().ToLowerInvariant()}:{email}";

    private static string GetCooldownKey(string email, OtpPurpose purpose) =>
        $"otp:cooldown:{purpose.ToString().ToLowerInvariant()}:{email}";

    private static string GetAttemptsKey(string email, OtpPurpose purpose) =>
        $"otp:attempts:{purpose.ToString().ToLowerInvariant()}:{email}";
}
