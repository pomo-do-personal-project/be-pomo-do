namespace be_pomo_do.Application.DTOs.Assets;

// Yêu cầu cập nhật tài nguyên hệ thống từ Admin
public record UpdateAssetRequest(
    string Name,
    string Url,
    string? ThumbnailUrl,
    string? Category,
    bool IsActive,
    bool IsDefault
);
