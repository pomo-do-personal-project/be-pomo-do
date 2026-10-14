using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Common.Interfaces;

/// <summary>Repository interface for Tag database operations.</summary>
public interface ITagRepository
{
    /// <summary>Retrieves all tags belonging to the specified user.</summary>
    Task<List<Tag>> GetUserTagsAsync(Guid userId);

    /// <summary>Retrieves a single tag by ID and User ID.</summary>
    Task<Tag?> GetTagByIdAsync(Guid tagId, Guid userId);

    /// <summary>Checks whether a tag with the specified name already exists for the user.</summary>
    Task<bool> TagNameExistsAsync(Guid userId, string name, Guid? excludeId = null);

    /// <summary>Adds a new tag to persistence.</summary>
    Task AddTagAsync(Tag tag);

    /// <summary>Removes a tag permanently.</summary>
    void RemoveTag(Tag tag);

    /// <summary>Persists all tracked changes to the database.</summary>
    Task SaveChangesAsync();
}
