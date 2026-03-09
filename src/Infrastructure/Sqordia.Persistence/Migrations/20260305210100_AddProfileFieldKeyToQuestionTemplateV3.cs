using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileFieldKeyToQuestionTemplateV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileFieldKey",
                table: "QuestionTemplatesV3",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTemplatesV3_ProfileFieldKey",
                table: "QuestionTemplatesV3",
                column: "ProfileFieldKey",
                filter: "\"ProfileFieldKey\" IS NOT NULL");

            // Seed ProfileFieldKey mappings for existing V3 questions
            // Q1 = Business name/activity → companyName
            migrationBuilder.Sql(
                "UPDATE \"QuestionTemplatesV3\" SET \"ProfileFieldKey\" = 'companyName' WHERE \"QuestionNumber\" = 1 AND \"IsDeleted\" = false");
            // Q5 = Industry sector → industry
            migrationBuilder.Sql(
                "UPDATE \"QuestionTemplatesV3\" SET \"ProfileFieldKey\" = 'industry' WHERE \"QuestionNumber\" = 5 AND \"IsDeleted\" = false");
            // Q6 = Target customer → targetMarket
            migrationBuilder.Sql(
                "UPDATE \"QuestionTemplatesV3\" SET \"ProfileFieldKey\" = 'targetMarket' WHERE \"QuestionNumber\" = 6 AND \"IsDeleted\" = false");
            // Q10 = Team composition → teamSize
            migrationBuilder.Sql(
                "UPDATE \"QuestionTemplatesV3\" SET \"ProfileFieldKey\" = 'teamSize' WHERE \"QuestionNumber\" = 10 AND \"IsDeleted\" = false");
            // Q14 = Financing needs → fundingStatus
            migrationBuilder.Sql(
                "UPDATE \"QuestionTemplatesV3\" SET \"ProfileFieldKey\" = 'fundingStatus' WHERE \"QuestionNumber\" = 14 AND \"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuestionTemplatesV3_ProfileFieldKey",
                table: "QuestionTemplatesV3");

            migrationBuilder.DropColumn(
                name: "ProfileFieldKey",
                table: "QuestionTemplatesV3");
        }
    }
}
