using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCmsApprovalAndScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "CmsVersions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "CmsVersions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                table: "CmsVersions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "CmsVersions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectedByUserId",
                table: "CmsVersions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "CmsVersions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledPublishAt",
                table: "CmsVersions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CmsVersionHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CmsVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OldStatus = table.Column<int>(type: "integer", nullable: true),
                    NewStatus = table.Column<int>(type: "integer", nullable: true),
                    OldApprovalStatus = table.Column<int>(type: "integer", nullable: true),
                    NewApprovalStatus = table.Column<int>(type: "integer", nullable: true),
                    ChangeSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ScheduledPublishAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsVersionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsVersionHistory_CmsVersions_CmsVersionId",
                        column: x => x.CmsVersionId,
                        principalTable: "CmsVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CmsVersions_ApprovalStatus",
                table: "CmsVersions",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_CmsVersions_ScheduledPublishAt",
                table: "CmsVersions",
                column: "ScheduledPublishAt");

            migrationBuilder.CreateIndex(
                name: "IX_CmsVersionHistory_Action",
                table: "CmsVersionHistory",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_CmsVersionHistory_CmsVersionId",
                table: "CmsVersionHistory",
                column: "CmsVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_CmsVersionHistory_PerformedAt",
                table: "CmsVersionHistory",
                column: "PerformedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CmsVersionHistory");

            migrationBuilder.DropIndex(
                name: "IX_CmsVersions_ApprovalStatus",
                table: "CmsVersions");

            migrationBuilder.DropIndex(
                name: "IX_CmsVersions_ScheduledPublishAt",
                table: "CmsVersions");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "CmsVersions");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "CmsVersions");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "CmsVersions");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "CmsVersions");

            migrationBuilder.DropColumn(
                name: "RejectedByUserId",
                table: "CmsVersions");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "CmsVersions");

            migrationBuilder.DropColumn(
                name: "ScheduledPublishAt",
                table: "CmsVersions");
        }
    }
}
