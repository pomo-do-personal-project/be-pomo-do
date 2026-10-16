using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace be_pomo_do.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomReportTimesToUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "daily_digest_time",
                table: "user_settings",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "07:00");

            migrationBuilder.AddColumn<string>(
                name: "weekly_report_time",
                table: "user_settings",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "08:00");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "daily_digest_time",
                table: "user_settings");

            migrationBuilder.DropColumn(
                name: "weekly_report_time",
                table: "user_settings");
        }
    }
}
