using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Core.Entities;
using be_pomo_do.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace be_pomo_do.Infrastructure.Repositories;

/// <summary>Entity Framework Core implementation of <see cref="IUserRepository"/>.</summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly ICancellationTokenProvider _ctProvider;
    private CancellationToken Ct => _ctProvider.Token;

    /// <summary>Initializes a new instance of <see cref="UserRepository"/>.</summary>
    public UserRepository(AppDbContext context, ICancellationTokenProvider cancellationTokenProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _ctProvider = cancellationTokenProvider ?? throw new ArgumentNullException(nameof(cancellationTokenProvider));
    }

    /// <inheritdoc/>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Settings)
            .Include(u => u.Streak)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), Ct);
    }

    /// <inheritdoc/>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.Settings)
            .Include(u => u.Streak)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, Ct);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower(), Ct);
    }

    /// <inheritdoc/>
    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user, Ct);
    }

    /// <inheritdoc/>
    public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, Ct);
    }

    /// <inheritdoc/>
    public async Task<RefreshToken?> GetRefreshTokenWithUserAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token, Ct);
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync(Ct);
    }
}
