using be_pomo_do.Application.DTOs.Auth;
using be_pomo_do.Application.Services;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace be_pomo_do.Presentation.Controllers;

/// <summary>
/// Handles user authentication, registration, and token management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService">The authentication service instance.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    /// <summary>
    /// Registers a new user account and generates initial access and refresh tokens.
    /// </summary>
    /// <param name="request">The registration details including email, password, and display name.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A response containing user details along with the generated access and refresh tokens.</returns>
    /// <response code="201">The account was successfully created.</response>
    /// <response code="400">The request contains invalid input or the email is already in use.</response>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        var response = ApiResponse<AuthResponse>.Ok(result, "Đăng ký tài khoản thành công.");
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Authenticates an existing user and returns access and refresh tokens.
    /// </summary>
    /// <param name="request">The login credentials (email and password).</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A response containing user details along with the new access and refresh tokens.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="400">Invalid email or password.</response>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        var response = ApiResponse<AuthResponse>.Ok(result, "Đăng nhập thành công.");
        return Ok(response);
    }
}
