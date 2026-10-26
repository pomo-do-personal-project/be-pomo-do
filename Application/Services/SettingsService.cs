using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.DTOs.Settings;
using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Services;

// Service handling retrieval and updates of user settings and pomodoro timers.
public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _settingsRepository;

    // Initializes a new instance of SettingsService.
    public SettingsService(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
    }

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
                ThemeMode = string.IsNullOrWhiteSpace(request.ThemeMode) ? "light" : request.ThemeMode.Trim().ToLower(),
                EmailWeeklyReport = request.EmailWeeklyReport,
                EmailDailyDigest = request.EmailDailyDigest,
                ReportEmail = string.IsNullOrWhiteSpace(request.ReportEmail) ? null : request.ReportEmail.Trim(),
                DailyDigestTime = string.IsNullOrWhiteSpace(request.DailyDigestTime) ? "07:00" : request.DailyDigestTime.Trim(),
                WeeklyReportTime = string.IsNullOrWhiteSpace(request.WeeklyReportTime) ? "08:00" : request.WeeklyReportTime.Trim()
            };
            await _settingsRepository.AddAsync(settings);
        }
        else
        {
            settings.FocusDuration = request.FocusDuration;
            settings.ShortBreakDuration = request.ShortBreakDuration;
            settings.LongBreakDuration = request.LongBreakDuration;
            settings.AutoStartBreaks = request.AutoStartBreaks;
            settings.EmailWeeklyReport = request.EmailWeeklyReport;
            settings.EmailDailyDigest = request.EmailDailyDigest;
            settings.ReportEmail = string.IsNullOrWhiteSpace(request.ReportEmail) ? null : request.ReportEmail.Trim();
            if (!string.IsNullOrWhiteSpace(request.DailyDigestTime))
            {
                settings.DailyDigestTime = request.DailyDigestTime.Trim();
            }
            if (!string.IsNullOrWhiteSpace(request.WeeklyReportTime))
            {
                settings.WeeklyReportTime = request.WeeklyReportTime.Trim();
            }
            if (!string.IsNullOrWhiteSpace(request.ThemeMode))
            {
                settings.ThemeMode = request.ThemeMode.Trim().ToLower();
            }

            settings.WallpaperUrl = request.WallpaperUrl;
            settings.AlarmSoundUrl = request.AlarmSoundUrl;
            settings.AmbientSoundUrl = request.AmbientSoundUrl;
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
            ThemeMode = entity.ThemeMode,
            EmailWeeklyReport = entity.EmailWeeklyReport,
            EmailDailyDigest = entity.EmailDailyDigest,
            ReportEmail = entity.ReportEmail,
            DailyDigestTime = entity.DailyDigestTime,
            WeeklyReportTime = entity.WeeklyReportTime,
            WallpaperUrl = entity.WallpaperUrl,
            AlarmSoundUrl = entity.AlarmSoundUrl,
            AmbientSoundUrl = entity.AmbientSoundUrl
        };
    }
}

