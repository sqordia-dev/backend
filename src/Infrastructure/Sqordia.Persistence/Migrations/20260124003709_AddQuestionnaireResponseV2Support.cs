using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionnaireResponseV2Support : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireResponses_BusinessPlanId_QuestionTemplateId",
                table: "QuestionnaireResponses");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuestionTemplateId",
                table: "QuestionnaireResponses",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "QuestionTemplateV2Id",
                table: "QuestionnaireResponses",
                type: "uuid",
                nullable: true);

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
                name: "IX_QuestionnaireResponses_QuestionTemplateV2Id",
                table: "QuestionnaireResponses",
                column: "QuestionTemplateV2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireResponses_QuestionTemplatesV2_QuestionTemplate~",
                table: "QuestionnaireResponses",
                column: "QuestionTemplateV2Id",
                principalTable: "QuestionTemplatesV2",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireResponses_QuestionTemplatesV2_QuestionTemplate~",
                table: "QuestionnaireResponses");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireResponses_BusinessPlanId_QuestionTemplateId",
                table: "QuestionnaireResponses");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireResponses_BusinessPlanId_QuestionTemplateV2Id",
                table: "QuestionnaireResponses");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireResponses_QuestionTemplateV2Id",
                table: "QuestionnaireResponses");

            migrationBuilder.DropColumn(
                name: "QuestionTemplateV2Id",
                table: "QuestionnaireResponses");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuestionTemplateId",
                table: "QuestionnaireResponses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireResponses_BusinessPlanId_QuestionTemplateId",
                table: "QuestionnaireResponses",
                columns: new[] { "BusinessPlanId", "QuestionTemplateId" },
                unique: true);
        }
    }
}
