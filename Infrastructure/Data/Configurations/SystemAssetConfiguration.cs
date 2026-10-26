using be_pomo_do.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_pomo_do.Infrastructure.Data.Configurations;

// Cấu hình entity và dữ liệu khởi tạo cho SystemAsset
public class SystemAssetConfiguration : IEntityTypeConfiguration<SystemAsset>
{
    public void Configure(EntityTypeBuilder<SystemAsset> entity)
    {
        entity.ToTable("system_assets");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.AssetType).HasColumnName("asset_type").HasMaxLength(30).IsRequired();
        entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        entity.Property(e => e.Url).HasColumnName("url").HasColumnType("text").IsRequired();
        entity.Property(e => e.ThumbnailUrl).HasColumnName("thumbnail_url").HasColumnType("text");
        entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(50);
        entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.Property(e => e.IsDefault).HasColumnName("is_default").HasDefaultValue(false);
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

        entity.HasIndex(e => new { e.AssetType, e.IsActive });
        entity.HasIndex(e => e.Category);

        entity.HasData(
            new SystemAsset
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                AssetType = "audio_alarm",
                Name = "Chuông chùa Zen thanh tịnh",
                Url = "/sounds/zen_bell.wav",
                Category = "#Zen",
                IsActive = true,
                IsDefault = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemAsset
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111102"),
                AssetType = "audio_alarm",
                Name = "Chuông điện tử dịu êm",
                Url = "/sounds/digital_chime.wav",
                Category = "#Digital",
                IsActive = true,
                IsDefault = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemAsset
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111103"),
                AssetType = "audio_alarm",
                Name = "Tiếng chim rừng sớm",
                Url = "/sounds/morning_birds.wav",
                Category = "#Nature",
                IsActive = true,
                IsDefault = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemAsset
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222201"),
                AssetType = "audio_ambient",
                Name = "Mưa rơi êm dịu (Rain)",
                Url = "/sounds/rain_ambient.wav",
                Category = "#Nature",
                IsActive = true,
                IsDefault = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemAsset
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222202"),
                AssetType = "audio_ambient",
                Name = "Quán Cafe sách (Coffee Shop)",
                Url = "/sounds/coffee_shop.wav",
                Category = "#Cozy",
                IsActive = true,
                IsDefault = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemAsset
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222203"),
                AssetType = "audio_ambient",
                Name = "Sóng biển đêm (Ocean Waves)",
                Url = "/sounds/ocean_waves.wav",
                Category = "#Nature",
                IsActive = true,
                IsDefault = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemAsset
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333301"),
                AssetType = "wallpaper",
                Name = "Phòng Cozy Lofi Anime",
                Url = "https://images.unsplash.com/photo-1518495973542-4542c06a5843?q=80&w=1920&auto=format&fit=crop",
                ThumbnailUrl = "https://images.unsplash.com/photo-1518495973542-4542c06a5843?q=80&w=400&auto=format&fit=crop",
                Category = "#Lofi",
                IsActive = true,
                IsDefault = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemAsset
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333302"),
                AssetType = "wallpaper",
                Name = "Cyberpunk Neon Rain",
                Url = "https://images.unsplash.com/photo-1509198397868-475647b2a1e5?q=80&w=1920&auto=format&fit=crop",
                ThumbnailUrl = "https://images.unsplash.com/photo-1509198397868-475647b2a1e5?q=80&w=400&auto=format&fit=crop",
                Category = "#Cyberpunk",
                IsActive = true,
                IsDefault = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemAsset
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333303"),
                AssetType = "wallpaper",
                Name = "Bàn làm việc Tối giản",
                Url = "https://images.unsplash.com/photo-1497366216548-37526070297c?q=80&w=1920&auto=format&fit=crop",
                ThumbnailUrl = "https://images.unsplash.com/photo-1497366216548-37526070297c?q=80&w=400&auto=format&fit=crop",
                Category = "#Minimal",
                IsActive = true,
                IsDefault = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemAsset
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333304"),
                AssetType = "wallpaper",
                Name = "Rừng sương mù tĩnh lặng",
                Url = "https://images.unsplash.com/photo-1448375240586-882707db888b?q=80&w=1920&auto=format&fit=crop",
                ThumbnailUrl = "https://images.unsplash.com/photo-1448375240586-882707db888b?q=80&w=400&auto=format&fit=crop",
                Category = "#Nature",
                IsActive = true,
                IsDefault = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemAsset
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444401"),
                AssetType = "wallpaper",
                Name = "Tokyo - Chùa Phú Sĩ & Hoa anh đào",
                Url = "/wallpapers/city/tokyo.jpg",
                ThumbnailUrl = "/wallpapers/city/tokyo.jpg",
                Category = "#CitySeries",
                IsActive = true,
                IsDefault = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemAsset
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444402"),
                AssetType = "wallpaper",
                Name = "Hà Nội - Tháp Rùa sương sớm",
                Url = "/wallpapers/city/hanoi.jpg",
                ThumbnailUrl = "/wallpapers/city/hanoi.jpg",
                Category = "#CitySeries",
                IsActive = true,
                IsDefault = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemAsset
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444403"),
                AssetType = "wallpaper",
                Name = "Paris - Tháp Eiffel hoàng hôn tím",
                Url = "/wallpapers/city/paris.jpg",
                ThumbnailUrl = "/wallpapers/city/paris.jpg",
                Category = "#CitySeries",
                IsActive = true,
                IsDefault = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemAsset
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444404"),
                AssetType = "wallpaper",
                Name = "Cairo - Kim tự tháp & Đêm sa mạc",
                Url = "/wallpapers/city/cairo.jpg",
                ThumbnailUrl = "/wallpapers/city/cairo.jpg",
                Category = "#CitySeries",
                IsActive = true,
                IsDefault = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemAsset
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444405"),
                AssetType = "wallpaper",
                Name = "Sydney - Nhà hát Con sò & Vịnh đêm",
                Url = "/wallpapers/city/sydney.jpg",
                ThumbnailUrl = "/wallpapers/city/sydney.jpg",
                Category = "#CitySeries",
                IsActive = true,
                IsDefault = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
