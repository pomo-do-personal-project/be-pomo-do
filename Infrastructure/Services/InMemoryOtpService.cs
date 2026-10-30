using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Core.Exceptions;
using be_pomo_do.Core.ValueObjects;
using Microsoft.Extensions.Logging;

namespace be_pomo_do.Infrastructure.Services;

// Lưu trữ và quản lý OTP/Token trong bộ nhớ (fallback khi Redis gặp sự cố)
public class InMemoryOtpService : IOtpService
{
    private record OtpEntry(string Code, DateTime ExpiresAt, int Attempts);

    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan CooldownDuration = TimeSpan.FromSeconds(60);
    private const int MaxAttempts = 5;

    private static readonly ConcurrentDictionary<string, OtpEntry> _otpStore = new();
    private static readonly ConcurrentDictionary<string, DateTime> _cooldownStore = new();

    private readonly ILogger<InMemoryOtpService> _logger;

    private static void PurgeExpiredEntries()
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in _otpStore)
        {
            if (kvp.Value.ExpiresAt <= now)
                _otpStore.TryRemove(kvp.Key, out _);
        }
        foreach (var kvp in _cooldownStore)
        {
            if (kvp.Value <= now)
                _cooldownStore.TryRemove(kvp.Key, out _);
        }
    }

    // Khởi tạo InMemoryOtpService với logger
    public InMemoryOtpService(ILogger<InMemoryOtpService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Sinh và lưu mã OTP 6 số đăng ký
    public Task<string> GenerateAndSaveOtpAsync(string email, OtpPurpose purpose = OtpPurpose.Register, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        PurgeExpiredEntries();

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var key = $"otp:{purpose.ToString().ToLowerInvariant()}:{normalizedEmail}";

        if (_cooldownStore.TryGetValue(key, out var cooldownUntil) && cooldownUntil > DateTime.UtcNow)
        {
            var remaining = (int)Math.Ceiling((cooldownUntil - DateTime.UtcNow).TotalSeconds);
            throw new DomainException($"Vui lòng chờ {remaining} giây trước khi yêu cầu gửi lại.");
        }

        var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        _otpStore[key] = new OtpEntry(otpCode, DateTime.UtcNow.Add(OtpLifetime), 0);
        _cooldownStore[key] = DateTime.UtcNow.Add(CooldownDuration);

        _logger.LogInformation("[InMemory OTP] Generated 6-digit OTP for {Email} ({Purpose}) with TTL 5m.", normalizedEmail, purpose);
        return Task.FromResult(otpCode);
    }

    // Xác thực mã OTP 6 số đăng ký
    public Task<bool> VerifyOtpAsync(string email, string otpCode, OtpPurpose purpose = OtpPurpose.Register, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(otpCode);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var key = $"otp:{purpose.ToString().ToLowerInvariant()}:{normalizedEmail}";

        while (true)
        {
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

            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(entry.Code),
                    Encoding.UTF8.GetBytes(otpCode.Trim())))
            {
                var updated = entry with { Attempts = newAttempts };
                if (!_otpStore.TryUpdate(key, updated, entry))
                    continue;

                var remaining = MaxAttempts - newAttempts;
                throw new DomainException($"Mã OTP không chính xác. Bạn còn {remaining} lần thử.");
            }

            // Thành công: Xóa OTP
            _otpStore.TryRemove(key, out _);
            _logger.LogInformation("[InMemory OTP] Successfully verified and consumed OTP for {Email} ({Purpose}).", normalizedEmail, purpose);
            return Task.FromResult(true);
        }
    }

    // Lấy thời gian cooldown còn lại của OTP
    public Task<int> GetRemainingCooldownSecondsAsync(string email, OtpPurpose purpose = OtpPurpose.Register, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var key = $"otp:{purpose.ToString().ToLowerInvariant()}:{normalizedEmail}";

        if (_cooldownStore.TryGetValue(key, out var cooldownUntil) && cooldownUntil > DateTime.UtcNow)
        {
            return Task.FromResult((int)Math.Ceiling((cooldownUntil - DateTime.UtcNow).TotalSeconds));
        }

        return Task.FromResult(0);
    }

    // Sinh và lưu token 64 ký tự đặt lại mật khẩu (hạn 15 phút, cooldown 60s)
    public Task<string> GeneratePasswordResetTokenAsync(string email, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        PurgeExpiredEntries();

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var key = $"reset_token:{normalizedEmail}";

        if (_cooldownStore.TryGetValue(key, out var cooldownUntil) && cooldownUntil > DateTime.UtcNow)
        {
            var remaining = (int)Math.Ceiling((cooldownUntil - DateTime.UtcNow).TotalSeconds);
            throw new DomainException($"Vui lòng chờ {remaining} giây trước khi yêu cầu gửi lại email xác nhận.");
        }

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

        _otpStore[key] = new OtpEntry(token, DateTime.UtcNow.Add(ResetTokenLifetime), 0);
        _cooldownStore[key] = DateTime.UtcNow.Add(CooldownDuration);

        _logger.LogInformation("[InMemory Token] Generated Password Reset Token for {Email} with TTL 15m.", normalizedEmail);
        return Task.FromResult(token);
    }

    // Xác thực token đặt lại mật khẩu từ link email
    public Task<bool> VerifyPasswordResetTokenAsync(string email, string token, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var key = $"reset_token:{normalizedEmail}";

        while (true)
        {
            if (!_otpStore.TryGetValue(key, out var entry) || entry.ExpiresAt < DateTime.UtcNow)
            {
                _otpStore.TryRemove(key, out _);
                throw new DomainException("Liên kết hoặc mã xác nhận đặt lại mật khẩu không tồn tại hoặc đã hết hạn.");
            }

            var newAttempts = entry.Attempts + 1;
            if (newAttempts > MaxAttempts)
            {
                _otpStore.TryRemove(key, out _);
                throw new DomainException("Liên kết xác nhận đã bị vô hiệu hóa vì nhập sai quá nhiều lần. Vui lòng yêu cầu liên kết mới.");
            }

            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(entry.Code),
                    Encoding.UTF8.GetBytes(token.Trim())))
            {
                var updated = entry with { Attempts = newAttempts };
                if (!_otpStore.TryUpdate(key, updated, entry))
                    continue;

                throw new DomainException("Mã xác nhận đặt lại mật khẩu không chính xác.");
            }

            _otpStore.TryRemove(key, out _);
            _logger.LogInformation("[InMemory Token] Successfully verified and consumed Password Reset Token for {Email}.", normalizedEmail);
            return Task.FromResult(true);
        }
    }

    // Lấy thời gian cooldown còn lại của link đặt lại mật khẩu
    public Task<int> GetPasswordResetCooldownSecondsAsync(string email, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var key = $"reset_token:{normalizedEmail}";

        if (_cooldownStore.TryGetValue(key, out var cooldownUntil) && cooldownUntil > DateTime.UtcNow)
        {
            return Task.FromResult((int)Math.Ceiling((cooldownUntil - DateTime.UtcNow).TotalSeconds));
        }

        return Task.FromResult(0);
    }
}
