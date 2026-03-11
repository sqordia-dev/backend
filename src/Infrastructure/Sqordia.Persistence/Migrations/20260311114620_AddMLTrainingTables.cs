using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMLTrainingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_call_telemetry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    PromptTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ModelUsed = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SectionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PipelinePass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InputTokens = table.Column<int>(type: "integer", nullable: false),
                    OutputTokens = table.Column<int>(type: "integer", nullable: false),
                    LatencyMs = table.Column<long>(type: "bigint", nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Temperature = table.Column<float>(type: "real", nullable: false),
                    QualityScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    WasAccepted = table.Column<bool>(type: "boolean", nullable: true),
                    WasRegenerated = table.Column<bool>(type: "boolean", nullable: true),
                    WasEdited = table.Column<bool>(type: "boolean", nullable: true),
                    EditRatio = table.Column<double>(type: "double precision", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_call_telemetry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "learned_preferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Industry = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PlanType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PreferenceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PreferenceJson = table.Column<string>(type: "text", nullable: false),
                    SampleCount = table.Column<int>(type: "integer", nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LearnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learned_preferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "section_edit_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AiGeneratedContent = table.Column<string>(type: "text", nullable: false),
                    UserEditedContent = table.Column<string>(type: "text", nullable: false),
                    EditDistance = table.Column<int>(type: "integer", nullable: false),
                    EditRatio = table.Column<double>(type: "double precision", nullable: false),
                    PromptTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    Industry = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PlanType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_section_edit_history", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_call_telemetry_BusinessPlanId",
                table: "ai_call_telemetry",
                column: "BusinessPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_call_telemetry_CreatedAt",
                table: "ai_call_telemetry",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ai_call_telemetry_PromptTemplateId",
                table: "ai_call_telemetry",
                column: "PromptTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_call_telemetry_SectionType",
                table: "ai_call_telemetry",
                column: "SectionType");

            migrationBuilder.CreateIndex(
                name: "IX_ai_call_telemetry_SectionType_CreatedAt",
                table: "ai_call_telemetry",
                columns: new[] { "SectionType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_learned_preferences_SectionType_Industry_IsActive",
                table: "learned_preferences",
                columns: new[] { "SectionType", "Industry", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_learned_preferences_SectionType_Language_IsActive",
                table: "learned_preferences",
                columns: new[] { "SectionType", "Language", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_section_edit_history_BusinessPlanId",
                table: "section_edit_history",
                column: "BusinessPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_section_edit_history_CreatedAt",
                table: "section_edit_history",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_section_edit_history_SectionType",
                table: "section_edit_history",
                column: "SectionType");

            migrationBuilder.CreateIndex(
                name: "IX_section_edit_history_SectionType_Industry",
                table: "section_edit_history",
                columns: new[] { "SectionType", "Industry" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_call_telemetry");

            migrationBuilder.DropTable(
                name: "learned_preferences");

            migrationBuilder.DropTable(
                name: "section_edit_history");
        }
    }
}
