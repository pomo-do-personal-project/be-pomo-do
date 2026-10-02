using be_pomo_do.Application.DTOs.Auth;
using be_pomo_do.Application.Services;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace be_pomo_do.Presentation.Controllers;

/// <summary>Handles authentication, registration, token rotation, and session management.</summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService ?? throw new ArgumentNullException(nameof(authService));

    /// <summary>Registers a new user account and returns an authentication payload with initial tokens.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        SetTokenCookies(result.AccessToken, result.RefreshToken);
        var response = ApiResponse<AuthResponse>.Ok(result, "Đăng ký tài khoản thành công.");
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>Authenticates user credentials and issues a fresh access and refresh token pair.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        SetTokenCookies(result.AccessToken, result.RefreshToken);
        var response = ApiResponse<AuthResponse>.Ok(result, "Đăng nhập thành công.");
        return Ok(response);
    }

    /// <summary>Rotates the refresh token and returns a newly issued access and refresh token pair.</summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken(
        [FromBody] RefreshTokenRequest? request,
        CancellationToken cancellationToken)
    {
        // Ưu tiên đọc refresh token từ Cookie BFF, nếu không có thì đọc từ request body
        var refreshToken = Request.Cookies["refresh_token"] ?? request?.RefreshToken;

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail("Không tìm thấy refresh token trong cookie hoặc payload."));
        }

        var result = await _authService.RefreshTokenAsync(refreshToken, cancellationToken);
        SetTokenCookies(result.AccessToken, result.RefreshToken);
        var response = ApiResponse<AuthResponse>.Ok(result, "Làm mới token thành công.");
        return Ok(response);
    }

    /// <summary>Retrieves profile details of the currently authenticated user.</summary>
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetMe(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<UserDto>.Fail("Không xác định được danh tính người dùng từ token."));
        }

        var userDto = await _authService.GetMeAsync(userId, cancellationToken);
        return Ok(ApiResponse<UserDto>.Ok(userDto, "Lấy thông tin tài khoản thành công."));
    }

    /// <summary>Revokes the current refresh token and clears client authentication cookies.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> Logout(
        [FromBody] RefreshTokenRequest? request,
        CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refresh_token"] ?? request?.RefreshToken;

        await _authService.LogoutAsync(refreshToken, cancellationToken);
        ClearTokenCookies();

        return Ok(ApiResponse<string>.Ok("Đăng xuất thành công."));
    }

    // Appends HttpOnly, Secure, SameSite cookies for access and refresh tokens (BFF pattern)
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

    // Clears auth cookies on logout
    private void ClearTokenCookies()
    {
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token", new CookieOptions
        {
            Path = "/api/auth"
        });
    }
}
