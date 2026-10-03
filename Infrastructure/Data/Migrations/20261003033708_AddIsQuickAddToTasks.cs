using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace be_pomo_do.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsQuickAddToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_quick_add",
                table: "tasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_quick_add",
                table: "tasks");
        }
    }
}
