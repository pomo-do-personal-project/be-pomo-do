using be_pomo_do.Application.DTOs.Auth;

namespace be_pomo_do.Application.Services;

// Operations for user authentication, registration, and token generation
public interface IAuthService
{
    // Registers a new user account with default settings, streak, and token pair
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    // Authenticates user credentials and issues a new access/refresh token pair
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
