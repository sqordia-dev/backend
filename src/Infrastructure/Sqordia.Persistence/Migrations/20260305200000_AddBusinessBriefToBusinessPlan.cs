using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessBriefToBusinessPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Phase 1: Business Brief
            migrationBuilder.AddColumn<string>(
                name: "BusinessBriefJson",
                table: "BusinessPlans",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BusinessBriefGeneratedAt",
                table: "BusinessPlans",
                type: "timestamp with time zone",
                nullable: true);

            // Phase 2: Multi-pass generation pipeline
            migrationBuilder.AddColumn<string>(
                name: "GenerationPlanJson",
                table: "BusinessPlans",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QualityReportJson",
                table: "BusinessPlans",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BankReadinessScore",
                table: "BusinessPlans",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QualityScore",
                table: "BusinessPlans",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QualityScore",
                table: "BusinessPlans");

            migrationBuilder.DropColumn(
                name: "BankReadinessScore",
                table: "BusinessPlans");

            migrationBuilder.DropColumn(
                name: "QualityReportJson",
                table: "BusinessPlans");

            migrationBuilder.DropColumn(
                name: "GenerationPlanJson",
                table: "BusinessPlans");

            migrationBuilder.DropColumn(
                name: "BusinessBriefGeneratedAt",
                table: "BusinessPlans");

            migrationBuilder.DropColumn(
                name: "BusinessBriefJson",
                table: "BusinessPlans");
        }
    }
}
