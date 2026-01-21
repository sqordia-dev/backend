using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGrowthArchitectV2Features : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentGenerationSection",
                table: "BusinessPlans",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GenerationProgress",
                table: "BusinessPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyBurnRate",
                table: "BusinessPlans",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Persona",
                table: "BusinessPlans",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PivotPointMonth",
                table: "BusinessPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReadinessScore",
                table: "BusinessPlans",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RunwayMonths",
                table: "BusinessPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StrategyMapJson",
                table: "BusinessPlans",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetCAC",
                table: "BusinessPlans",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuestionTemplatesV2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonaType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StepNumber = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    QuestionTextEN = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    HelpText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    HelpTextEN = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    QuestionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    Section = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Options = table.Column<string>(type: "jsonb", nullable: true),
                    OptionsEN = table.Column<string>(type: "jsonb", nullable: true),
                    ValidationRules = table.Column<string>(type: "jsonb", nullable: true),
                    ConditionalLogic = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_QuestionTemplatesV2", x => x.Id);
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionTemplatesV2");

            migrationBuilder.DropColumn(
                name: "CurrentGenerationSection",
                table: "BusinessPlans");

            migrationBuilder.DropColumn(
                name: "GenerationProgress",
                table: "BusinessPlans");

            migrationBuilder.DropColumn(
                name: "MonthlyBurnRate",
                table: "BusinessPlans");

            migrationBuilder.DropColumn(
                name: "Persona",
                table: "BusinessPlans");

            migrationBuilder.DropColumn(
                name: "PivotPointMonth",
                table: "BusinessPlans");

            migrationBuilder.DropColumn(
                name: "ReadinessScore",
                table: "BusinessPlans");

            migrationBuilder.DropColumn(
                name: "RunwayMonths",
                table: "BusinessPlans");

            migrationBuilder.DropColumn(
                name: "StrategyMapJson",
                table: "BusinessPlans");

            migrationBuilder.DropColumn(
                name: "TargetCAC",
                table: "BusinessPlans");
        }
    }
}
