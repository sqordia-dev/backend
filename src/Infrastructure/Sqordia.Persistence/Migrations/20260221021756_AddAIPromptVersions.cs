using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAIPromptVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIPromptVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AIPromptId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    SystemPrompt = table.Column<string>(type: "text", nullable: false),
                    UserPromptTemplate = table.Column<string>(type: "text", nullable: false),
                    Variables = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIPromptVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIPromptVersions_AIPrompts_AIPromptId",
                        column: x => x.AIPromptId,
                        principalTable: "AIPrompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIPromptVersions_AIPromptId",
                table: "AIPromptVersions",
                column: "AIPromptId");

            migrationBuilder.CreateIndex(
                name: "IX_AIPromptVersions_AIPromptId_Version",
                table: "AIPromptVersions",
                columns: new[] { "AIPromptId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIPromptVersions_ChangedAt",
                table: "AIPromptVersions",
                column: "ChangedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIPromptVersions");
        }
    }
}
