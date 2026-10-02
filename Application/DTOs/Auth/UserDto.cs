namespace be_pomo_do.Application.DTOs.Auth;

/// <summary>
/// Data transfer object representing user profile information.
/// </summary>
/// <param name="Id">The unique identifier of the user.</param>
/// <param name="Email">The user's registered email address.</param>
/// <param name="DisplayName">The user's public display name.</param>
/// <param name="AvatarUrl">The URL of the user's avatar image, if available.</param>
/// <param name="CreatedAt">The timestamp in UTC when the user was created.</param>
public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    DateTime CreatedAt
);
