namespace be_pomo_do.Application.DTOs.Assets;

// DTO biểu diễn dữ liệu tài nguyên trả về cho Client
public record SystemAssetDto(
    Guid Id,
    string AssetType,
    string Name,
    string Url,
    string? ThumbnailUrl,
    string? Category,
    bool IsActive,
    bool IsDefault,
    DateTime CreatedAt
);
