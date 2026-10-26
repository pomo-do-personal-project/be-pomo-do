using be_pomo_do.Core.Entities;

namespace be_pomo_do.Application.Common.Interfaces;

// Giao diện Repository quản lý truy vấn dữ liệu tài nguyên hệ thống
public interface ISystemAssetRepository
{
    // Lấy danh sách tài nguyên đang kích hoạt dành cho User
    Task<IReadOnlyList<SystemAsset>> GetActiveAssetsAsync(string? assetType, string? category, CancellationToken cancellationToken = default);

    // Lấy toàn bộ danh sách tài nguyên kèm phân trang và bộ lọc dành cho Admin
    Task<(IReadOnlyList<SystemAsset> Items, int TotalCount)> GetAdminAssetsAsync(
        string? assetType,
        string? category,
        bool? isActive,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    // Tìm kiếm tài nguyên theo Id
    Task<SystemAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Thêm tài nguyên mới
    Task AddAsync(SystemAsset asset, CancellationToken cancellationToken = default);

    // Xóa tài nguyên
    Task DeleteAsync(SystemAsset asset, CancellationToken cancellationToken = default);

    // Bỏ đánh dấu IsDefault của các tài nguyên cùng loại
    Task ClearDefaultAsync(string assetType, Guid excludeId, CancellationToken cancellationToken = default);

    // Lưu các thay đổi vào CSDL
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
