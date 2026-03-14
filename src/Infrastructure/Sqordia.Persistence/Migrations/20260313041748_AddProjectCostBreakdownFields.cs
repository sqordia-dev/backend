using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectCostBreakdownFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdminExpAcquireAfter",
                table: "ProjectCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AdminExpAcquireBefore",
                table: "ProjectCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AdminExpAlreadyAcquired",
                table: "ProjectCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "AdminExpDurationMonths",
                table: "ProjectCosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CapexAcquireAfter",
                table: "ProjectCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CapexAcquireBefore",
                table: "ProjectCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CapexAlreadyAcquired",
                table: "ProjectCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CapexDurationMonths",
                table: "ProjectCosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "InventoryAcquireAfter",
                table: "ProjectCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InventoryAcquireBefore",
                table: "ProjectCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InventoryAlreadyAcquired",
                table: "ProjectCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "InventoryDurationMonths",
                table: "ProjectCosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryAcquireAfter",
                table: "ProjectCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryAcquireBefore",
                table: "ProjectCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryAlreadyAcquired",
                table: "ProjectCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SalaryDurationMonths",
                table: "ProjectCosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SalesExpAcquireAfter",
                table: "ProjectCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SalesExpAcquireBefore",
                table: "ProjectCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SalesExpAlreadyAcquired",
                table: "ProjectCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SalesExpDurationMonths",
                table: "ProjectCosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminExpAcquireAfter",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "AdminExpAcquireBefore",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "AdminExpAlreadyAcquired",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "AdminExpDurationMonths",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "CapexAcquireAfter",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "CapexAcquireBefore",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "CapexAlreadyAcquired",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "CapexDurationMonths",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "InventoryAcquireAfter",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "InventoryAcquireBefore",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "InventoryAlreadyAcquired",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "InventoryDurationMonths",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "SalaryAcquireAfter",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "SalaryAcquireBefore",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "SalaryAlreadyAcquired",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "SalaryDurationMonths",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "SalesExpAcquireAfter",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "SalesExpAcquireBefore",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "SalesExpAlreadyAcquired",
                table: "ProjectCosts");

            migrationBuilder.DropColumn(
                name: "SalesExpDurationMonths",
                table: "ProjectCosts");
        }
    }
}
