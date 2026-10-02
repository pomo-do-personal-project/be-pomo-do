using be_pomo_do.Application.DTOs.Auth;

namespace be_pomo_do.Application.Services;

/// <summary>
/// Provides operations for user authentication, registration, and token generation.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account with default settings, initial streak, and authentication tokens.
    /// </summary>
    /// <param name="request">The registration request payload.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the authentication response.</returns>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user with email and password, issuing a new access token and refresh token.
    /// </summary>
    /// <param name="request">The login request payload.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the authentication response.</returns>
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
