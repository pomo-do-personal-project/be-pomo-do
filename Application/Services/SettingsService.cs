using System.Net.Mail;
using System.Text.RegularExpressions;
using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.DTOs.Settings;
using be_pomo_do.Core.Entities;
using be_pomo_do.Core.Exceptions;

namespace be_pomo_do.Application.Services;

// Service handling retrieval and updates of user settings and pomodoro timers.
public class SettingsService : ISettingsService
{
    private static readonly Regex TimeFormatRegex = new(@"^([01]\d|2[0-3]):[0-5]\d$", RegexOptions.Compiled);
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
        ArgumentNullException.ThrowIfNull(request);
        ValidateSettingsRequest(request);

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

    private static void ValidateSettingsRequest(UpdateUserSettingsRequest request)
    {
        if (request.FocusDuration is < 1 or > 180)
            throw new DomainException("Thời gian tập trung phải từ 1 đến 180 phút.");

        if (request.ShortBreakDuration is < 1 or > 60)
            throw new DomainException("Thời gian nghỉ ngắn phải từ 1 đến 60 phút.");

        if (request.LongBreakDuration is < 1 or > 60)
            throw new DomainException("Thời gian nghỉ dài phải từ 1 đến 60 phút.");

        if (!string.IsNullOrWhiteSpace(request.DailyDigestTime) && !TimeFormatRegex.IsMatch(request.DailyDigestTime.Trim()))
            throw new DomainException("Định dạng giờ Daily Digest không hợp lệ. Vui lòng chọn giờ theo chuẩn HH:mm (00:00 - 23:59).");

        if (!string.IsNullOrWhiteSpace(request.WeeklyReportTime) && !TimeFormatRegex.IsMatch(request.WeeklyReportTime.Trim()))
            throw new DomainException("Định dạng giờ Weekly Report không hợp lệ. Vui lòng chọn giờ theo chuẩn HH:mm (00:00 - 23:59).");

        if (!string.IsNullOrWhiteSpace(request.ReportEmail) && !MailAddress.TryCreate(request.ReportEmail.Trim(), out _))
            throw new DomainException("Địa chỉ email nhận báo cáo không hợp lệ.");

        ValidateMediaUrl(request.WallpaperUrl, "Hình nền");
        ValidateMediaUrl(request.AlarmSoundUrl, "Âm thanh báo thức");
        ValidateMediaUrl(request.AmbientSoundUrl, "Âm thanh nền");
    }

    private static void ValidateMediaUrl(string? url, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        if (url.Length > 2048) throw new DomainException($"Đường dẫn {fieldName} vượt quá 2048 ký tự.");

        if (url.StartsWith('/') || url.StartsWith("data:image/")) return;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException($"Đường dẫn {fieldName} phải là liên kết HTTP/HTTPS hợp lệ hoặc đường dẫn nội bộ.");
        }
    }
}

