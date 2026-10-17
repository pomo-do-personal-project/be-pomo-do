using be_pomo_do.Application.DTOs.Settings;

namespace be_pomo_do.Application.Services;

// Contract for managing user preferences and pomodoro duration configurations.
public interface ISettingsService
{
    // Gets user preferences, creating defaults if not present.
    Task<UserSettingsDto> GetSettingsAsync(Guid userId);

    // Updates user preferences and pomodoro durations.
    Task<UserSettingsDto> UpdateSettingsAsync(Guid userId, UpdateUserSettingsRequest request);
}
