using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace be_pomo_do.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAndAuditSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_admin",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_suspended",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "suspended_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "suspended_reason",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "admin_action_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    details = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_action_histories", x => x.id);
                    table.ForeignKey(
                        name: "FK_admin_action_histories_users_admin_id",
                        column: x => x.admin_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_admin_action_histories_users_target_user_id",
                        column: x => x.target_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_action_histories_admin_id",
                table: "admin_action_histories",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_admin_action_histories_created_at",
                table: "admin_action_histories",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_admin_action_histories_target_user_id",
                table: "admin_action_histories",
                column: "target_user_id");

            // Bootstrap superadmin
            migrationBuilder.Sql("UPDATE users SET is_admin = true WHERE email = 'mngo64825@gmail.com';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_action_histories");

            migrationBuilder.DropColumn(
                name: "is_admin",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_suspended",
                table: "users");

            migrationBuilder.DropColumn(
                name: "suspended_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "suspended_reason",
                table: "users");
        }
    }
}
