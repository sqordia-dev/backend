using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStartMonthSalesTaxFrequencyAndLegalForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LegalForm",
                table: "Organizations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesTaxFrequency",
                table: "FinancialPlans",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartMonth",
                table: "FinancialPlans",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LegalForm",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "SalesTaxFrequency",
                table: "FinancialPlans");

            migrationBuilder.DropColumn(
                name: "StartMonth",
                table: "FinancialPlans");
        }
    }
}
