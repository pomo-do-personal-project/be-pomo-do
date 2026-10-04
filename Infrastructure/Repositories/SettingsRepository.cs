using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Core.Entities;
using be_pomo_do.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace be_pomo_do.Infrastructure.Repositories;

/// <summary>Entity Framework Core implementation of <see cref="ISettingsRepository"/>.</summary>
public class SettingsRepository : ISettingsRepository
{
    private readonly AppDbContext _context;
    private readonly ICancellationTokenProvider _ctProvider;
    private CancellationToken Ct => _ctProvider.Token;

    /// <summary>Initializes a new instance of <see cref="SettingsRepository"/>.</summary>
    public SettingsRepository(AppDbContext context, ICancellationTokenProvider cancellationTokenProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _ctProvider = cancellationTokenProvider ?? throw new ArgumentNullException(nameof(cancellationTokenProvider));
    }

    /// <inheritdoc/>
    public async Task<UserSettings?> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId, Ct);
    }

    /// <inheritdoc/>
    public async Task AddAsync(UserSettings settings)
    {
        await _context.UserSettings.AddAsync(settings, Ct);
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync(Ct);
    }
}
