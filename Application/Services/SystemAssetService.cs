using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.DTOs.Assets;
using be_pomo_do.Core.Entities;
using be_pomo_do.Core.Exceptions;

namespace be_pomo_do.Application.Services;

// Cài đặt nghiệp vụ quản lý tài nguyên hệ thống (kho hình nền và âm thanh)
public class SystemAssetService : ISystemAssetService
{
    private readonly ISystemAssetRepository _assetRepository;

    public SystemAssetService(ISystemAssetRepository assetRepository)
    {
        _assetRepository = assetRepository ?? throw new ArgumentNullException(nameof(assetRepository));
    }

    public async Task<IReadOnlyList<SystemAssetDto>> GetActiveAssetsAsync(string? assetType, string? category, CancellationToken cancellationToken = default)
    {
        var entities = await _assetRepository.GetActiveAssetsAsync(assetType, category, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<(IReadOnlyList<SystemAssetDto> Items, int TotalCount)> GetAdminAssetsAsync(
        string? assetType,
        string? category,
        bool? isActive,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var (items, totalCount) = await _assetRepository.GetAdminAssetsAsync(assetType, category, isActive, search, page, pageSize, cancellationToken);
        return (items.Select(MapToDto).ToList(), totalCount);
    }

    public async Task<SystemAssetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _assetRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new DomainException("Tài nguyên không tồn tại.");
        }
        return MapToDto(entity);
    }

    public async Task<SystemAssetDto> CreateAssetAsync(CreateAssetRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new DomainException("Tên tài nguyên không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(request.Url))
        {
            throw new DomainException("Đường dẫn tài nguyên không được để trống.");
        }

        var validTypes = new[] { "audio_alarm", "audio_ambient", "wallpaper" };
        var cleanType = request.AssetType.Trim().ToLower();
        if (!validTypes.Contains(cleanType))
        {
            throw new DomainException("Loại tài nguyên không hợp lệ. Chỉ chấp nhận audio_alarm, audio_ambient hoặc wallpaper.");
        }

        var asset = new SystemAsset
        {
            AssetType = cleanType,
            Name = request.Name.Trim(),
            Url = request.Url.Trim(),
            ThumbnailUrl = string.IsNullOrWhiteSpace(request.ThumbnailUrl) ? null : request.ThumbnailUrl.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim(),
            IsActive = request.IsActive,
            IsDefault = request.IsDefault
        };

        if (asset.IsDefault)
        {
            await _assetRepository.ClearDefaultAsync(asset.AssetType, asset.Id, cancellationToken);
        }

        await _assetRepository.AddAsync(asset, cancellationToken);
        await _assetRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(asset);
    }

    public async Task<SystemAssetDto> UpdateAssetAsync(Guid id, UpdateAssetRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await _assetRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new DomainException("Tài nguyên không tồn tại.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new DomainException("Tên tài nguyên không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(request.Url))
        {
            throw new DomainException("Đường dẫn tài nguyên không được để trống.");
        }

        entity.Name = request.Name.Trim();
        entity.Url = request.Url.Trim();
        entity.ThumbnailUrl = string.IsNullOrWhiteSpace(request.ThumbnailUrl) ? null : request.ThumbnailUrl.Trim();
        entity.Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();
        entity.IsActive = request.IsActive;

        if (request.IsDefault && !entity.IsDefault)
        {
            entity.IsDefault = true;
            await _assetRepository.ClearDefaultAsync(entity.AssetType, entity.Id, cancellationToken);
        }
        else if (!request.IsDefault)
        {
            entity.IsDefault = false;
        }

        await _assetRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAssetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _assetRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new DomainException("Tài nguyên không tồn tại.");
        }

        await _assetRepository.DeleteAsync(entity, cancellationToken);
        await _assetRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<SystemAssetDto> ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _assetRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new DomainException("Tài nguyên không tồn tại.");
        }

        if (entity.IsDefault && entity.IsActive)
        {
            throw new DomainException("Không thể tắt kích hoạt tài nguyên đang được đặt làm mặc định.");
        }

        entity.IsActive = !entity.IsActive;
        await _assetRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<SystemAssetDto> SetDefaultAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _assetRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new DomainException("Tài nguyên không tồn tại.");
        }

        entity.IsDefault = true;
        entity.IsActive = true; // Mặc định thì phải kích hoạt

        await _assetRepository.ClearDefaultAsync(entity.AssetType, entity.Id, cancellationToken);
        await _assetRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(entity);
    }

    private static SystemAssetDto MapToDto(SystemAsset entity)
    {
        return new SystemAssetDto(
            entity.Id,
            entity.AssetType,
            entity.Name,
            entity.Url,
            entity.ThumbnailUrl,
            entity.Category,
            entity.IsActive,
            entity.IsDefault,
            entity.CreatedAt
        );
    }
}
