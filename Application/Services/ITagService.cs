using be_pomo_do.Application.DTOs.Tasks;

namespace be_pomo_do.Application.Services;

// Service contract for Tag management operations.
public interface ITagService
{
    // Retrieves all tags belonging to the specified user.
    Task<List<TagDto>> GetTagsAsync(Guid userId);

    // Retrieves details of a specific tag by ID.
    Task<TagDto> GetTagByIdAsync(Guid tagId, Guid userId);

    // Creates a new tag for the user.
    Task<TagDto> CreateTagAsync(Guid userId, CreateTagRequest request);

    // Updates an existing tag's name or color.
    Task<TagDto> UpdateTagAsync(Guid tagId, Guid userId, UpdateTagRequest request);

    // Deletes a tag and removes all task associations.
    Task DeleteTagAsync(Guid tagId, Guid userId);
}
