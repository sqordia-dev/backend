using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStructureFinaleEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MainSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TitleFR = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TitleEN = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DescriptionFR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DescriptionEN = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    GeneratedLast = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainSections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionTemplatesV3",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionNumber = table.Column<int>(type: "integer", nullable: false),
                    PersonaType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StepNumber = table.Column<int>(type: "integer", nullable: false),
                    QuestionTextFR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    QuestionTextEN = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    HelpTextFR = table.Column<string>(type: "text", nullable: true),
                    HelpTextEN = table.Column<string>(type: "text", nullable: true),
                    QuestionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OptionsFR = table.Column<string>(type: "text", nullable: true),
                    OptionsEN = table.Column<string>(type: "text", nullable: true),
                    ValidationRules = table.Column<string>(type: "text", nullable: true),
                    ConditionalLogic = table.Column<string>(type: "text", nullable: true),
                    CoachPromptFR = table.Column<string>(type: "text", nullable: true),
                    CoachPromptEN = table.Column<string>(type: "text", nullable: true),
                    ExpertAdviceFR = table.Column<string>(type: "text", nullable: true),
                    ExpertAdviceEN = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SectionGroup = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionTemplatesV3", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MainSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TitleFR = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TitleEN = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DescriptionFR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DescriptionEN = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NoteFR = table.Column<string>(type: "text", nullable: true),
                    NoteEN = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubSections_MainSections_MainSectionId",
                        column: x => x.MainSectionId,
                        principalTable: "MainSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionSectionMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionTemplateV3Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MappingContext = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "primary"),
                    Weight = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false, defaultValue: 1.0m),
                    TransformationHint = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionSectionMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionSectionMappings_QuestionTemplatesV3_QuestionTemplat~",
                        column: x => x.QuestionTemplateV3Id,
                        principalTable: "QuestionTemplatesV3",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionSectionMappings_SubSections_SubSectionId",
                        column: x => x.SubSectionId,
                        principalTable: "SubSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SectionPrompts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MainSectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubSectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlanType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IndustryCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SystemPrompt = table.Column<string>(type: "text", nullable: false),
                    UserPromptTemplate = table.Column<string>(type: "text", nullable: false),
                    VariablesJson = table.Column<string>(type: "text", nullable: true),
                    OutputFormat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VisualElementsJson = table.Column<string>(type: "text", nullable: true),
                    ExampleOutput = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionPrompts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectionPrompts_MainSections_MainSectionId",
                        column: x => x.MainSectionId,
                        principalTable: "MainSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SectionPrompts_SubSections_SubSectionId",
                        column: x => x.SubSectionId,
                        principalTable: "SubSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MainSections_Active_Order",
                table: "MainSections",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MainSections_Code",
                table: "MainSections",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MainSections_Number",
                table: "MainSections",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSectionMappings_Question_Active",
                table: "QuestionSectionMappings",
                columns: new[] { "QuestionTemplateV3Id", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSectionMappings_SubSection_Active",
                table: "QuestionSectionMappings",
                columns: new[] { "SubSectionId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSectionMappings_SubSection_Context_Active",
                table: "QuestionSectionMappings",
                columns: new[] { "SubSectionId", "MappingContext", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSectionMappings_SubSection_Order",
                table: "QuestionSectionMappings",
                columns: new[] { "SubSectionId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSectionMappings_Unique",
                table: "QuestionSectionMappings",
                columns: new[] { "QuestionTemplateV3Id", "SubSectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplatesV3_Active_Order",
                table: "QuestionTemplatesV3",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplatesV3_Persona_Step_Active",
                table: "QuestionTemplatesV3",
                columns: new[] { "PersonaType", "StepNumber", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplatesV3_QuestionNumber",
                table: "QuestionTemplatesV3",
                column: "QuestionNumber");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplatesV3_Step_Order",
                table: "QuestionTemplatesV3",
                columns: new[] { "StepNumber", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SectionPrompts_Level_Active",
                table: "SectionPrompts",
                columns: new[] { "Level", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SectionPrompts_MainSection_Plan_Lang_Active",
                table: "SectionPrompts",
                columns: new[] { "MainSectionId", "PlanType", "Language", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SectionPrompts_SubSection_Industry_Active",
                table: "SectionPrompts",
                columns: new[] { "SubSectionId", "IndustryCategory", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SectionPrompts_SubSection_Plan_Lang_Active",
                table: "SectionPrompts",
                columns: new[] { "SubSectionId", "PlanType", "Language", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SectionPrompts_Unique_Active_Master",
                table: "SectionPrompts",
                columns: new[] { "MainSectionId", "PlanType", "Language", "IndustryCategory", "IsActive" },
                unique: true,
                filter: "\"IsActive\" = true AND \"MainSectionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SectionPrompts_Unique_Active_Override",
                table: "SectionPrompts",
                columns: new[] { "SubSectionId", "PlanType", "Language", "IndustryCategory", "IsActive" },
                unique: true,
                filter: "\"IsActive\" = true AND \"SubSectionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SubSections_Code",
                table: "SubSections",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubSections_MainSection_Active",
                table: "SubSections",
                columns: new[] { "MainSectionId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SubSections_MainSection_Order",
                table: "SubSections",
                columns: new[] { "MainSectionId", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionSectionMappings");

            migrationBuilder.DropTable(
                name: "SectionPrompts");

            migrationBuilder.DropTable(
                name: "QuestionTemplatesV3");

            migrationBuilder.DropTable(
                name: "SubSections");

            migrationBuilder.DropTable(
                name: "MainSections");
        }
    }
}
