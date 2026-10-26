using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace be_pomo_do.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemAssetsAndUserSettingsAudio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "alarm_sound_url",
                table: "user_settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ambient_sound_url",
                table: "user_settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "wallpaper_url",
                table: "user_settings",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "system_assets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    thumbnail_url = table.Column<string>(type: "text", nullable: true),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_assets", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_system_assets_asset_type_is_active",
                table: "system_assets",
                columns: new[] { "asset_type", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_system_assets_category",
                table: "system_assets",
                column: "category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "system_assets");

            migrationBuilder.DropColumn(
                name: "alarm_sound_url",
                table: "user_settings");

            migrationBuilder.DropColumn(
                name: "ambient_sound_url",
                table: "user_settings");

            migrationBuilder.DropColumn(
                name: "wallpaper_url",
                table: "user_settings");
        }
    }
}
