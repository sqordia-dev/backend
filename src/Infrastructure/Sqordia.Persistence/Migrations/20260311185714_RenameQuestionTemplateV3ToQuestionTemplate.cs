using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameQuestionTemplateV3ToQuestionTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys referencing old table/column names
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireResponses_QuestionTemplatesV3_QuestionTemplate~",
                table: "QuestionnaireResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionSectionMappings_QuestionTemplatesV3_QuestionTemplat~",
                table: "QuestionSectionMappings");

            // Drop old indexes that reference V3 names
            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireResponses_BusinessPlanId_QuestionTemplateV3Id",
                table: "QuestionnaireResponses");

            migrationBuilder.DropIndex(
                name: "IX_QuestionTemplatesV3_Active_Order",
                table: "QuestionTemplatesV3");

            migrationBuilder.DropIndex(
                name: "IX_QuestionTemplatesV3_Persona_Step_Active",
                table: "QuestionTemplatesV3");

            migrationBuilder.DropIndex(
                name: "IX_QuestionTemplatesV3_ProfileFieldKey",
                table: "QuestionTemplatesV3");

            migrationBuilder.DropIndex(
                name: "IX_QuestionTemplatesV3_QuestionNumber",
                table: "QuestionTemplatesV3");

            migrationBuilder.DropIndex(
                name: "IX_QuestionTemplatesV3_Step_Order",
                table: "QuestionTemplatesV3");

            // Rename the table
            migrationBuilder.RenameTable(
                name: "QuestionTemplatesV3",
                newName: "QuestionTemplates");

            // Rename FK columns
            migrationBuilder.RenameColumn(
                name: "QuestionTemplateV3Id",
                table: "QuestionnaireResponses",
                newName: "QuestionTemplateId");

            migrationBuilder.RenameColumn(
                name: "QuestionTemplateV3Id",
                table: "QuestionSectionMappings",
                newName: "QuestionTemplateId");

            // Rename existing indexes on QuestionnaireResponses
            migrationBuilder.RenameIndex(
                name: "IX_QuestionnaireResponses_QuestionTemplateV3Id",
                table: "QuestionnaireResponses",
                newName: "IX_QuestionnaireResponses_QuestionTemplateId");

            // Recreate indexes with new names on renamed table
            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplates_Active_Order",
                table: "QuestionTemplates",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplates_Persona_Step_Active",
                table: "QuestionTemplates",
                columns: new[] { "PersonaType", "StepNumber", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplates_ProfileFieldKey",
                table: "QuestionTemplates",
                column: "ProfileFieldKey",
                filter: "\"ProfileFieldKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplates_QuestionNumber",
                table: "QuestionTemplates",
                column: "QuestionNumber");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplates_Step_Order",
                table: "QuestionTemplates",
                columns: new[] { "StepNumber", "DisplayOrder" });

            // Recreate composite unique index with new column name
            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireResponses_BusinessPlanId_QuestionTemplateId",
                table: "QuestionnaireResponses",
                columns: new[] { "BusinessPlanId", "QuestionTemplateId" },
                unique: true,
                filter: "\"QuestionTemplateId\" IS NOT NULL");

            // Recreate foreign keys with new names
            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireResponses_QuestionTemplates_QuestionTemplateId",
                table: "QuestionnaireResponses",
                column: "QuestionTemplateId",
                principalTable: "QuestionTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionSectionMappings_QuestionTemplates_QuestionTemplateId",
                table: "QuestionSectionMappings",
                column: "QuestionTemplateId",
                principalTable: "QuestionTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop new foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireResponses_QuestionTemplates_QuestionTemplateId",
                table: "QuestionnaireResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionSectionMappings_QuestionTemplates_QuestionTemplateId",
                table: "QuestionSectionMappings");

            // Drop new indexes
            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireResponses_BusinessPlanId_QuestionTemplateId",
                table: "QuestionnaireResponses");

            migrationBuilder.DropIndex(
                name: "IX_QuestionTemplates_Active_Order",
                table: "QuestionTemplates");

            migrationBuilder.DropIndex(
                name: "IX_QuestionTemplates_Persona_Step_Active",
                table: "QuestionTemplates");

            migrationBuilder.DropIndex(
                name: "IX_QuestionTemplates_ProfileFieldKey",
                table: "QuestionTemplates");

            migrationBuilder.DropIndex(
                name: "IX_QuestionTemplates_QuestionNumber",
                table: "QuestionTemplates");

            migrationBuilder.DropIndex(
                name: "IX_QuestionTemplates_Step_Order",
                table: "QuestionTemplates");

            // Rename table back
            migrationBuilder.RenameTable(
                name: "QuestionTemplates",
                newName: "QuestionTemplatesV3");

            // Rename columns back
            migrationBuilder.RenameColumn(
                name: "QuestionTemplateId",
                table: "QuestionnaireResponses",
                newName: "QuestionTemplateV3Id");

            migrationBuilder.RenameColumn(
                name: "QuestionTemplateId",
                table: "QuestionSectionMappings",
                newName: "QuestionTemplateV3Id");

            // Rename index back
            migrationBuilder.RenameIndex(
                name: "IX_QuestionnaireResponses_QuestionTemplateId",
                table: "QuestionnaireResponses",
                newName: "IX_QuestionnaireResponses_QuestionTemplateV3Id");

            // Recreate old indexes
            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireResponses_BusinessPlanId_QuestionTemplateV3Id",
                table: "QuestionnaireResponses",
                columns: new[] { "BusinessPlanId", "QuestionTemplateV3Id" },
                unique: true,
                filter: "\"QuestionTemplateV3Id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplatesV3_Active_Order",
                table: "QuestionTemplatesV3",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplatesV3_Persona_Step_Active",
                table: "QuestionTemplatesV3",
                columns: new[] { "PersonaType", "StepNumber", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplatesV3_ProfileFieldKey",
                table: "QuestionTemplatesV3",
                column: "ProfileFieldKey",
                filter: "\"ProfileFieldKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplatesV3_QuestionNumber",
                table: "QuestionTemplatesV3",
                column: "QuestionNumber");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplatesV3_Step_Order",
                table: "QuestionTemplatesV3",
                columns: new[] { "StepNumber", "DisplayOrder" });

            // Recreate old foreign keys
            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireResponses_QuestionTemplatesV3_QuestionTemplate~",
                table: "QuestionnaireResponses",
                column: "QuestionTemplateV3Id",
                principalTable: "QuestionTemplatesV3",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionSectionMappings_QuestionTemplatesV3_QuestionTemplat~",
                table: "QuestionSectionMappings",
                column: "QuestionTemplateV3Id",
                principalTable: "QuestionTemplatesV3",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
