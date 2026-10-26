using be_pomo_do.Application.DTOs.Assets;

namespace be_pomo_do.Application.Common.Interfaces;

// Giao diện nghiệp vụ quản lý tài nguyên hệ thống
public interface ISystemAssetService
{
    // Lấy danh sách tài nguyên đang kích hoạt cho User
    Task<IReadOnlyList<SystemAssetDto>> GetActiveAssetsAsync(string? assetType, string? category, CancellationToken cancellationToken = default);

    // Lấy danh sách tài nguyên quản trị cho Admin
    Task<(IReadOnlyList<SystemAssetDto> Items, int TotalCount)> GetAdminAssetsAsync(
        string? assetType,
        string? category,
        bool? isActive,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    // Lấy chi tiết tài nguyên theo Id
    Task<SystemAssetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Admin tạo tài nguyên mới
    Task<SystemAssetDto> CreateAssetAsync(CreateAssetRequest request, CancellationToken cancellationToken = default);

    // Admin cập nhật tài nguyên
    Task<SystemAssetDto> UpdateAssetAsync(Guid id, UpdateAssetRequest request, CancellationToken cancellationToken = default);

    // Admin xóa tài nguyên
    Task<bool> DeleteAssetAsync(Guid id, CancellationToken cancellationToken = default);

    // Admin bật/tắt nhanh trạng thái khả dụng
    Task<SystemAssetDto> ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default);

    // Admin đặt làm tài nguyên mặc định
    Task<SystemAssetDto> SetDefaultAsync(Guid id, CancellationToken cancellationToken = default);
}
