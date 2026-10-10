using System.Collections.Concurrent;
using System.Security.Cryptography;
using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Core.Exceptions;
using be_pomo_do.Core.ValueObjects;
using Microsoft.Extensions.Logging;

namespace be_pomo_do.Infrastructure.Services;

/// <summary>In-memory fallback implementation of <see cref="IOtpService"/> used when Redis is not configured.</summary>
public class InMemoryOtpService : IOtpService
{
    private record OtpEntry(string Code, DateTime ExpiresAt, int Attempts);

    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan CooldownDuration = TimeSpan.FromSeconds(60);
    private const int MaxAttempts = 5;

    private static readonly ConcurrentDictionary<string, OtpEntry> _otpStore = new();
    private static readonly ConcurrentDictionary<string, DateTime> _cooldownStore = new();

    private readonly ILogger<InMemoryOtpService> _logger;

    /// <summary>Initializes a new instance of the <see cref="InMemoryOtpService"/> class.</summary>
    /// <param name="logger">The logger instance.</param>
    public InMemoryOtpService(ILogger<InMemoryOtpService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task<string> GenerateAndSaveOtpAsync(string email, OtpPurpose purpose, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var key = $"{purpose}:{normalizedEmail}";

        if (_cooldownStore.TryGetValue(key, out var cooldownUntil) && cooldownUntil > DateTime.UtcNow)
        {
            var remaining = (int)Math.Ceiling((cooldownUntil - DateTime.UtcNow).TotalSeconds);
            throw new DomainException($"Vui lòng chờ {remaining} giây trước khi yêu cầu mã OTP mới.");
        }

        var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        _otpStore[key] = new OtpEntry(otpCode, DateTime.UtcNow.Add(OtpLifetime), 0);
        _cooldownStore[key] = DateTime.UtcNow.Add(CooldownDuration);

        _logger.LogInformation("[InMemory OTP] Generated OTP for {Email} ({Purpose}) with TTL 5m.", normalizedEmail, purpose);
        return Task.FromResult(otpCode);
    }

    /// <inheritdoc/>
    public Task<bool> VerifyOtpAsync(string email, string otpCode, OtpPurpose purpose, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(otpCode);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var key = $"{purpose}:{normalizedEmail}";

        if (!_otpStore.TryGetValue(key, out var entry) || entry.ExpiresAt < DateTime.UtcNow)
        {
            _otpStore.TryRemove(key, out _);
            throw new DomainException("Mã OTP không tồn tại hoặc đã hết hạn.");
        }

        var newAttempts = entry.Attempts + 1;
        if (newAttempts > MaxAttempts)
        {
            _otpStore.TryRemove(key, out _);
            throw new DomainException("Bạn đã nhập sai mã OTP quá 5 lần. Mã xác thực đã bị hủy vì lý do an toàn. Vui lòng yêu cầu mã mới.");
        }

        if (entry.Code != otpCode.Trim())
        {
            _otpStore[key] = entry with { Attempts = newAttempts };
            var remaining = MaxAttempts - newAttempts;
            throw new DomainException($"Mã OTP không chính xác. Bạn còn {remaining} lần thử.");
        }

        // Thành công: Xóa OTP
        _otpStore.TryRemove(key, out _);
        _logger.LogInformation("[InMemory OTP] Successfully verified and consumed OTP for {Email} ({Purpose}).", normalizedEmail, purpose);
        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public Task<int> GetRemainingCooldownSecondsAsync(string email, OtpPurpose purpose, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var key = $"{purpose}:{normalizedEmail}";

        if (_cooldownStore.TryGetValue(key, out var cooldownUntil) && cooldownUntil > DateTime.UtcNow)
        {
            return Task.FromResult((int)Math.Ceiling((cooldownUntil - DateTime.UtcNow).TotalSeconds));
        }

        return Task.FromResult(0);
    }
}
