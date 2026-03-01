using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionTemplateV3IdToResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SectionPrompts_Unique_Active_Master",
                table: "SectionPrompts");

            migrationBuilder.DropIndex(
                name: "IX_SectionPrompts_Unique_Active_Override",
                table: "SectionPrompts");

            migrationBuilder.DropIndex(
                name: "IX_PromptTemplates_Unique_Active",
                table: "PromptTemplates");

            migrationBuilder.AlterColumn<string>(
                name: "NoteFR",
                table: "SubSections",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NoteEN",
                table: "SubSections",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VisualElementsJson",
                table: "SectionPrompts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VariablesJson",
                table: "SectionPrompts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserPromptTemplate",
                table: "SectionPrompts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SystemPrompt",
                table: "SectionPrompts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ExampleOutput",
                table: "SectionPrompts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ValidationRules",
                table: "QuestionTemplatesV3",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OptionsFR",
                table: "QuestionTemplatesV3",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OptionsEN",
                table: "QuestionTemplatesV3",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HelpTextFR",
                table: "QuestionTemplatesV3",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HelpTextEN",
                table: "QuestionTemplatesV3",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExpertAdviceFR",
                table: "QuestionTemplatesV3",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExpertAdviceEN",
                table: "QuestionTemplatesV3",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConditionalLogic",
                table: "QuestionTemplatesV3",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CoachPromptFR",
                table: "QuestionTemplatesV3",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CoachPromptEN",
                table: "QuestionTemplatesV3",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "QuestionTemplateV3Id",
                table: "QuestionnaireResponses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VisualElementsJson",
                table: "PromptTemplates",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExampleOutput",
                table: "PromptTemplates",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

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
                name: "IX_QuestionnaireResponses_BusinessPlanId_QuestionTemplateV3Id",
                table: "QuestionnaireResponses",
                columns: new[] { "BusinessPlanId", "QuestionTemplateV3Id" },
                unique: true,
                filter: "\"QuestionTemplateV3Id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireResponses_QuestionTemplateV3Id",
                table: "QuestionnaireResponses",
                column: "QuestionTemplateV3Id");

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_Unique_Active",
                table: "PromptTemplates",
                columns: new[] { "SectionType", "PlanType", "IndustryCategory", "IsActive" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireResponses_QuestionTemplatesV3_QuestionTemplate~",
                table: "QuestionnaireResponses",
                column: "QuestionTemplateV3Id",
                principalTable: "QuestionTemplatesV3",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireResponses_QuestionTemplatesV3_QuestionTemplate~",
                table: "QuestionnaireResponses");

            migrationBuilder.DropIndex(
                name: "IX_SectionPrompts_Unique_Active_Master",
                table: "SectionPrompts");

            migrationBuilder.DropIndex(
                name: "IX_SectionPrompts_Unique_Active_Override",
                table: "SectionPrompts");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireResponses_BusinessPlanId_QuestionTemplateV3Id",
                table: "QuestionnaireResponses");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireResponses_QuestionTemplateV3Id",
                table: "QuestionnaireResponses");

            migrationBuilder.DropIndex(
                name: "IX_PromptTemplates_Unique_Active",
                table: "PromptTemplates");

            migrationBuilder.DropColumn(
                name: "QuestionTemplateV3Id",
                table: "QuestionnaireResponses");

            migrationBuilder.AlterColumn<string>(
                name: "NoteFR",
                table: "SubSections",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NoteEN",
                table: "SubSections",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VisualElementsJson",
                table: "SectionPrompts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VariablesJson",
                table: "SectionPrompts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserPromptTemplate",
                table: "SectionPrompts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SystemPrompt",
                table: "SectionPrompts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ExampleOutput",
                table: "SectionPrompts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ValidationRules",
                table: "QuestionTemplatesV3",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OptionsFR",
                table: "QuestionTemplatesV3",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OptionsEN",
                table: "QuestionTemplatesV3",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HelpTextFR",
                table: "QuestionTemplatesV3",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HelpTextEN",
                table: "QuestionTemplatesV3",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExpertAdviceFR",
                table: "QuestionTemplatesV3",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExpertAdviceEN",
                table: "QuestionTemplatesV3",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConditionalLogic",
                table: "QuestionTemplatesV3",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CoachPromptFR",
                table: "QuestionTemplatesV3",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CoachPromptEN",
                table: "QuestionTemplatesV3",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VisualElementsJson",
                table: "PromptTemplates",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExampleOutput",
                table: "PromptTemplates",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectionPrompts_Unique_Active_Master",
                table: "SectionPrompts",
                columns: new[] { "MainSectionId", "PlanType", "Language", "IndustryCategory", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1 AND [MainSectionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SectionPrompts_Unique_Active_Override",
                table: "SectionPrompts",
                columns: new[] { "SubSectionId", "PlanType", "Language", "IndustryCategory", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1 AND [SubSectionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_Unique_Active",
                table: "PromptTemplates",
                columns: new[] { "SectionType", "PlanType", "IndustryCategory", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");
        }
    }
}
