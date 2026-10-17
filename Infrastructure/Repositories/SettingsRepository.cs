using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Core.Entities;
using be_pomo_do.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace be_pomo_do.Infrastructure.Repositories;

// Entity Framework Core implementation of ISettingsRepository.
public class SettingsRepository : ISettingsRepository
{
    private readonly AppDbContext _context;
    private readonly ICancellationTokenProvider _ctProvider;
    private CancellationToken Ct => _ctProvider.Token;

    // Initializes a new instance of SettingsRepository.
    public SettingsRepository(AppDbContext context, ICancellationTokenProvider cancellationTokenProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _ctProvider = cancellationTokenProvider ?? throw new ArgumentNullException(nameof(cancellationTokenProvider));
    }

    public async Task<UserSettings?> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId, Ct);
    }

    public async Task AddAsync(UserSettings settings)
    {
        await _context.UserSettings.AddAsync(settings, Ct);
    }

    // Lưu các thay đổi vào CSDL
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync(Ct);
    }

    // Lấy danh sách cấu hình của các user bật nhận Daily Digest với khung giờ <= currentTime và chưa gửi trong ngày hôm nay
    public async Task<List<UserSettings>> GetUsersForDailyDigestAsync(DateOnly today, string currentTime, CancellationToken ct = default)
    {
        return await _context.UserSettings
            .Where(s => s.EmailDailyDigest 
                && string.Compare(s.DailyDigestTime, currentTime) <= 0 
                && (s.LastDailyDigestSentDate == null || s.LastDailyDigestSentDate < today))
            .ToListAsync(ct == default ? Ct : ct);
    }

    // Lấy danh sách cấu hình của các user bật nhận Weekly Report với khung giờ <= currentTime và chưa gửi trong tuần này
    public async Task<List<UserSettings>> GetUsersForWeeklyReportAsync(DateOnly currentWeekMonday, string currentTime, CancellationToken ct = default)
    {
        return await _context.UserSettings
            .Where(s => s.EmailWeeklyReport 
                && string.Compare(s.WeeklyReportTime, currentTime) <= 0 
                && (s.LastWeeklyReportSentDate == null || s.LastWeeklyReportSentDate < currentWeekMonday))
            .ToListAsync(ct == default ? Ct : ct);
    }
}

