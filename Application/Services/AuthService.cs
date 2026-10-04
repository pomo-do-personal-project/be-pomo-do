using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.DTOs.Auth;
using be_pomo_do.Core.Entities;
using be_pomo_do.Core.Exceptions;
using be_pomo_do.Core.ValueObjects;
using be_pomo_do.Infrastructure.Authentication;
using Microsoft.Extensions.Options;

namespace be_pomo_do.Application.Services;

/// <summary>Provides application business workflows for user authentication, registration, token rotation, and session management.</summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly JwtOptions _jwtOptions;

    /// <summary>Initializes a new instance of <see cref="AuthService"/>.</summary>
    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
        _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Kiểm tra email đã tồn tại hay chưa
        var emailExists = await _userRepository.ExistsByEmailAsync(request.Email);
        if (emailExists)
        {
            throw new DomainException("Email này đã được sử dụng. Vui lòng chọn email khác.");
        }

        // 2. Băm mật khẩu bằng BCrypt
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // 3. Khởi tạo User Entity
        var user = new User
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            DisplayName = request.DisplayName.Trim(),
            Provider = UserProvider.Local,
            CreatedAt = DateTime.UtcNow
        };

        // 4. Khởi tạo UserSettings mặc định (Focus: 25, ShortBreak: 5, LongBreak: 15)
        user.Settings = new UserSettings
        {
            UserId = user.Id,
            FocusDuration = 25,
            ShortBreakDuration = 5,
            LongBreakDuration = 15,
            AutoStartBreaks = false,
            ThemeMode = "light"
        };

        // 5. Khởi tạo DailyStreak mặc định
        user.Streak = new DailyStreak
        {
            UserId = user.Id,
            CurrentStreak = 0,
            LongestStreak = 0,
            LastActiveDate = null
        };

        // 6. Sinh Tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

        // 7. Tạo RefreshToken Entity và gắn vào User
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        user.RefreshTokens.Add(refreshToken);

        // 8. Lưu đồng thời toàn bộ vào Database qua UserRepository
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // 9. Trả về AuthResponse
        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.AvatarUrl,
            user.CreatedAt
        );

        return new AuthResponse(
            User: userDto,
            AccessToken: accessToken,
            RefreshToken: refreshTokenString,
            ExpiresInSeconds: _jwtOptions.AccessTokenExpirationMinutes * 60
        );
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // 1. Tìm người dùng theo Email
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user is null)
        {
            throw new DomainException("Email hoặc mật khẩu không chính xác.");
        }

        // 2. Kiểm tra mật khẩu tài khoản nội bộ (Local)
        if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new DomainException("Email hoặc mật khẩu không chính xác.");
        }

        // 3. Sinh Access Token (15 phút) và Refresh Token mới (7 ngày)
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

        // 4. Lưu Refresh Token mới vào Database
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _userRepository.AddRefreshTokenAsync(refreshToken);
        await _userRepository.SaveChangesAsync();

        // 5. Trả về AuthResponse
        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.AvatarUrl,
            user.CreatedAt
        );

        return new AuthResponse(
            User: userDto,
            AccessToken: accessToken,
            RefreshToken: refreshTokenString,
            ExpiresInSeconds: _jwtOptions.AccessTokenExpirationMinutes * 60
        );
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> RefreshTokenAsync(string refreshTokenString)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenString))
        {
            throw new DomainException("Refresh token không hợp lệ hoặc không tồn tại.");
        }

        // 1. Tìm refresh token trong CSDL
        var existingToken = await _userRepository.GetRefreshTokenWithUserAsync(refreshTokenString);
        if (existingToken is null)
        {
            throw new DomainException("Refresh token không hợp lệ hoặc không tồn tại.");
        }

        // 2. Kiểm tra token đã bị thu hồi hoặc hết hạn chưa
        if (existingToken.IsRevoked)
        {
            throw new DomainException("Phiên đăng nhập đã bị vô hiệu hóa. Vui lòng đăng nhập lại.");
        }

        if (existingToken.IsExpired)
        {
            throw new DomainException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
        }

        var user = existingToken.User;
        if (user is null)
        {
            throw new DomainException("Tài khoản người dùng không tồn tại.");
        }

        // 3. Cơ chế xoay vòng Refresh Token (Token Rotation): Hủy token cũ
        var newRefreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();
        existingToken.IsRevoked = true;
        existingToken.ReplacedByToken = newRefreshTokenString;

        // 4. Tạo refresh token mới
        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _userRepository.AddRefreshTokenAsync(newRefreshToken);

        // 5. Cấp mới Access Token
        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);

        await _userRepository.SaveChangesAsync();

        // 6. Trả về AuthResponse
        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.AvatarUrl,
            user.CreatedAt
        );

        return new AuthResponse(
            User: userDto,
            AccessToken: newAccessToken,
            RefreshToken: newRefreshTokenString,
            ExpiresInSeconds: _jwtOptions.AccessTokenExpirationMinutes * 60
        );
    }

    /// <inheritdoc/>
    public async Task<UserDto> GetMeAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            throw new DomainException("Không tìm thấy thông tin tài khoản người dùng.");
        }

        return new UserDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.AvatarUrl,
            user.CreatedAt
        );
    }

    /// <inheritdoc/>
    public async Task LogoutAsync(string? refreshTokenString)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenString))
        {
            return;
        }

        var token = await _userRepository.GetRefreshTokenWithUserAsync(refreshTokenString);
        if (token is not null && !token.IsRevoked)
        {
            token.IsRevoked = true;
            await _userRepository.SaveChangesAsync();
        }
    }
}
