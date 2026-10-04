using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.DTOs.Settings;
using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Services;

/// <summary>Service handling retrieval and updates of user settings and pomodoro timers.</summary>
public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _settingsRepository;

    /// <summary>Initializes a new instance of <see cref="SettingsService"/>.</summary>
    public SettingsService(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
    }

    /// <inheritdoc/>
    public async Task<UserSettingsDto> GetSettingsAsync(Guid userId)
    {
        var settings = await _settingsRepository.GetByUserIdAsync(userId);
        if (settings == null)
        {
            settings = new UserSettings
            {
                UserId = userId,
                FocusDuration = 25,
                ShortBreakDuration = 5,
                LongBreakDuration = 15,
                AutoStartBreaks = false,
                ThemeMode = "light"
            };
            await _settingsRepository.AddAsync(settings);
            await _settingsRepository.SaveChangesAsync();
        }

        return MapToDto(settings);
    }

    /// <inheritdoc/>
    public async Task<UserSettingsDto> UpdateSettingsAsync(Guid userId, UpdateUserSettingsRequest request)
    {
        var settings = await _settingsRepository.GetByUserIdAsync(userId);
        if (settings == null)
        {
            settings = new UserSettings
            {
                UserId = userId,
                FocusDuration = request.FocusDuration,
                ShortBreakDuration = request.ShortBreakDuration,
                LongBreakDuration = request.LongBreakDuration,
                AutoStartBreaks = request.AutoStartBreaks,
                ThemeMode = string.IsNullOrWhiteSpace(request.ThemeMode) ? "light" : request.ThemeMode.Trim().ToLower()
            };
            await _settingsRepository.AddAsync(settings);
        }
        else
        {
            settings.FocusDuration = request.FocusDuration;
            settings.ShortBreakDuration = request.ShortBreakDuration;
            settings.LongBreakDuration = request.LongBreakDuration;
            settings.AutoStartBreaks = request.AutoStartBreaks;
            if (!string.IsNullOrWhiteSpace(request.ThemeMode))
            {
                settings.ThemeMode = request.ThemeMode.Trim().ToLower();
            }
        }

        await _settingsRepository.SaveChangesAsync();
        return MapToDto(settings);
    }

    private static UserSettingsDto MapToDto(UserSettings entity)
    {
        return new UserSettingsDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            FocusDuration = entity.FocusDuration,
            ShortBreakDuration = entity.ShortBreakDuration,
            LongBreakDuration = entity.LongBreakDuration,
            AutoStartBreaks = entity.AutoStartBreaks,
            ThemeMode = entity.ThemeMode
        };
    }
}
