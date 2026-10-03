using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Core.Entities;
using be_pomo_do.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace be_pomo_do.Infrastructure.Repositories;

/// <summary>Entity Framework Core implementation of <see cref="ISettingsRepository"/>.</summary>
public class SettingsRepository : ISettingsRepository
{
    private readonly AppDbContext _context;

    /// <summary>Initializes a new instance of <see cref="SettingsRepository"/> with explicit dependency injection.</summary>
    /// <param name="context">The application database context.</param>
    public SettingsRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<UserSettings?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);
    }

    /// <inheritdoc/>
    public async Task AddAsync(UserSettings settings, CancellationToken ct = default)
    {
        await _context.UserSettings.AddAsync(settings, ct);
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
