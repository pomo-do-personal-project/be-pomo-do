using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.DTOs.Tasks;
using be_pomo_do.Core.Entities;
using be_pomo_do.Core.Exceptions;

namespace be_pomo_do.Application.Services;

// Application service implementation for Tag management.
public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    // Initializes a new instance of TagService.
    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
    }

    public async Task<List<TagDto>> GetTagsAsync(Guid userId)
    {
        var tags = await _tagRepository.GetUserTagsAsync(userId);
        return tags.Select(MapToDto).ToList();
    }

    public async Task<TagDto> GetTagByIdAsync(Guid tagId, Guid userId)
    {
        var tag = await _tagRepository.GetTagByIdAsync(tagId, userId);
        if (tag == null)
        {
            throw new DomainException("Thẻ tag không tồn tại hoặc bạn không có quyền truy cập.");
        }

        return MapToDto(tag);
    }

    public async Task<TagDto> CreateTagAsync(Guid userId, CreateTagRequest request)
    {
        var cleanName = request.Name?.Trim() ?? string.Empty;
        if (cleanName.Length > 50) throw new DomainException("Tên thẻ tag không được vượt quá 50 ký tự.");
        if (!string.IsNullOrWhiteSpace(request.ColorHex) && !System.Text.RegularExpressions.Regex.IsMatch(request.ColorHex.Trim(), "^#[0-9A-Fa-f]{6}$"))
            throw new DomainException("Mã màu thẻ tag không hợp lệ (định dạng chuẩn #RRGGBB).");
        if (string.IsNullOrWhiteSpace(cleanName))
        {
            throw new DomainException("Tên thẻ tag không được để trống.");
        }

        // Kiểm tra trùng tên tag của cùng 1 user
        var exists = await _tagRepository.TagNameExistsAsync(userId, cleanName);
        if (exists)
        {
            throw new DomainException($"Thẻ tag '{cleanName}' đã tồn tại trong danh sách của bạn.");
        }

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = cleanName,
            ColorHex = string.IsNullOrWhiteSpace(request.ColorHex) ? "#3B82F6" : request.ColorHex.Trim()
        };

        await _tagRepository.AddTagAsync(tag);
        await _tagRepository.SaveChangesAsync();

        return MapToDto(tag);
    }

    public async Task<TagDto> UpdateTagAsync(Guid tagId, Guid userId, UpdateTagRequest request)
    {
        var tag = await _tagRepository.GetTagByIdAsync(tagId, userId);
        if (tag == null)
        {
            throw new DomainException("Thẻ tag không tồn tại hoặc bạn không có quyền chỉnh sửa.");
        }

        var cleanName = request.Name?.Trim() ?? string.Empty;
        if (cleanName.Length > 50) throw new DomainException("Tên thẻ tag không được vượt quá 50 ký tự.");
        if (!string.IsNullOrWhiteSpace(request.ColorHex) && !System.Text.RegularExpressions.Regex.IsMatch(request.ColorHex.Trim(), "^#[0-9A-Fa-f]{6}$"))
            throw new DomainException("Mã màu thẻ tag không hợp lệ (định dạng chuẩn #RRGGBB).");
        if (string.IsNullOrWhiteSpace(cleanName))
        {
            throw new DomainException("Tên thẻ tag không được để trống.");
        }

        // Kiểm tra trùng tên với thẻ khác của cùng 1 user
        var exists = await _tagRepository.TagNameExistsAsync(userId, cleanName, excludeId: tagId);
        if (exists)
        {
            throw new DomainException($"Thẻ tag '{cleanName}' đã tồn tại trong danh sách của bạn.");
        }

        tag.Name = cleanName;
        tag.ColorHex = string.IsNullOrWhiteSpace(request.ColorHex) ? "#3B82F6" : request.ColorHex.Trim();

        await _tagRepository.SaveChangesAsync();
        return MapToDto(tag);
    }

    public async Task DeleteTagAsync(Guid tagId, Guid userId)
    {
        var tag = await _tagRepository.GetTagByIdAsync(tagId, userId);
        if (tag == null)
        {
            throw new DomainException("Thẻ tag không tồn tại hoặc bạn không có quyền xóa.");
        }

        _tagRepository.RemoveTag(tag);
        await _tagRepository.SaveChangesAsync();
    }

    private static TagDto MapToDto(Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            ColorHex = tag.ColorHex
        };
    }
}
