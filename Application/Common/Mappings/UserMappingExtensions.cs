using be_pomo_do.Application.DTOs.Auth;
using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Common.Mappings;

// Extension methods for User entity mappings.
public static class UserMappingExtensions
{
    // Chuyển đổi User entity sang UserDto
    public static UserDto ToDto(this User user)
    {
        return new UserDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.AvatarUrl,
            user.CreatedAt,
            user.IsAdmin,
            user.IsSuspended,
            user.SuspendedReason
        );
    }
}
