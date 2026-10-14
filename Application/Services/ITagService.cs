using be_pomo_do.Application.DTOs.Tasks;

namespace be_pomo_do.Application.Services;

/// <summary>Service contract for Tag management operations.</summary>
public interface ITagService
{
    /// <summary>Retrieves all tags belonging to the specified user.</summary>
    Task<List<TagDto>> GetTagsAsync(Guid userId);

    /// <summary>Retrieves details of a specific tag by ID.</summary>
    Task<TagDto> GetTagByIdAsync(Guid tagId, Guid userId);

    /// <summary>Creates a new tag for the user.</summary>
    Task<TagDto> CreateTagAsync(Guid userId, CreateTagRequest request);

    /// <summary>Updates an existing tag's name or color.</summary>
    Task<TagDto> UpdateTagAsync(Guid tagId, Guid userId, UpdateTagRequest request);

    /// <summary>Deletes a tag and removes all task associations.</summary>
    Task DeleteTagAsync(Guid tagId, Guid userId);
}
