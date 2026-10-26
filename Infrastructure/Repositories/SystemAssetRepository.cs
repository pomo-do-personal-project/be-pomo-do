using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Core.Entities;
using be_pomo_do.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace be_pomo_do.Infrastructure.Repositories;

// Cài đặt truy vấn CSDL cho SystemAsset bằng Entity Framework Core
public class SystemAssetRepository : ISystemAssetRepository
{
    private readonly AppDbContext _context;
    private readonly ICancellationTokenProvider _ctProvider;
    private CancellationToken Ct => _ctProvider.Token;

    public SystemAssetRepository(AppDbContext context, ICancellationTokenProvider cancellationTokenProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _ctProvider = cancellationTokenProvider ?? throw new ArgumentNullException(nameof(cancellationTokenProvider));
    }

    public async Task<IReadOnlyList<SystemAsset>> GetActiveAssetsAsync(string? assetType, string? category, CancellationToken cancellationToken = default)
    {
        var ct = cancellationToken != default ? cancellationToken : Ct;
        var query = _context.SystemAssets
            .AsNoTracking()
            .Where(a => a.IsActive);

        if (!string.IsNullOrWhiteSpace(assetType))
        {
            query = query.Where(a => a.AssetType == assetType.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(a => a.Category != null && a.Category.ToLower() == category.Trim().ToLower());
        }

        return await query
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Name)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<SystemAsset> Items, int TotalCount)> GetAdminAssetsAsync(
        string? assetType,
        string? category,
        bool? isActive,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var ct = cancellationToken != default ? cancellationToken : Ct;
        var query = _context.SystemAssets.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(assetType))
        {
            query = query.Where(a => a.AssetType == assetType.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(a => a.Category != null && a.Category.ToLower() == category.Trim().ToLower());
        }

        if (isActive.HasValue)
        {
            query = query.Where(a => a.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var cleanSearch = search.Trim().ToLower();
            query = query.Where(a => a.Name.ToLower().Contains(cleanSearch) || (a.Category != null && a.Category.ToLower().Contains(cleanSearch)));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<SystemAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ct = cancellationToken != default ? cancellationToken : Ct;
        return await _context.SystemAssets.FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task AddAsync(SystemAsset asset, CancellationToken cancellationToken = default)
    {
        var ct = cancellationToken != default ? cancellationToken : Ct;
        await _context.SystemAssets.AddAsync(asset, ct);
    }

    public Task DeleteAsync(SystemAsset asset, CancellationToken cancellationToken = default)
    {
        _context.SystemAssets.Remove(asset);
        return Task.CompletedTask;
    }

    public async Task ClearDefaultAsync(string assetType, Guid excludeId, CancellationToken cancellationToken = default)
    {
        var ct = cancellationToken != default ? cancellationToken : Ct;
        var defaults = await _context.SystemAssets
            .Where(a => a.AssetType == assetType && a.IsDefault && a.Id != excludeId)
            .ToListAsync(ct);

        foreach (var item in defaults)
        {
            item.IsDefault = false;
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var ct = cancellationToken != default ? cancellationToken : Ct;
        await _context.SaveChangesAsync(ct);
    }
}
