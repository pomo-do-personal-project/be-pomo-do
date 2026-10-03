using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Core.Entities;
using be_pomo_do.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace be_pomo_do.Infrastructure.Repositories;

/// <summary>Entity Framework Core implementation of <see cref="IPomodoroRepository"/>.</summary>
public class PomodoroRepository : IPomodoroRepository
{
    private readonly AppDbContext _context;

    /// <summary>Initializes a new instance of <see cref="PomodoroRepository"/> with explicit dependency injection.</summary>
    /// <param name="context">The database context.</param>
    public PomodoroRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<FocusSession?> GetSessionByIdAsync(Guid sessionId, Guid userId, CancellationToken ct = default)
    {
        return await _context.FocusSessions
            .Include(f => f.Task)
            .FirstOrDefaultAsync(f => f.Id == sessionId && f.UserId == userId, ct);
    }

    /// <inheritdoc/>
    public async Task<List<FocusSession>> GetSessionHistoryAsync(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken ct = default)
    {
        var query = _context.FocusSessions
            .AsNoTracking()
            .Include(f => f.Task)
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
            .OrderByDescending(f => f.StartedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task AddSessionAsync(FocusSession session, CancellationToken ct = default)
    {
        await _context.FocusSessions.AddAsync(session, ct);
    }

    /// <inheritdoc/>
    public async Task<DailyStreak?> GetDailyStreakByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.DailyStreaks
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);
    }

    /// <inheritdoc/>
    public async Task AddDailyStreakAsync(DailyStreak dailyStreak, CancellationToken ct = default)
    {
        await _context.DailyStreaks.AddAsync(dailyStreak, ct);
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
