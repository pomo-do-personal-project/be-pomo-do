namespace be_pomo_do.Application.DTOs.Assets;

// Yêu cầu thêm mới tài nguyên hệ thống từ Admin
public record CreateAssetRequest(
    string AssetType,
    string Name,
    string Url,
    string? ThumbnailUrl,
    string? Category,
    bool IsActive = true,
    bool IsDefault = false
);
