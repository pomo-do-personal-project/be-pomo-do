using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Common.Interfaces;

// Repository interface for Tag database operations.
public interface ITagRepository
{
    // Retrieves all tags belonging to the specified user.
    Task<List<Tag>> GetUserTagsAsync(Guid userId);

    // Retrieves a single tag by ID and User ID.
    Task<Tag?> GetTagByIdAsync(Guid tagId, Guid userId);

    // Checks whether a tag with the specified name already exists for the user.
    Task<bool> TagNameExistsAsync(Guid userId, string name, Guid? excludeId = null);

    // Adds a new tag to persistence.
    Task AddTagAsync(Tag tag);

    // Removes a tag permanently.
    void RemoveTag(Tag tag);

    // Persists all tracked changes to the database.
    Task SaveChangesAsync();
}
