using System.Security.Cryptography;
using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Core.Exceptions;
using be_pomo_do.Core.ValueObjects;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace be_pomo_do.Infrastructure.Services;

// Quản lý lưu trữ OTP và Reset Token trên Redis (hỗ trợ fallback In-Memory)
public class RedisOtpService : IOtpService
{
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan CooldownDuration = TimeSpan.FromSeconds(60);
    private const int MaxAttempts = 5;

    private readonly IConnectionMultiplexer _redis;
    private readonly InMemoryOtpService _fallback;
    private readonly ILogger<RedisOtpService> _logger;

    // Khởi tạo RedisOtpService với Redis connection và fallback store
    public RedisOtpService(
        IConnectionMultiplexer redis,
        InMemoryOtpService fallback,
        ILogger<RedisOtpService> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _fallback = fallback ?? throw new ArgumentNullException(nameof(fallback));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private IDatabase Db => _redis.GetDatabase();

    // Sinh và lưu mã OTP 6 số đăng ký vào Redis
    public async Task<string> GenerateAndSaveOtpAsync(string email, OtpPurpose purpose = OtpPurpose.Register, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalizedEmail = email.Trim().ToLowerInvariant();

        try
        {
            var otpKey = GetOtpKey(normalizedEmail, purpose);
            var cooldownKey = GetCooldownKey(normalizedEmail, purpose);
            var attemptsKey = GetAttemptsKey(normalizedEmail, purpose);

            var remainingCooldown = await GetRemainingCooldownSecondsAsync(normalizedEmail, purpose, ct);
            if (remainingCooldown > 0)
            {
                throw new DomainException($"Vui lòng chờ {remainingCooldown} giây trước khi yêu cầu gửi lại.");
            }

            var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            var db = Db;
            await db.StringSetAsync(otpKey, otpCode, OtpLifetime);
            await db.StringSetAsync(cooldownKey, "1", CooldownDuration);
            await db.KeyDeleteAsync(attemptsKey);

            _logger.LogInformation("Saved Register OTP for {Email} into Redis with TTL 5m.", normalizedEmail);
            return otpCode;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogWarning(ex, "Redis connection failed. Falling back to In-Memory OTP store.");
            return await _fallback.GenerateAndSaveOtpAsync(email, purpose, ct);
        }
    }

    // Xác thực mã OTP 6 số đăng ký từ Redis
    public async Task<bool> VerifyOtpAsync(string email, string otpCode, OtpPurpose purpose = OtpPurpose.Register, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(otpCode);

        var normalizedEmail = email.Trim().ToLowerInvariant();

        try
        {
            var otpKey = GetOtpKey(normalizedEmail, purpose);
            var attemptsKey = GetAttemptsKey(normalizedEmail, purpose);

            var db = Db;
            var storedOtp = await db.StringGetAsync(otpKey);
            if (storedOtp.IsNullOrEmpty)
            {
                return await _fallback.VerifyOtpAsync(email, otpCode, purpose, ct);
            }

            var attempts = await db.StringIncrementAsync(attemptsKey);
            if (attempts == 1)
            {
                await db.KeyExpireAsync(attemptsKey, OtpLifetime);
            }

            if (attempts > MaxAttempts)
            {
                await db.KeyDeleteAsync(otpKey);
                await db.KeyDeleteAsync(attemptsKey);
                throw new DomainException("Bạn đã nhập sai mã OTP quá 5 lần. Mã xác thực đã bị hủy vì lý do an toàn. Vui lòng yêu cầu mã mới.");
            }

            if (storedOtp.ToString() != otpCode.Trim())
            {
                var remaining = MaxAttempts - (int)attempts;
                throw new DomainException($"Mã OTP không chính xác. Bạn còn {remaining} lần thử.");
            }

            await db.KeyDeleteAsync(otpKey);
            await db.KeyDeleteAsync(attemptsKey);

            _logger.LogInformation("Successfully verified and consumed Register OTP for {Email}.", normalizedEmail);
            return true;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogWarning(ex, "Redis connection failed. Falling back to In-Memory OTP verification.");
            return await _fallback.VerifyOtpAsync(email, otpCode, purpose, ct);
        }
    }

    // Lấy thời gian cooldown còn lại của OTP
    public async Task<int> GetRemainingCooldownSecondsAsync(string email, OtpPurpose purpose = OtpPurpose.Register, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalizedEmail = email.Trim().ToLowerInvariant();

        try
        {
            var cooldownKey = GetCooldownKey(normalizedEmail, purpose);
            var ttl = await Db.KeyTimeToLiveAsync(cooldownKey);
            if (ttl.HasValue && ttl.Value.TotalSeconds > 0)
            {
                return (int)Math.Ceiling(ttl.Value.TotalSeconds);
            }

            return 0;
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogWarning(ex, "Redis connection failed. Falling back to In-Memory cooldown check.");
            return await _fallback.GetRemainingCooldownSecondsAsync(email, purpose, ct);
        }
    }

    // Sinh và lưu token 64 ký tự đặt lại mật khẩu vào Redis (hạn 15 phút, cooldown 60s)
    public async Task<string> GeneratePasswordResetTokenAsync(string email, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalizedEmail = email.Trim().ToLowerInvariant();

        try
        {
            var tokenKey = GetPasswordResetTokenKey(normalizedEmail);
            var cooldownKey = GetPasswordResetCooldownKey(normalizedEmail);
            var attemptsKey = GetPasswordResetAttemptsKey(normalizedEmail);

            var remainingCooldown = await GetPasswordResetCooldownSecondsAsync(normalizedEmail, ct);
            if (remainingCooldown > 0)
            {
                throw new DomainException($"Vui lòng chờ {remainingCooldown} giây trước khi yêu cầu gửi lại email xác nhận.");
            }

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

            var db = Db;
            await db.StringSetAsync(tokenKey, token, ResetTokenLifetime);
            await db.StringSetAsync(cooldownKey, "1", CooldownDuration);
            await db.KeyDeleteAsync(attemptsKey);

            _logger.LogInformation("Saved Password Reset Token for {Email} into Redis with TTL 15m.", normalizedEmail);
            return token;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogWarning(ex, "Redis connection failed. Falling back to In-Memory Password Reset Token store.");
            return await _fallback.GeneratePasswordResetTokenAsync(email, ct);
        }
    }

    // Xác thực token đặt lại mật khẩu từ link email
    public async Task<bool> VerifyPasswordResetTokenAsync(string email, string token, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var normalizedEmail = email.Trim().ToLowerInvariant();

        try
        {
            var tokenKey = GetPasswordResetTokenKey(normalizedEmail);
            var attemptsKey = GetPasswordResetAttemptsKey(normalizedEmail);

            var db = Db;
            var storedToken = await db.StringGetAsync(tokenKey);
            if (storedToken.IsNullOrEmpty)
            {
                return await _fallback.VerifyPasswordResetTokenAsync(email, token, ct);
            }

            var attempts = await db.StringIncrementAsync(attemptsKey);
            if (attempts == 1)
            {
                await db.KeyExpireAsync(attemptsKey, ResetTokenLifetime);
            }

            if (attempts > MaxAttempts)
            {
                await db.KeyDeleteAsync(tokenKey);
                await db.KeyDeleteAsync(attemptsKey);
                throw new DomainException("Liên kết xác nhận đã bị vô hiệu hóa vì nhập sai quá nhiều lần. Vui lòng yêu cầu liên kết mới.");
            }

            if (storedToken.ToString() != token.Trim())
            {
                throw new DomainException("Mã xác nhận đặt lại mật khẩu không chính xác.");
            }

            await db.KeyDeleteAsync(tokenKey);
            await db.KeyDeleteAsync(attemptsKey);

            _logger.LogInformation("Successfully verified and consumed Password Reset Token for {Email}.", normalizedEmail);
            return true;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogWarning(ex, "Redis connection failed. Falling back to In-Memory Password Reset Token verification.");
            return await _fallback.VerifyPasswordResetTokenAsync(email, token, ct);
        }
    }

    // Lấy thời gian cooldown còn lại của link đặt lại mật khẩu
    public async Task<int> GetPasswordResetCooldownSecondsAsync(string email, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalizedEmail = email.Trim().ToLowerInvariant();

        try
        {
            var cooldownKey = GetPasswordResetCooldownKey(normalizedEmail);
            var ttl = await Db.KeyTimeToLiveAsync(cooldownKey);
            if (ttl.HasValue && ttl.Value.TotalSeconds > 0)
            {
                return (int)Math.Ceiling(ttl.Value.TotalSeconds);
            }

            return 0;
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogWarning(ex, "Redis connection failed. Falling back to In-Memory Password Reset cooldown check.");
            return await _fallback.GetPasswordResetCooldownSecondsAsync(email, ct);
        }
    }

    private static string GetOtpKey(string email, OtpPurpose purpose) =>
        $"otp:{purpose.ToString().ToLowerInvariant()}:{email}";

    private static string GetCooldownKey(string email, OtpPurpose purpose) =>
        $"otp:cooldown:{purpose.ToString().ToLowerInvariant()}:{email}";

    private static string GetAttemptsKey(string email, OtpPurpose purpose) =>
        $"otp:attempts:{purpose.ToString().ToLowerInvariant()}:{email}";

    private static string GetPasswordResetTokenKey(string email) =>
        $"password_reset:token:{email}";

    private static string GetPasswordResetCooldownKey(string email) =>
        $"password_reset:cooldown:{email}";

    private static string GetPasswordResetAttemptsKey(string email) =>
        $"password_reset:attempts:{email}";
}
