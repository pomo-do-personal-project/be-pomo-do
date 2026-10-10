using be_pomo_do.Application.DTOs.Auth;

namespace be_pomo_do.Application.Services;

/// <summary>Defines application service operations for user registration, authentication, token rotation, and session management.</summary>
public interface IAuthService
{
    /// <summary>Registers a new user account with default user settings, daily streak, and initial token pair.</summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    /// <summary>Authenticates user credentials and issues a fresh access and refresh token pair.</summary>
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>Validates an active refresh token, revokes it (token rotation), and issues a new access and refresh token pair.</summary>
    Task<AuthResponse> RefreshTokenAsync(string refreshTokenString);

    /// <summary>Retrieves user profile information for the authenticated user identifier.</summary>
    Task<UserDto> GetMeAsync(Guid userId);

    /// <summary>Generates and dispatches an OTP code to verify email address for registration or password recovery.</summary>
    Task SendOtpAsync(SendOtpRequest request);

    /// <summary>Initiates the forgot-password flow by verifying account existence and dispatching a recovery OTP.</summary>
    Task ForgotPasswordAsync(ForgotPasswordRequest request);

    /// <summary>Resets a user's password after verifying the submitted OTP and revoking existing refresh tokens.</summary>
    Task ResetPasswordAsync(ResetPasswordRequest request);

    /// <summary>Revokes the specified refresh token in the persistence store to terminate the session.</summary>
    Task LogoutAsync(string? refreshTokenString);
}
