using be_pomo_do.Core.Exceptions;
using be_pomo_do.Application.DTOs.Auth;
using be_pomo_do.Application.Services;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace be_pomo_do.Presentation.Controllers;

// Handles authentication, registration, token rotation, and session management.
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    // Initializes a new instance of AuthController with traditional constructor dependency injection.
    public AuthController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    // Dispatches a 6-digit OTP verification code to the requested email for account registration.
    [HttpPost("otp/send")]
    public async Task<ActionResult<ApiResponse<string>>> SendOtp([FromBody] SendOtpRequest request)
    {
        await _authService.SendOtpAsync(request);
        return Ok(ApiResponse<string>.Ok("Mã OTP đã được gửi về email của bạn. Mã có hiệu lực trong 5 phút."));
    }

    // Registers a new user account with verified OTP code (requires explicit login for tokens).
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        var response = ApiResponse<UserDto>.Ok(result, "Đăng ký tài khoản thành công. Vui lòng đăng nhập.");
        return StatusCode(StatusCodes.Status201Created, response);
    }

    // Initiates the forgot-password flow by sending an email with a secure confirmation button link.
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request);
        return Ok(ApiResponse<string>.Ok("Liên kết xác nhận đặt lại mật khẩu đã được gửi về email của bạn. Vui lòng kiểm tra hộp thư."));
    }

    // Verifies the token from confirmation email link and updates the user account password.
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authService.ResetPasswordAsync(request);
        return Ok(ApiResponse<string>.Ok("Đặt lại mật khẩu thành công. Vui lòng đăng nhập bằng mật khẩu mới."));
    }

    // Authenticates user credentials and issues a fresh access and refresh token pair.
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        SetTokenCookies(result.AccessToken, result.RefreshToken);
        var response = ApiResponse<AuthResponse>.Ok(result, "Đăng nhập thành công.");
        return Ok(response);
    }

    // Đăng nhập hoặc đăng ký tự động bằng Google ID Token
    [HttpPost("google")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> GoogleAuth([FromBody] GoogleAuthRequest request)
    {
        var result = await _authService.GoogleAuthAsync(request);
        SetTokenCookies(result.AccessToken, result.RefreshToken);
        var response = ApiResponse<AuthResponse>.Ok(result, "Đăng nhập Google thành công.");
        return Ok(response);
    }

    // Đăng nhập hoặc đăng ký tự động bằng Supabase Access Token
    [HttpPost("supabase")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> SupabaseAuth([FromBody] SupabaseAuthRequest request)
    {
        var result = await _authService.SupabaseAuthAsync(request);
        SetTokenCookies(result.AccessToken, result.RefreshToken);
        var response = ApiResponse<AuthResponse>.Ok(result, "Đăng nhập Supabase Google thành công.");
        return Ok(response);
    }

    // Rotates the refresh token and returns a newly issued access and refresh token pair.
    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest? request)
    {
        var refreshToken = Request.Cookies["refresh_token"] ?? request?.RefreshToken;

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            ClearTokenCookies();
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Không tìm thấy refresh token trong cookie hoặc payload."));
        }

        try
        {
            var result = await _authService.RefreshTokenAsync(refreshToken);
            SetTokenCookies(result.AccessToken, result.RefreshToken);
            var response = ApiResponse<AuthResponse>.Ok(result, "Làm mới token thành công.");
            return Ok(response);
        }
        catch (DomainException ex)
        {
            ClearTokenCookies();
            return Unauthorized(ApiResponse<AuthResponse>.Fail(ex.Message));
        }
    }

    // Retrieves profile details of the currently authenticated user.
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

    // Revokes the current refresh token and clears client authentication cookies.
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
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = DateTime.UtcNow.AddMinutes(15)
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("access_token", accessToken, accessCookieOptions);
        Response.Cookies.Append("refresh_token", refreshToken, refreshCookieOptions);
    }

    private void ClearTokenCookies()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        };

        Response.Cookies.Delete("access_token", cookieOptions);
        Response.Cookies.Delete("refresh_token", cookieOptions);
    }
}
