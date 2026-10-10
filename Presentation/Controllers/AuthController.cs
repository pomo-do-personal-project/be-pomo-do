using be_pomo_do.Application.DTOs.Auth;
using be_pomo_do.Application.Services;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace be_pomo_do.Presentation.Controllers;

/// <summary>Handles authentication, registration, token rotation, and session management.</summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>Initializes a new instance of <see cref="AuthController"/> with traditional constructor dependency injection.</summary>
    /// <param name="authService">The authentication application service instance.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    /// <summary>Dispatches an OTP verification code to the requested email for registration or password recovery.</summary>
    /// <param name="request">The OTP dispatch request parameters.</param>
    /// <returns>An API response indicating dispatch status.</returns>
    [HttpPost("otp/send")]
    public async Task<ActionResult<ApiResponse<string>>> SendOtp([FromBody] SendOtpRequest request)
    {
        await _authService.SendOtpAsync(request);
        return Ok(ApiResponse<string>.Ok("Mã OTP đã được gửi về email của bạn. Mã có hiệu lực trong 5 phút."));
    }

    /// <summary>Registers a new user account with verified OTP code and returns an authentication payload.</summary>
    /// <param name="request">The user registration payload including the verified OTP.</param>
    /// <returns>An API response containing tokens and created user profile.</returns>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        SetTokenCookies(result.AccessToken, result.RefreshToken);
        var response = ApiResponse<AuthResponse>.Ok(result, "Đăng ký tài khoản thành công.");
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>Initiates the forgot-password flow by verifying account existence and dispatching a recovery OTP.</summary>
    /// <param name="request">The forgot password request containing the account email.</param>
    /// <returns>An API response confirming recovery OTP dispatch.</returns>
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request);
        return Ok(ApiResponse<string>.Ok("Mã xác thực đã được gửi về email của bạn."));
    }

    /// <summary>Verifies the recovery OTP and updates the user account password.</summary>
    /// <param name="request">The password reset request containing email, OTP code, and new password.</param>
    /// <returns>An API response confirming the password has been reset.</returns>
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authService.ResetPasswordAsync(request);
        return Ok(ApiResponse<string>.Ok("Đặt lại mật khẩu thành công. Vui lòng đăng nhập bằng mật khẩu mới."));
    }

    /// <summary>Authenticates user credentials and issues a fresh access and refresh token pair.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        SetTokenCookies(result.AccessToken, result.RefreshToken);
        var response = ApiResponse<AuthResponse>.Ok(result, "Đăng nhập thành công.");
        return Ok(response);
    }

    /// <summary>Rotates the refresh token and returns a newly issued access and refresh token pair.</summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest? request)
    {
        var refreshToken = Request.Cookies["refresh_token"] ?? request?.RefreshToken;

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail("Không tìm thấy refresh token trong cookie hoặc payload."));
        }

        var result = await _authService.RefreshTokenAsync(refreshToken);
        SetTokenCookies(result.AccessToken, result.RefreshToken);
        var response = ApiResponse<AuthResponse>.Ok(result, "Làm mới token thành công.");
        return Ok(response);
    }

    /// <summary>Retrieves profile details of the currently authenticated user.</summary>
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetMe()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<UserDto>.Fail("Không xác định được danh tính người dùng từ token."));
        }

        var userDto = await _authService.GetMeAsync(userId);
        return Ok(ApiResponse<UserDto>.Ok(userDto, "Lấy thông tin tài khoản thành công."));
    }

    /// <summary>Revokes the current refresh token and clears client authentication cookies.</summary>
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<string>>> Logout([FromBody] RefreshTokenRequest? request)
    {
        var refreshToken = Request.Cookies["refresh_token"] ?? request?.RefreshToken;

        await _authService.LogoutAsync(refreshToken);
        ClearTokenCookies();

        return Ok(ApiResponse<string>.Ok("Đăng xuất thành công."));
    }

    private void SetTokenCookies(string accessToken, string refreshToken)
    {
        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(15)
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("access_token", accessToken, accessCookieOptions);
        Response.Cookies.Append("refresh_token", refreshToken, refreshCookieOptions);
    }

    private void ClearTokenCookies()
    {
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token", new CookieOptions
        {
            Path = "/api/auth"
        });
    }
}
