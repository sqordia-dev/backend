using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveV1V2QuestionnaireTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireResponses_QuestionTemplatesV2_QuestionTemplate~",
                table: "QuestionnaireResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireResponses_QuestionTemplates_QuestionTemplateId",
                table: "QuestionnaireResponses");

            migrationBuilder.DropTable(
                name: "QuestionTemplates");

            migrationBuilder.DropTable(
                name: "QuestionTemplatesV2");

            migrationBuilder.DropTable(
                name: "QuestionnaireTemplates");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireResponses_BusinessPlanId_QuestionTemplateId",
                table: "QuestionnaireResponses");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireResponses_BusinessPlanId_QuestionTemplateV2Id",
                table: "QuestionnaireResponses");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireResponses_QuestionTemplateId",
                table: "QuestionnaireResponses");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireResponses_QuestionTemplateV2Id",
                table: "QuestionnaireResponses");

            migrationBuilder.DropColumn(
                name: "QuestionTemplateId",
                table: "QuestionnaireResponses");

            migrationBuilder.DropColumn(
                name: "QuestionTemplateV2Id",
                table: "QuestionnaireResponses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "QuestionTemplateId",
                table: "QuestionnaireResponses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "QuestionTemplateV2Id",
                table: "QuestionnaireResponses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuestionnaireTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PlanType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionTemplatesV2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionalLogic = table.Column<string>(type: "jsonb", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    HelpText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    HelpTextEN = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Options = table.Column<string>(type: "jsonb", nullable: true),
                    OptionsEN = table.Column<string>(type: "jsonb", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    PersonaType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    QuestionText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    QuestionTextEN = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    QuestionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Section = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StepNumber = table.Column<int>(type: "integer", nullable: false),
                    ValidationRules = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionTemplatesV2", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionalLogic = table.Column<string>(type: "text", nullable: true),
                    HelpText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HelpTextEN = table.Column<string>(type: "text", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    Options = table.Column<string>(type: "text", nullable: true),
                    OptionsEN = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    QuestionTextEN = table.Column<string>(type: "text", nullable: true),
                    QuestionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Section = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ValidationRules = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionTemplates_QuestionnaireTemplates_QuestionnaireTempl~",
                        column: x => x.QuestionnaireTemplateId,
                        principalTable: "QuestionnaireTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireResponses_BusinessPlanId_QuestionTemplateId",
                table: "QuestionnaireResponses",
                columns: new[] { "BusinessPlanId", "QuestionTemplateId" },
                unique: true,
                filter: "\"QuestionTemplateId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireResponses_BusinessPlanId_QuestionTemplateV2Id",
                table: "QuestionnaireResponses",
                columns: new[] { "BusinessPlanId", "QuestionTemplateV2Id" },
                unique: true,
                filter: "\"QuestionTemplateV2Id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireResponses_QuestionTemplateId",
                table: "QuestionnaireResponses",
                column: "QuestionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireResponses_QuestionTemplateV2Id",
                table: "QuestionnaireResponses",
                column: "QuestionTemplateV2Id");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_IsActive",
                table: "QuestionnaireTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_PlanType",
                table: "QuestionnaireTemplates",
                column: "PlanType");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_PlanType_IsActive_Version",
                table: "QuestionnaireTemplates",
                columns: new[] { "PlanType", "IsActive", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplates_QuestionnaireTemplateId",
                table: "QuestionTemplates",
                column: "QuestionnaireTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplates_QuestionnaireTemplateId_Order",
                table: "QuestionTemplates",
                columns: new[] { "QuestionnaireTemplateId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplatesV2_IsActive",
                table: "QuestionTemplatesV2",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplatesV2_PersonaType",
                table: "QuestionTemplatesV2",
                column: "PersonaType");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplatesV2_PersonaType_StepNumber_Order",
                table: "QuestionTemplatesV2",
                columns: new[] { "PersonaType", "StepNumber", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplatesV2_StepNumber",
                table: "QuestionTemplatesV2",
                column: "StepNumber");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplatesV2_StepNumber_Order",
                table: "QuestionTemplatesV2",
                columns: new[] { "StepNumber", "Order" });

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireResponses_QuestionTemplatesV2_QuestionTemplate~",
                table: "QuestionnaireResponses",
                column: "QuestionTemplateV2Id",
                principalTable: "QuestionTemplatesV2",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireResponses_QuestionTemplates_QuestionTemplateId",
                table: "QuestionnaireResponses",
                column: "QuestionTemplateId",
                principalTable: "QuestionTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
