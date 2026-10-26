using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace be_pomo_do.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncSystemAssetsAndUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111101"),
                column: "url",
                value: "/sounds/zen_bell.wav");

            migrationBuilder.UpdateData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111102"),
                column: "url",
                value: "/sounds/digital_chime.wav");

            migrationBuilder.UpdateData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111103"),
                column: "url",
                value: "/sounds/morning_birds.wav");

            migrationBuilder.UpdateData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222201"),
                column: "url",
                value: "/sounds/rain_ambient.wav");

            migrationBuilder.UpdateData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222202"),
                column: "url",
                value: "/sounds/coffee_shop.wav");

            migrationBuilder.UpdateData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222203"),
                column: "url",
                value: "/sounds/ocean_waves.wav");

            migrationBuilder.Sql(@"
                INSERT INTO system_assets (id, asset_type, category, created_at, is_active, name, thumbnail_url, url)
                VALUES 
                    ('44444444-4444-4444-4444-444444444401', 'wallpaper', '#CitySeries', TIMESTAMPTZ '2026-01-01T00:00:00Z', TRUE, 'Tokyo - Chùa Phú Sĩ & Hoa anh đào', '/wallpapers/city/tokyo.jpg', '/wallpapers/city/tokyo.jpg'),
                    ('44444444-4444-4444-4444-444444444402', 'wallpaper', '#CitySeries', TIMESTAMPTZ '2026-01-01T00:00:00Z', TRUE, 'Hà Nội - Tháp Rùa sương sớm', '/wallpapers/city/hanoi.jpg', '/wallpapers/city/hanoi.jpg'),
                    ('44444444-4444-4444-4444-444444444403', 'wallpaper', '#CitySeries', TIMESTAMPTZ '2026-01-01T00:00:00Z', TRUE, 'Paris - Tháp Eiffel hoàng hôn tím', '/wallpapers/city/paris.jpg', '/wallpapers/city/paris.jpg'),
                    ('44444444-4444-4444-4444-444444444404', 'wallpaper', '#CitySeries', TIMESTAMPTZ '2026-01-01T00:00:00Z', TRUE, 'Cairo - Kim tự tháp & Đêm sa mạc', '/wallpapers/city/cairo.jpg', '/wallpapers/city/cairo.jpg'),
                    ('44444444-4444-4444-4444-444444444405', 'wallpaper', '#CitySeries', TIMESTAMPTZ '2026-01-01T00:00:00Z', TRUE, 'Sydney - Nhà hát Con sò & Vịnh đêm', '/wallpapers/city/sydney.jpg', '/wallpapers/city/sydney.jpg')
                ON CONFLICT (id) DO UPDATE SET 
                    url = EXCLUDED.url,
                    thumbnail_url = EXCLUDED.thumbnail_url,
                    name = EXCLUDED.name,
                    category = EXCLUDED.category;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444401"));

            migrationBuilder.DeleteData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444402"));

            migrationBuilder.DeleteData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444403"));

            migrationBuilder.DeleteData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444404"));

            migrationBuilder.DeleteData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444405"));

            migrationBuilder.UpdateData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111101"),
                column: "url",
                value: "https://actions.google.com/sounds/v1/musical_instruments/tibetan_singing_bowl.ogg");

            migrationBuilder.UpdateData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111102"),
                column: "url",
                value: "https://actions.google.com/sounds/v1/cartoon/cartoon_cowbell.ogg");

            migrationBuilder.UpdateData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111103"),
                column: "url",
                value: "https://actions.google.com/sounds/v1/ambiences/morning_birds.ogg");

            migrationBuilder.UpdateData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222201"),
                column: "url",
                value: "https://actions.google.com/sounds/v1/weather/rain_heavy.ogg");

            migrationBuilder.UpdateData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222202"),
                column: "url",
                value: "https://actions.google.com/sounds/v1/ambiences/coffee_shop.ogg");

            migrationBuilder.UpdateData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222203"),
                column: "url",
                value: "https://actions.google.com/sounds/v1/weather/waves_crashing_against_cliff.ogg");
        }
    }
}
