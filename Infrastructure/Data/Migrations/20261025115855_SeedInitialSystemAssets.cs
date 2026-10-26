using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace be_pomo_do.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialSystemAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "system_assets",
                columns: new[] { "id", "asset_type", "category", "created_at", "is_active", "is_default", "name", "thumbnail_url", "url" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111101"), "audio_alarm", "#Zen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "Chuông chùa Zen thanh tịnh", null, "https://actions.google.com/sounds/v1/musical_instruments/tibetan_singing_bowl.ogg" });

            migrationBuilder.InsertData(
                table: "system_assets",
                columns: new[] { "id", "asset_type", "category", "created_at", "is_active", "name", "thumbnail_url", "url" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111102"), "audio_alarm", "#Digital", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Chuông điện tử dịu êm", null, "https://actions.google.com/sounds/v1/cartoon/cartoon_cowbell.ogg" },
                    { new Guid("11111111-1111-1111-1111-111111111103"), "audio_alarm", "#Nature", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Tiếng chim rừng sớm", null, "https://actions.google.com/sounds/v1/ambiences/morning_birds.ogg" }
                });

            migrationBuilder.InsertData(
                table: "system_assets",
                columns: new[] { "id", "asset_type", "category", "created_at", "is_active", "is_default", "name", "thumbnail_url", "url" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222201"), "audio_ambient", "#Nature", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "Mưa rơi êm dịu (Rain)", null, "https://actions.google.com/sounds/v1/weather/rain_heavy.ogg" });

            migrationBuilder.InsertData(
                table: "system_assets",
                columns: new[] { "id", "asset_type", "category", "created_at", "is_active", "name", "thumbnail_url", "url" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222202"), "audio_ambient", "#Cozy", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Quán Cafe sách (Coffee Shop)", null, "https://actions.google.com/sounds/v1/ambiences/coffee_shop.ogg" },
                    { new Guid("22222222-2222-2222-2222-222222222203"), "audio_ambient", "#Nature", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Sóng biển đêm (Ocean Waves)", null, "https://actions.google.com/sounds/v1/weather/waves_crashing_against_cliff.ogg" }
                });

            migrationBuilder.InsertData(
                table: "system_assets",
                columns: new[] { "id", "asset_type", "category", "created_at", "is_active", "is_default", "name", "thumbnail_url", "url" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333301"), "wallpaper", "#Lofi", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "Phòng Cozy Lofi Anime", "https://images.unsplash.com/photo-1518495973542-4542c06a5843?q=80&w=400&auto=format&fit=crop", "https://images.unsplash.com/photo-1518495973542-4542c06a5843?q=80&w=1920&auto=format&fit=crop" });

            migrationBuilder.InsertData(
                table: "system_assets",
                columns: new[] { "id", "asset_type", "category", "created_at", "is_active", "name", "thumbnail_url", "url" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333302"), "wallpaper", "#Cyberpunk", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Cyberpunk Neon Rain", "https://images.unsplash.com/photo-1509198397868-475647b2a1e5?q=80&w=400&auto=format&fit=crop", "https://images.unsplash.com/photo-1509198397868-475647b2a1e5?q=80&w=1920&auto=format&fit=crop" },
                    { new Guid("33333333-3333-3333-3333-333333333303"), "wallpaper", "#Minimal", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Bàn làm việc Tối giản", "https://images.unsplash.com/photo-1497366216548-37526070297c?q=80&w=400&auto=format&fit=crop", "https://images.unsplash.com/photo-1497366216548-37526070297c?q=80&w=1920&auto=format&fit=crop" },
                    { new Guid("33333333-3333-3333-3333-333333333304"), "wallpaper", "#Nature", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Rừng sương mù tĩnh lặng", "https://images.unsplash.com/photo-1448375240586-882707db888b?q=80&w=400&auto=format&fit=crop", "https://images.unsplash.com/photo-1448375240586-882707db888b?q=80&w=1920&auto=format&fit=crop" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111101"));

            migrationBuilder.DeleteData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111102"));

            migrationBuilder.DeleteData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111103"));

            migrationBuilder.DeleteData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222201"));

            migrationBuilder.DeleteData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222202"));

            migrationBuilder.DeleteData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222203"));

            migrationBuilder.DeleteData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333301"));

            migrationBuilder.DeleteData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333302"));

            migrationBuilder.DeleteData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333303"));

            migrationBuilder.DeleteData(
                table: "system_assets",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333304"));
        }
    }
}
