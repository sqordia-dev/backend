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
            // These columns may already exist from previous migrations (AddGrowthArchitectV2Features, AddVaultShareFeatures)
            migrationBuilder.Sql(@"
                ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""Persona"" character varying(50);
                ALTER TABLE ""BusinessPlanShares"" ADD COLUMN IF NOT EXISTS ""AllowDownload"" boolean NOT NULL DEFAULT false;
                ALTER TABLE ""BusinessPlanShares"" ADD COLUMN IF NOT EXISTS ""EnableWatermark"" boolean NOT NULL DEFAULT false;
                ALTER TABLE ""BusinessPlanShares"" ADD COLUMN IF NOT EXISTS ""IsVaultShare"" boolean NOT NULL DEFAULT false;
                ALTER TABLE ""BusinessPlanShares"" ADD COLUMN IF NOT EXISTS ""MaxViews"" integer;
                ALTER TABLE ""BusinessPlanShares"" ADD COLUMN IF NOT EXISTS ""PasswordHash"" character varying(256);
                ALTER TABLE ""BusinessPlanShares"" ADD COLUMN IF NOT EXISTS ""RequireEmailVerification"" boolean NOT NULL DEFAULT false;
                ALTER TABLE ""BusinessPlanShares"" ADD COLUMN IF NOT EXISTS ""TrackViews"" boolean NOT NULL DEFAULT true;
                ALTER TABLE ""BusinessPlanShares"" ADD COLUMN IF NOT EXISTS ""WatermarkText"" character varying(500);
            ");

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

            migrationBuilder.Sql(@"
                INSERT INTO ""LocationOverheadRates"" (""Id"", ""CreatedAt"", ""CreatedBy"", ""Currency"", ""EffectiveDate"", ""ExpiryDate"", ""InsuranceRate"", ""IsActive"", ""OfficeCost"", ""OverheadRate"", ""Province"", ""ProvinceCode"", ""TaxRate"", ""UpdatedAt"", ""UpdatedBy"")
                VALUES
                ('a0000000-0000-0000-0000-000000000001', '2024-01-01T00:00:00Z', 'System', 'CAD', '2024-01-01T00:00:00Z', NULL, 200, true, 600, 10.0, 'Alberta', 'AB', 15.0, '2024-01-01T00:00:00Z', 'System'),
                ('a0000000-0000-0000-0000-000000000002', '2024-01-01T00:00:00Z', 'System', 'CAD', '2024-01-01T00:00:00Z', NULL, 250, true, 800, 12.0, 'British Columbia', 'BC', 20.0, '2024-01-01T00:00:00Z', 'System'),
                ('a0000000-0000-0000-0000-000000000003', '2024-01-01T00:00:00Z', 'System', 'CAD', '2024-01-01T00:00:00Z', NULL, 180, true, 450, 10.0, 'Manitoba', 'MB', 17.0, '2024-01-01T00:00:00Z', 'System'),
                ('a0000000-0000-0000-0000-000000000004', '2024-01-01T00:00:00Z', 'System', 'CAD', '2024-01-01T00:00:00Z', NULL, 170, true, 400, 9.0, 'New Brunswick', 'NB', 20.0, '2024-01-01T00:00:00Z', 'System'),
                ('a0000000-0000-0000-0000-000000000005', '2024-01-01T00:00:00Z', 'System', 'CAD', '2024-01-01T00:00:00Z', NULL, 175, true, 420, 9.0, 'Newfoundland and Labrador', 'NL', 20.0, '2024-01-01T00:00:00Z', 'System'),
                ('a0000000-0000-0000-0000-000000000006', '2024-01-01T00:00:00Z', 'System', 'CAD', '2024-01-01T00:00:00Z', NULL, 175, true, 450, 9.5, 'Nova Scotia', 'NS', 21.0, '2024-01-01T00:00:00Z', 'System'),
                ('a0000000-0000-0000-0000-000000000007', '2024-01-01T00:00:00Z', 'System', 'CAD', '2024-01-01T00:00:00Z', NULL, 250, true, 750, 12.0, 'Ontario', 'ON', 20.0, '2024-01-01T00:00:00Z', 'System'),
                ('a0000000-0000-0000-0000-000000000008', '2024-01-01T00:00:00Z', 'System', 'CAD', '2024-01-01T00:00:00Z', NULL, 160, true, 380, 8.5, 'Prince Edward Island', 'PE', 20.0, '2024-01-01T00:00:00Z', 'System'),
                ('a0000000-0000-0000-0000-000000000009', '2024-01-01T00:00:00Z', 'System', 'CAD', '2024-01-01T00:00:00Z', NULL, 220, true, 600, 11.0, 'Quebec', 'QC', 24.0, '2024-01-01T00:00:00Z', 'System'),
                ('a0000000-0000-0000-0000-000000000010', '2024-01-01T00:00:00Z', 'System', 'CAD', '2024-01-01T00:00:00Z', NULL, 180, true, 450, 9.5, 'Saskatchewan', 'SK', 16.0, '2024-01-01T00:00:00Z', 'System'),
                ('a0000000-0000-0000-0000-000000000011', '2024-01-01T00:00:00Z', 'System', 'CAD', '2024-01-01T00:00:00Z', NULL, 200, true, 700, 11.0, 'Northwest Territories', 'NT', 15.0, '2024-01-01T00:00:00Z', 'System'),
                ('a0000000-0000-0000-0000-000000000012', '2024-01-01T00:00:00Z', 'System', 'CAD', '2024-01-01T00:00:00Z', NULL, 220, true, 900, 12.0, 'Nunavut', 'NU', 15.0, '2024-01-01T00:00:00Z', 'System'),
                ('a0000000-0000-0000-0000-000000000013', '2024-01-01T00:00:00Z', 'System', 'CAD', '2024-01-01T00:00:00Z', NULL, 190, true, 650, 10.5, 'Yukon', 'YT', 15.0, '2024-01-01T00:00:00Z', 'System')
                ON CONFLICT DO NOTHING;
            ");

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
