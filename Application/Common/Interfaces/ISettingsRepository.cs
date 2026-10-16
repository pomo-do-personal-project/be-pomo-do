using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Common.Interfaces;

/// <summary>Repository contract for querying and persisting user settings.</summary>
public interface ISettingsRepository
{
    /// <summary>Retrieves user settings by user identifier.</summary>
    Task<UserSettings?> GetByUserIdAsync(Guid userId);

    /// <summary>Adds initial settings for a user.</summary>
    Task AddAsync(UserSettings settings);

    // Lưu các thay đổi vào CSDL
    Task SaveChangesAsync();

    // Lấy danh sách cấu hình của các user bật nhận Daily Digest với khung giờ <= currentTime và chưa gửi trong ngày hôm nay
    Task<List<UserSettings>> GetUsersForDailyDigestAsync(DateOnly today, string currentTime, CancellationToken ct = default);

    // Lấy danh sách cấu hình của các user bật nhận Weekly Report với khung giờ <= currentTime và chưa gửi trong tuần này
    Task<List<UserSettings>> GetUsersForWeeklyReportAsync(DateOnly currentWeekMonday, string currentTime, CancellationToken ct = default);
}

