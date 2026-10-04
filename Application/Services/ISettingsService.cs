using be_pomo_do.Application.DTOs.Settings;

namespace be_pomo_do.Application.Services;

/// <summary>Contract for managing user preferences and pomodoro duration configurations.</summary>
public interface ISettingsService
{
    /// <summary>Gets user preferences, creating defaults if not present.</summary>
    Task<UserSettingsDto> GetSettingsAsync(Guid userId);

    /// <summary>Updates user preferences and pomodoro durations.</summary>
    Task<UserSettingsDto> UpdateSettingsAsync(Guid userId, UpdateUserSettingsRequest request);
}
