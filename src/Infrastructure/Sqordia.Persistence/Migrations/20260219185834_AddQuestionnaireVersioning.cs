using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionnaireVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionnaireSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StepNumber = table.Column<int>(type: "integer", nullable: false),
                    TitleFR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TitleEN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DescriptionFR = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DescriptionEN = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_QuestionnaireSteps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    QuestionsSnapshot = table.Column<string>(type: "text", nullable: false),
                    StepsSnapshot = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_QuestionnaireVersions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireSteps_StepNumber",
                table: "QuestionnaireSteps",
                column: "StepNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireVersions_Status",
                table: "QuestionnaireVersions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireVersions_VersionNumber",
                table: "QuestionnaireVersions",
                column: "VersionNumber");

            // Seed initial questionnaire steps
            migrationBuilder.Sql(@"
                INSERT INTO ""QuestionnaireSteps"" (""Id"", ""StepNumber"", ""TitleFR"", ""TitleEN"", ""DescriptionFR"", ""DescriptionEN"", ""Icon"", ""IsActive"", ""Created"", ""IsDeleted"")
                VALUES
                (gen_random_uuid(), 1, 'Identité & Vision', 'Identity & Vision', 'Définissez qui vous êtes et où vous allez', 'Define who you are and where you''re going', NULL, true, NOW(), false),
                (gen_random_uuid(), 2, 'L''Offre', 'The Offering', 'Décrivez vos produits et services', 'Describe your products and services', NULL, true, NOW(), false),
                (gen_random_uuid(), 3, 'Analyse de marché', 'Market Analysis', 'Comprenez votre marché et vos concurrents', 'Understand your market and competitors', NULL, true, NOW(), false),
                (gen_random_uuid(), 4, 'Opérations & Équipe', 'Operations & People', 'Planifiez vos opérations et votre équipe', 'Plan your operations and team', NULL, true, NOW(), false),
                (gen_random_uuid(), 5, 'Finances & Risques', 'Financials & Risks', 'Évaluez vos projections financières et risques', 'Assess your financial projections and risks', NULL, true, NOW(), false)
                ON CONFLICT DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionnaireSteps");

            migrationBuilder.DropTable(
                name: "QuestionnaireVersions");
        }
    }
}
