using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedWebhookEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "processed_webhook_events",
                columns: table => new
                {
                    event_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_webhook_events", x => x.event_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_OrgId_UserId_IsActive",
                table: "OrganizationMembers",
                columns: new[] { "OrganizationId", "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedWebhookEvents_ProcessedAt",
                table: "processed_webhook_events",
                column: "processed_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_webhook_events");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationMembers_OrgId_UserId_IsActive",
                table: "OrganizationMembers");
        }
    }
}
