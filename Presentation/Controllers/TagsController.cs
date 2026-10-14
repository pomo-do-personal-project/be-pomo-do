using System.Security.Claims;
using be_pomo_do.Application.DTOs.Tasks;
using be_pomo_do.Application.Services;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be_pomo_do.Presentation.Controllers;

/// <summary>Endpoints for managing user custom tags and category colors.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    /// <summary>Initializes a new instance of <see cref="TagsController"/>.</summary>
    public TagsController(ITagService tagService)
    {
        _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
    }

    /// <summary>Retrieves all tags belonging to the current user.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TagDto>>>> GetTags()
    {
        var userId = GetCurrentUserId();
        var tags = await _tagService.GetTagsAsync(userId);
        return Ok(ApiResponse<List<TagDto>>.Ok(tags, "Lấy danh sách thẻ tag thành công."));
    }

    /// <summary>Retrieves details of a specific tag by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TagDto>>> GetTagById([FromRoute] Guid id)
    {
        var userId = GetCurrentUserId();
        var tag = await _tagService.GetTagByIdAsync(id, userId);
        return Ok(ApiResponse<TagDto>.Ok(tag, "Lấy chi tiết thẻ tag thành công."));
    }

    /// <summary>Creates a new custom tag.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TagDto>>> CreateTag([FromBody] CreateTagRequest request)
    {
        var userId = GetCurrentUserId();
        var tag = await _tagService.CreateTagAsync(userId, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TagDto>.Ok(tag, "Tạo thẻ tag thành công."));
    }

    /// <summary>Updates name or color of an existing tag.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TagDto>>> UpdateTag(
        [FromRoute] Guid id,
        [FromBody] UpdateTagRequest request)
    {
        var userId = GetCurrentUserId();
        var tag = await _tagService.UpdateTagAsync(id, userId, request);
        return Ok(ApiResponse<TagDto>.Ok(tag, "Cập nhật thẻ tag thành công."));
    }

    /// <summary>Permanently deletes a tag and detaches it from all tasks.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteTag([FromRoute] Guid id)
    {
        var userId = GetCurrentUserId();
        await _tagService.DeleteTagAsync(id, userId);
        return Ok(ApiResponse<string>.Ok("Đã xóa thẻ tag thành công."));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");
        }

        return userId;
    }
}
