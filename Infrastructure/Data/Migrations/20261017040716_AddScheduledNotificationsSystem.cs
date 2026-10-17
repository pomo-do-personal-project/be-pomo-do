using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace be_pomo_do.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledNotificationsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "scheduled_notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "info"),
                    scope = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "direct"),
                    recipient_emails = table.Column<string>(type: "text", nullable: true),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_sent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduled_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_scheduled_notifications_users_admin_id",
                        column: x => x.admin_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_notifications_admin_id",
                table: "scheduled_notifications",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_notifications_is_sent_scheduled_at",
                table: "scheduled_notifications",
                columns: new[] { "is_sent", "scheduled_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "scheduled_notifications");
        }
    }
}
