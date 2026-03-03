using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAICoachTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AICoachConversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    QuestionNumber = table.Column<int>(type: "integer", nullable: true),
                    QuestionText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "en"),
                    Persona = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TotalTokensUsed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastMessageAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AICoachConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AICoachUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    TotalTokensUsed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AICoachUsages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AICoachMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    TokenCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AICoachMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AICoachMessages_AICoachConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "AICoachConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AICoachConversations_BusinessPlanId",
                table: "AICoachConversations",
                column: "BusinessPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_AICoachConversations_LastMessageAt",
                table: "AICoachConversations",
                column: "LastMessageAt");

            migrationBuilder.CreateIndex(
                name: "IX_AICoachConversations_UserId",
                table: "AICoachConversations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AICoachConversations_UserId_BusinessPlanId_QuestionId",
                table: "AICoachConversations",
                columns: new[] { "UserId", "BusinessPlanId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AICoachMessages_ConversationId",
                table: "AICoachMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_AICoachMessages_ConversationId_Sequence",
                table: "AICoachMessages",
                columns: new[] { "ConversationId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_AICoachMessages_Created",
                table: "AICoachMessages",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_AICoachUsages_Month",
                table: "AICoachUsages",
                column: "Month");

            migrationBuilder.CreateIndex(
                name: "IX_AICoachUsages_OrganizationId",
                table: "AICoachUsages",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AICoachUsages_UserId",
                table: "AICoachUsages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AICoachUsages_UserId_OrganizationId_Month",
                table: "AICoachUsages",
                columns: new[] { "UserId", "OrganizationId", "Month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AICoachMessages");

            migrationBuilder.DropTable(
                name: "AICoachUsages");

            migrationBuilder.DropTable(
                name: "AICoachConversations");
        }
    }
}
