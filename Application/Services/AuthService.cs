using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.Common.Mappings;
using be_pomo_do.Application.DTOs.Auth;
using be_pomo_do.Core.Entities;
using be_pomo_do.Core.Exceptions;
using be_pomo_do.Core.ValueObjects;
using be_pomo_do.Infrastructure.Authentication;
using Microsoft.Extensions.Options;

namespace be_pomo_do.Application.Services;

// Provides application business workflows for user authentication, registration, token rotation, and session management.
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IOtpService otpService,
        IEmailService emailService,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
        _otpService = otpService ?? throw new ArgumentNullException(nameof(otpService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
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

        var user = new User
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            DisplayName = request.DisplayName.Trim(),
            Provider = UserProvider.Local,
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

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshToken = BuildRefreshToken(user.Id, refreshTokenString);
        await _userRepository.AddRefreshTokenAsync(refreshToken);
        await _userRepository.SaveChangesAsync();

        return new AuthResponse(user.ToDto(), accessToken, refreshTokenString, _jwtOptions.AccessTokenExpirationMinutes * 60);
    }

    // Làm mới access token bằng refresh token (Token Rotation)
    public async Task<AuthResponse> RefreshTokenAsync(string refreshTokenString)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenString))
        {
            throw new DomainException("Refresh token không hợp lệ hoặc không tồn tại.");
        }

        var existingToken = await _userRepository.GetRefreshTokenWithUserAsync(refreshTokenString);
        if (existingToken is null || existingToken.IsRevoked || existingToken.IsExpired)
        {
            throw new DomainException("Phiên đăng nhập không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.");
        }

        var user = existingToken.User ?? throw new DomainException("Tài khoản người dùng không tồn tại.");

        if (user.IsSuspended)
        {
            var reason = string.IsNullOrWhiteSpace(user.SuspendedReason) ? "Vi phạm điều khoản dịch vụ" : user.SuspendedReason;
            throw new DomainException($"Tài khoản của bạn đã bị tạm khóa. Lý do: {reason}");
        }

        var newRefreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();
        existingToken.IsRevoked = true;
        existingToken.ReplacedByToken = newRefreshTokenString;

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
            throw new DomainException("Email này đã được sử dụng. Vui lòng đăng nhập hoặc chọn Quên mật khẩu.");
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

        var user = await _userRepository.GetByEmailAsync(email)
            ?? throw new DomainException("Không tìm thấy tài khoản tương ứng với email này.");

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

    // Helper tạo entity RefreshToken
    private RefreshToken BuildRefreshToken(Guid userId, string token) => new()
    {
        UserId = userId,
        Token = token,
        ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
        CreatedAt = DateTime.UtcNow,
        IsRevoked = false
    };
}
