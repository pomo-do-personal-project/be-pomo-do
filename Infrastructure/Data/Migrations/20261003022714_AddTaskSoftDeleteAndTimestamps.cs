using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace be_pomo_do.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskSoftDeleteAndTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "tasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "tasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "tasks",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "tasks");
        }
    }
}
