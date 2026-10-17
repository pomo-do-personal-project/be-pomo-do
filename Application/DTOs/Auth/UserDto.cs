namespace be_pomo_do.Application.DTOs.Auth;

// DTO thông tin người dùng (không chứa token hay dữ liệu nhạy cảm)
public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    DateTime CreatedAt,
    bool IsAdmin = false,
    bool IsSuspended = false,
    string? SuspendedReason = null
);

