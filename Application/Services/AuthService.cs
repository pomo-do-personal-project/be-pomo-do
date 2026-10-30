using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.Common.Mappings;
using be_pomo_do.Application.DTOs.Auth;
using be_pomo_do.Core.Entities;
using be_pomo_do.Core.Exceptions;
using be_pomo_do.Core.ValueObjects;
using be_pomo_do.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using Google.Apis.Auth;

namespace be_pomo_do.Application.Services;

// Cung cấp luồng nghiệp vụ xác thực người dùng, đăng ký, xoay vòng token và quản lý phiên
public partial class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly JwtOptions _jwtOptions;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IOtpService otpService,
        IEmailService emailService,
        IOptions<JwtOptions> jwtOptions,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
        _otpService = otpService ?? throw new ArgumentNullException(nameof(otpService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    // Đăng ký tài khoản mới kèm xác thực mã OTP
    public async Task<UserDto> RegisterAsync(RegisterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var isOtpValid = await _otpService.VerifyOtpAsync(request.Email, request.OtpCode, OtpPurpose.Register);
        if (!isOtpValid)
        {
            throw new DomainException("Mã OTP không chính xác hoặc đã hết hạn.");
        }

        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            throw new DomainException("Email này đã được sử dụng. Vui lòng chọn email khác.");
        }

        var user = CreateNewUser(
            email: request.Email,
            displayName: request.DisplayName,
            provider: UserProvider.Local,
            passwordHash: _passwordHasher.HashPassword(request.Password));

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return user.ToDto();
    }

    // Đăng nhập tài khoản và cấp cặp token JWT
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user is null || string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new DomainException("Email hoặc mật khẩu không chính xác.");
        }

        if (user.IsSuspended)
        {
            var reason = string.IsNullOrWhiteSpace(user.SuspendedReason) ? "Vi phạm điều khoản dịch vụ" : user.SuspendedReason;
            throw new DomainException($"Tài khoản của bạn đã bị tạm khóa. Lý do: {reason}");
        }

        return await IssueAuthResponseAsync(user);
    }

    // Làm mới access token bằng refresh token (Token Rotation)
    public async Task<AuthResponse> RefreshTokenAsync(string refreshTokenString)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenString))
        {
            throw new DomainException("Refresh token không hợp lệ hoặc không tồn tại.");
        }

        var existingToken = await _userRepository.GetRefreshTokenWithUserAsync(refreshTokenString);
        if (existingToken is null)
        {
            throw new DomainException("Phiên đăng nhập không hợp lệ hoặc không tồn tại. Vui lòng đăng nhập lại.");
        }

        var user = existingToken.User ?? throw new DomainException("Tài khoản người dùng không tồn tại.");

        // PHÁT HIỆN TÁI SỬ DỤNG TOKEN (REUSE DETECTION - OWASP Standard):
        // Nếu token gửi lên đã bị thu hồi (IsRevoked = true), đây là dấu hiệu token bị rò rỉ và kẻ gian cố gắng tái sử dụng.
        // Ngay lập tức thu hồi toàn bộ chuỗi refresh token của user để bảo vệ tài khoản.
        if (existingToken.IsRevoked)
        {
            foreach (var rt in user.RefreshTokens.Where(t => !t.IsRevoked))
            {
                rt.IsRevoked = true;
            }
            await _userRepository.SaveChangesAsync();

            throw new DomainException("Phát hiện bất thường trong phiên đăng nhập (Token reuse). Để bảo vệ tài khoản, tất cả phiên làm việc đã bị thu hồi. Vui lòng đăng nhập lại.");
        }

        if (existingToken.IsExpired)
        {
            throw new DomainException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
        }

        if (user.IsSuspended)
        {
            var reason = string.IsNullOrWhiteSpace(user.SuspendedReason) ? "Vi phạm điều khoản dịch vụ" : user.SuspendedReason;
            throw new DomainException($"Tài khoản của bạn đã bị tạm khóa. Lý do: {reason}");
        }

        var newRefreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();
        existingToken.IsRevoked = true;
        existingToken.ReplacedByToken = HashToken(newRefreshTokenString);

        // Dọn dẹp token hết hạn hoặc đã thu hồi quá 7 ngày
        var staleTokens = user.RefreshTokens
            .Where(t => t.IsExpired || (t.IsRevoked && t.CreatedAt < DateTime.UtcNow.AddDays(-7)))
            .ToList();
        foreach (var stale in staleTokens)
        {
            _userRepository.RemoveRefreshToken(stale);
        }

        // Giới hạn tối đa 5 phiên làm việc đồng thời
        var activeTokens = user.RefreshTokens.Where(t => !t.IsRevoked && !t.IsExpired).OrderBy(t => t.CreatedAt).ToList();
        if (activeTokens.Count >= 5)
        {
            foreach (var oldToken in activeTokens.Take(activeTokens.Count - 4))
            {
                oldToken.IsRevoked = true;
            }
        }

        var newRefreshToken = BuildRefreshToken(user.Id, newRefreshTokenString);
        await _userRepository.AddRefreshTokenAsync(newRefreshToken);

        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        await _userRepository.SaveChangesAsync();

        return new AuthResponse(user.ToDto(), newAccessToken, newRefreshTokenString, _jwtOptions.AccessTokenExpirationMinutes * 60);
    }

    // Lấy thông tin tài khoản của người dùng hiện tại
    public async Task<UserDto> GetMeAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new DomainException("Không tìm thấy thông tin tài khoản người dùng.");
        return user.ToDto();
    }

    // Đăng xuất và thu hồi refresh token
    public async Task LogoutAsync(string? refreshTokenString)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenString)) return;

        var token = await _userRepository.GetRefreshTokenWithUserAsync(refreshTokenString);
        if (token is not null && !token.IsRevoked)
        {
            token.IsRevoked = true;
            await _userRepository.SaveChangesAsync();
        }
    }

    // Gửi mã OTP 6 số xác thực đăng ký tài khoản
    public async Task SendOtpAsync(SendOtpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByEmailAsync(email))
        {
            // Chống user enumeration: không tiết lộ email đã tồn tại hay chưa
            return;
        }

        var cooldown = await _otpService.GetRemainingCooldownSecondsAsync(email, request.Purpose);
        if (cooldown > 0)
        {
            throw new DomainException($"Vui lòng chờ {cooldown} giây trước khi yêu cầu gửi lại mã OTP mới.");
        }

        var otpCode = await _otpService.GenerateAndSaveOtpAsync(email, request.Purpose);
        await _emailService.SendOtpEmailAsync(email, otpCode);
    }

    // Gửi email chứa nút bấm xác nhận đặt lại mật khẩu
    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            // Chống user enumeration: không tiết lộ tài khoản có tồn tại hay không
            return;
        }

        var cooldown = await _otpService.GetPasswordResetCooldownSecondsAsync(email);
        if (cooldown > 0)
        {
            throw new DomainException($"Vui lòng chờ {cooldown} giây trước khi yêu cầu gửi lại email xác nhận.");
        }

        var resetToken = await _otpService.GeneratePasswordResetTokenAsync(email);
        await _emailService.SendPasswordResetEmailAsync(email, resetToken);
    }

    // Đặt lại mật khẩu mới bằng token từ link email
    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(email)
            ?? throw new DomainException("Không tìm thấy tài khoản tương ứng với email này.");

        if (!await _otpService.VerifyPasswordResetTokenAsync(email, request.Token))
        {
            throw new DomainException("Liên kết hoặc mã xác nhận đặt lại mật khẩu không chính xác hoặc đã hết hạn.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        foreach (var rt in user.RefreshTokens.Where(t => !t.IsRevoked))
        {
            rt.IsRevoked = true;
        }

        await _userRepository.SaveChangesAsync();
    }


    // Helper tạo entity User mới cùng cài đặt mặc định và streak khởi tạo (DRY)
    private static User CreateNewUser(
        string email,
        string displayName,
        UserProvider provider,
        string? passwordHash = null,
        string? providerId = null,
        string? avatarUrl = null)
    {
        var user = new User
        {
            Email = email.Trim().ToLowerInvariant(),
            DisplayName = displayName.Trim(),
            PasswordHash = passwordHash,
            Provider = provider,
            ProviderId = providerId,
            AvatarUrl = avatarUrl,
            CreatedAt = DateTime.UtcNow,
            Settings = new UserSettings
            {
                FocusDuration = 25,
                ShortBreakDuration = 5,
                LongBreakDuration = 15,
                AutoStartBreaks = false,
                ThemeMode = "light"
            },
            Streak = new DailyStreak { CurrentStreak = 0, LongestStreak = 0 }
        };
        user.Settings.UserId = user.Id;
        user.Streak.UserId = user.Id;
        return user;
    }

    // Helper phát hành cặp token JWT & RefreshToken đồng thời lưu trữ DB (DRY)
    private async Task<AuthResponse> IssueAuthResponseAsync(User user)
    {
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshToken = BuildRefreshToken(user.Id, refreshTokenString);
        await _userRepository.AddRefreshTokenAsync(refreshToken);
        await _userRepository.SaveChangesAsync();

        return new AuthResponse(user.ToDto(), accessToken, refreshTokenString, _jwtOptions.AccessTokenExpirationMinutes * 60);
    }

    // Helper tạo entity RefreshToken với token đã được băm SHA-256 an toàn
    private RefreshToken BuildRefreshToken(Guid userId, string token) => new()
    {
        UserId = userId,
        Token = HashToken(token),
        ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
        CreatedAt = DateTime.UtcNow,
        IsRevoked = false
    };

    public static string HashToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return string.Empty;
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
