namespace be_pomo_do.Application.DTOs.Auth;

// Data transfer object representing user profile information
public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    DateTime CreatedAt
);
