namespace be_pomo_do.Application.DTOs.Auth;

/// <summary>Represents the user profile data transfer object.</summary>
public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    DateTime CreatedAt
);

