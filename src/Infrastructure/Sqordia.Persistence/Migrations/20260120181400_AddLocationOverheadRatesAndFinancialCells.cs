using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationOverheadRatesAndFinancialCells : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Persona",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowDownload",
                table: "BusinessPlanShares",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableWatermark",
                table: "BusinessPlanShares",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVaultShare",
                table: "BusinessPlanShares",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxViews",
                table: "BusinessPlanShares",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "BusinessPlanShares",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireEmailVerification",
                table: "BusinessPlanShares",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrackViews",
                table: "BusinessPlanShares",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "WatermarkText",
                table: "BusinessPlanShares",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FinancialCells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    SheetName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Main"),
                    RowId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ColumnId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Formula = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsCalculated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CellType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "number"),
                    DisplayFormat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialCells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialCells_BusinessPlans_BusinessPlanId",
                        column: x => x.BusinessPlanId,
                        principalTable: "BusinessPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationOverheadRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProvinceCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OverheadRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    InsuranceRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    OfficeCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "CAD"),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationOverheadRates", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "LocationOverheadRates",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Currency", "EffectiveDate", "ExpiryDate", "InsuranceRate", "IsActive", "OfficeCost", "OverheadRate", "Province", "ProvinceCode", "TaxRate", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("a0000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "CAD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 200m, true, 600m, 10.0m, "Alberta", "AB", 15.0m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { new Guid("a0000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "CAD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 250m, true, 800m, 12.0m, "British Columbia", "BC", 20.0m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { new Guid("a0000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "CAD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 180m, true, 450m, 10.0m, "Manitoba", "MB", 17.0m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { new Guid("a0000000-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "CAD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 170m, true, 400m, 9.0m, "New Brunswick", "NB", 20.0m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { new Guid("a0000000-0000-0000-0000-000000000005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "CAD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 175m, true, 420m, 9.0m, "Newfoundland and Labrador", "NL", 20.0m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { new Guid("a0000000-0000-0000-0000-000000000006"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "CAD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 175m, true, 450m, 9.5m, "Nova Scotia", "NS", 21.0m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { new Guid("a0000000-0000-0000-0000-000000000007"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "CAD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 250m, true, 750m, 12.0m, "Ontario", "ON", 20.0m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { new Guid("a0000000-0000-0000-0000-000000000008"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "CAD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 160m, true, 380m, 8.5m, "Prince Edward Island", "PE", 20.0m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { new Guid("a0000000-0000-0000-0000-000000000009"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "CAD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 220m, true, 600m, 11.0m, "Quebec", "QC", 24.0m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { new Guid("a0000000-0000-0000-0000-000000000010"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "CAD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 180m, true, 450m, 9.5m, "Saskatchewan", "SK", 16.0m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { new Guid("a0000000-0000-0000-0000-000000000011"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "CAD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 200m, true, 700m, 11.0m, "Northwest Territories", "NT", 15.0m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { new Guid("a0000000-0000-0000-0000-000000000012"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "CAD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 220m, true, 900m, 12.0m, "Nunavut", "NU", 15.0m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" },
                    { new Guid("a0000000-0000-0000-0000-000000000013"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "CAD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 190m, true, 650m, 10.5m, "Yukon", "YT", 15.0m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialCells_BusinessPlan_Sheet_Row_Column",
                table: "FinancialCells",
                columns: new[] { "BusinessPlanId", "SheetName", "RowId", "ColumnId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialCells_BusinessPlanId",
                table: "FinancialCells",
                column: "BusinessPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationOverheadRates_Province_IsActive",
                table: "LocationOverheadRates",
                columns: new[] { "Province", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationOverheadRates_ProvinceCode_IsActive",
                table: "LocationOverheadRates",
                columns: new[] { "ProvinceCode", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialCells");

            migrationBuilder.DropTable(
                name: "LocationOverheadRates");

            migrationBuilder.DropColumn(
                name: "Persona",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AllowDownload",
                table: "BusinessPlanShares");

            migrationBuilder.DropColumn(
                name: "EnableWatermark",
                table: "BusinessPlanShares");

            migrationBuilder.DropColumn(
                name: "IsVaultShare",
                table: "BusinessPlanShares");

            migrationBuilder.DropColumn(
                name: "MaxViews",
                table: "BusinessPlanShares");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "BusinessPlanShares");

            migrationBuilder.DropColumn(
                name: "RequireEmailVerification",
                table: "BusinessPlanShares");

            migrationBuilder.DropColumn(
                name: "TrackViews",
                table: "BusinessPlanShares");

            migrationBuilder.DropColumn(
                name: "WatermarkText",
                table: "BusinessPlanShares");
        }
    }
}
