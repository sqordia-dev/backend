using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptRepository : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PromptTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PlanType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IndustryCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Alias = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SystemPrompt = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    UserPromptTemplate = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    OutputFormat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VisualElementsJson = table.Column<string>(type: "text", nullable: true),
                    ExampleOutput = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromptPerformance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PromptTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsageCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    EditCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RegenerateCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AcceptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalRating = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: false, defaultValue: 0.0),
                    RatingCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptPerformance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromptPerformance_PromptTemplates_PromptTemplateId",
                        column: x => x.PromptTemplateId,
                        principalTable: "PromptTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromptPerformance_Period",
                table: "PromptPerformance",
                columns: new[] { "PeriodStart", "PeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_PromptPerformance_PromptId_PeriodStart",
                table: "PromptPerformance",
                columns: new[] { "PromptTemplateId", "PeriodStart" });

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_Section_Industry_Active",
                table: "PromptTemplates",
                columns: new[] { "SectionType", "IndustryCategory", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_Section_PlanType_Active",
                table: "PromptTemplates",
                columns: new[] { "SectionType", "PlanType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_Section_PlanType_Alias",
                table: "PromptTemplates",
                columns: new[] { "SectionType", "PlanType", "Alias" });

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_Section_PlanType_Version",
                table: "PromptTemplates",
                columns: new[] { "SectionType", "PlanType", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_Unique_Active",
                table: "PromptTemplates",
                columns: new[] { "SectionType", "PlanType", "IndustryCategory", "IsActive" },
                unique: true,
                filter: "\"IsActive\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromptPerformance");

            migrationBuilder.DropTable(
                name: "PromptTemplates");
        }
    }
}
