using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Core.Entities;
using be_pomo_do.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace be_pomo_do.Infrastructure.Repositories;

// Entity Framework Core implementation of IPomodoroRepository.
public class PomodoroRepository : IPomodoroRepository
{
    private readonly AppDbContext _context;
    private readonly ICancellationTokenProvider _ctProvider;
    private CancellationToken Ct => _ctProvider.Token;

    // Initializes a new instance of PomodoroRepository.
    public PomodoroRepository(AppDbContext context, ICancellationTokenProvider cancellationTokenProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _ctProvider = cancellationTokenProvider ?? throw new ArgumentNullException(nameof(cancellationTokenProvider));
    }

    public async Task<FocusSession?> GetSessionByIdAsync(Guid sessionId, Guid userId)
    {
        return await _context.FocusSessions
            .Include(f => f.Task)
            .FirstOrDefaultAsync(f => f.Id == sessionId && f.UserId == userId, Ct);
    }

    public async Task<List<FocusSession>> GetSessionHistoryAsync(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        IQueryable<FocusSession> query = _context.FocusSessions
            .AsNoTracking()
            .Where(f => f.UserId == userId);

        if (fromUtc.HasValue)
        {
            query = query.Where(f => f.StartedAt >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(f => f.StartedAt <= toUtc.Value);
        }

        return await query
            .Include(f => f.Task)
            .OrderByDescending(f => f.StartedAt)
            .ToListAsync(Ct);
    }

    public async Task AddSessionAsync(FocusSession session)
    {
        await _context.FocusSessions.AddAsync(session, Ct);
    }

    public async Task<DailyStreak?> GetDailyStreakByUserIdAsync(Guid userId)
    {
        return await _context.DailyStreaks
            .FirstOrDefaultAsync(d => d.UserId == userId, Ct);
    }

    public async Task AddDailyStreakAsync(DailyStreak dailyStreak)
    {
        await _context.DailyStreaks.AddAsync(dailyStreak, Ct);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync(Ct);
    }
}
