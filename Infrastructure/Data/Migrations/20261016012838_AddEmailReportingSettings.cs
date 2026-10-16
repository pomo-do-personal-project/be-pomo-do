using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace be_pomo_do.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailReportingSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "email_daily_digest",
                table: "user_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "email_weekly_report",
                table: "user_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "last_daily_digest_sent_date",
                table: "user_settings",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "last_weekly_report_sent_date",
                table: "user_settings",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "report_email",
                table: "user_settings",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email_daily_digest",
                table: "user_settings");

            migrationBuilder.DropColumn(
                name: "email_weekly_report",
                table: "user_settings");

            migrationBuilder.DropColumn(
                name: "last_daily_digest_sent_date",
                table: "user_settings");

            migrationBuilder.DropColumn(
                name: "last_weekly_report_sent_date",
                table: "user_settings");

            migrationBuilder.DropColumn(
                name: "report_email",
                table: "user_settings");
        }
    }
}
