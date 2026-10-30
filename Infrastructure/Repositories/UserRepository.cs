using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Core.Entities;
using be_pomo_do.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace be_pomo_do.Infrastructure.Repositories;

// Entity Framework Core implementation of IUserRepository.
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly ICancellationTokenProvider _ctProvider;
    private CancellationToken Ct => _ctProvider.Token;

    // Initializes a new instance of UserRepository.
    public UserRepository(AppDbContext context, ICancellationTokenProvider cancellationTokenProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _ctProvider = cancellationTokenProvider ?? throw new ArgumentNullException(nameof(cancellationTokenProvider));
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Settings)
            .Include(u => u.Streak)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), Ct);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.Settings)
            .Include(u => u.Streak)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, Ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower(), Ct);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user, Ct);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, Ct);
    }

    public void RemoveRefreshToken(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Remove(refreshToken);
    }

    public async Task<RefreshToken?> GetRefreshTokenWithUserAsync(string token)
    {
        var hashed = HashToken(token);
        return await _context.RefreshTokens
            .Include(r => r.User)
                .ThenInclude(u => u!.RefreshTokens)
            .FirstOrDefaultAsync(r => r.Token == hashed || r.Token == token, Ct);
    }

    public static string HashToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return string.Empty;
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync(Ct);
    }
}
