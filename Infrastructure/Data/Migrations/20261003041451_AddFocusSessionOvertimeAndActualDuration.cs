using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace be_pomo_do.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFocusSessionOvertimeAndActualDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "actual_seconds",
                table: "focus_sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at",
                table: "focus_sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "duration_minutes",
                table: "focus_sessions",
                type: "integer",
                nullable: false,
                defaultValue: 25);

            migrationBuilder.AddColumn<bool>(
                name: "is_completed",
                table: "focus_sessions",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "focus_sessions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "started_at",
                table: "focus_sessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "actual_seconds",
                table: "focus_sessions");

            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "focus_sessions");

            migrationBuilder.DropColumn(
                name: "duration_minutes",
                table: "focus_sessions");

            migrationBuilder.DropColumn(
                name: "is_completed",
                table: "focus_sessions");

            migrationBuilder.DropColumn(
                name: "note",
                table: "focus_sessions");

            migrationBuilder.DropColumn(
                name: "started_at",
                table: "focus_sessions");
        }
    }
}
