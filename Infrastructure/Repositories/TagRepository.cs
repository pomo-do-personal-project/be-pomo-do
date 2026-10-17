using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Core.Entities;
using be_pomo_do.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace be_pomo_do.Infrastructure.Repositories;

// Entity Framework Core implementation of ITagRepository.
public class TagRepository : ITagRepository
{
    private readonly AppDbContext _context;
    private readonly ICancellationTokenProvider _ctProvider;
    private CancellationToken Ct => _ctProvider.Token;

    // Initializes a new instance of TagRepository.
    public TagRepository(AppDbContext context, ICancellationTokenProvider cancellationTokenProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _ctProvider = cancellationTokenProvider ?? throw new ArgumentNullException(nameof(cancellationTokenProvider));
    }

    public async Task<List<Tag>> GetUserTagsAsync(Guid userId)
    {
        return await _context.Tags
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .ToListAsync(Ct);
    }

    public async Task<Tag?> GetTagByIdAsync(Guid tagId, Guid userId)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Id == tagId && t.UserId == userId, Ct);
    }

    public async Task<bool> TagNameExistsAsync(Guid userId, string name, Guid? excludeId = null)
    {
        var cleanName = name.Trim().ToLower();
        var query = _context.Tags.Where(t => t.UserId == userId && t.Name.ToLower() == cleanName);

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return await query.AnyAsync(Ct);
    }

    public async Task AddTagAsync(Tag tag)
    {
        await _context.Tags.AddAsync(tag, Ct);
    }

    public void RemoveTag(Tag tag)
    {
        _context.Tags.Remove(tag);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync(Ct);
    }
}
