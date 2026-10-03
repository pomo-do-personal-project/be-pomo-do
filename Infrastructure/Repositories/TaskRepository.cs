using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.DTOs.Tasks;
using be_pomo_do.Core.Entities;
using be_pomo_do.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace be_pomo_do.Infrastructure.Repositories;

/// <summary>Entity Framework Core implementation of <see cref="ITaskRepository"/>.</summary>
public class TaskRepository(AppDbContext context) : ITaskRepository
{
    private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc/>
    public async Task<List<TaskItem>> GetUserTasksAsync(Guid userId, TaskFilterParams filterParams, CancellationToken ct = default)
    {
        IQueryable<TaskItem> query = _context.Tasks.AsNoTracking();

        if (filterParams.InTrash)
        {
            query = query.IgnoreQueryFilters().Where(t => t.IsDeleted);
        }

        query = query.Where(t => t.UserId == userId);

        if (filterParams.Status.HasValue)
        {
            query = query.Where(t => t.Status == filterParams.Status.Value);
        }

        if (filterParams.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filterParams.Priority.Value);
        }

        if (filterParams.TagId.HasValue)
        {
            query = query.Where(t => t.TaskTags.Any(tt => tt.TagId == filterParams.TagId.Value));
        }

        if (filterParams.IsQuickAdd.HasValue)
        {
            query = query.Where(t => t.IsQuickAdd == filterParams.IsQuickAdd.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Search))
        {
            var searchPattern = $"%{filterParams.Search.Trim()}%";
            query = query.Where(t => EF.Functions.ILike(t.Title, searchPattern));
        }

        query = filterParams.SortBy?.ToLowerInvariant() switch
        {
            "duedate" => filterParams.IsDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            "priority" => filterParams.IsDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "title" => filterParams.IsDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            _ => filterParams.IsDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };

        return await query
            .Include(t => t.Subtasks.OrderBy(s => s.OrderIndex))
            .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
            .Include(t => t.FocusSessions)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<TaskItem?> GetTaskByIdAsync(Guid taskId, Guid userId, bool includeDeleted = false, CancellationToken ct = default)
    {
        IQueryable<TaskItem> query = _context.Tasks;

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query
            .Include(t => t.Subtasks.OrderBy(s => s.OrderIndex))
            .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
            .Include(t => t.FocusSessions)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId, ct);
    }

    /// <inheritdoc/>
    public async Task AddTaskAsync(TaskItem task, CancellationToken ct = default)
    {
        await _context.Tasks.AddAsync(task, ct);
    }

    /// <inheritdoc/>
    public async Task<Subtask?> GetSubtaskByIdAsync(Guid subtaskId, Guid taskId, CancellationToken ct = default)
    {
        return await _context.Subtasks
            .FirstOrDefaultAsync(s => s.Id == subtaskId && s.TaskId == taskId, ct);
    }

    /// <inheritdoc/>
    public async Task AddSubtaskAsync(Subtask subtask, CancellationToken ct = default)
    {
        await _context.Subtasks.AddAsync(subtask, ct);
    }

    /// <inheritdoc/>
    public void RemoveSubtask(Subtask subtask)
    {
        _context.Subtasks.Remove(subtask);
    }

    /// <inheritdoc/>
    public async Task<List<Tag>> GetUserTagsByIdsAsync(Guid userId, List<Guid> tagIds, CancellationToken ct = default)
    {
        if (tagIds.Count == 0) return [];

        return await _context.Tags
            .Where(t => t.UserId == userId && tagIds.Contains(t.Id))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
