using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCmsVersioningSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Templates",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            // PostgreSQL cannot implicitly cast integer to varchar — use CASE to map enum values
            migrationBuilder.Sql(@"
                ALTER TABLE ""Templates""
                ALTER COLUMN ""Type"" TYPE character varying(50)
                USING CASE ""Type""
                    WHEN 1 THEN 'Standard'
                    WHEN 2 THEN 'Premium'
                    WHEN 3 THEN 'Custom'
                    WHEN 4 THEN 'IndustrySpecific'
                    WHEN 5 THEN 'Regional'
                    WHEN 6 THEN 'LanguageSpecific'
                    WHEN 7 THEN 'SizeSpecific'
                    WHEN 8 THEN 'SectorSpecific'
                    WHEN 9 THEN 'ComplianceSpecific'
                    WHEN 10 THEN 'FundingSpecific'
                    WHEN 11 THEN 'Other'
                    ELSE 'Standard'
                END;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "Templates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Templates""
                ALTER COLUMN ""Status"" TYPE character varying(50)
                USING CASE ""Status""
                    WHEN 1 THEN 'Draft'
                    WHEN 2 THEN 'Review'
                    WHEN 3 THEN 'Approved'
                    WHEN 4 THEN 'Published'
                    WHEN 5 THEN 'Archived'
                    WHEN 6 THEN 'Deprecated'
                    WHEN 7 THEN 'UnderMaintenance'
                    WHEN 8 THEN 'PendingApproval'
                    WHEN 9 THEN 'Rejected'
                    WHEN 10 THEN 'Other'
                    ELSE 'Draft'
                END;
                ALTER TABLE ""Templates"" ALTER COLUMN ""Status"" SET DEFAULT 'Draft';
                ALTER TABLE ""Templates"" ALTER COLUMN ""Status"" SET NOT NULL;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "PreviewImage",
                table: "Templates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Templates",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Templates",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Templates""
                ALTER COLUMN ""Category"" TYPE character varying(50)
                USING CASE ""Category""
                    WHEN 1 THEN 'BusinessPlan'
                    WHEN 2 THEN 'FinancialProjection'
                    WHEN 3 THEN 'MarketingPlan'
                    WHEN 4 THEN 'OperationsPlan'
                    WHEN 5 THEN 'RiskAssessment'
                    WHEN 6 THEN 'ExecutiveSummary'
                    WHEN 7 THEN 'CompanyProfile'
                    WHEN 8 THEN 'MarketAnalysis'
                    WHEN 9 THEN 'CompetitiveAnalysis'
                    WHEN 10 THEN 'SalesPlan'
                    WHEN 11 THEN 'HRPlan'
                    WHEN 12 THEN 'TechnologyPlan'
                    WHEN 13 THEN 'SustainabilityPlan'
                    WHEN 14 THEN 'ExitStrategy'
                    WHEN 15 THEN 'LegalCompliance'
                    WHEN 16 THEN 'Other'
                    ELSE 'BusinessPlan'
                END;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "CellType",
                table: "FinancialCells",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Number",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "number");

            migrationBuilder.CreateTable(
                name: "CmsAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_CmsAssets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CmsVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_CmsVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CmsContentBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CmsVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BlockType = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    SectionKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Metadata = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_CmsContentBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsContentBlocks_CmsVersions_CmsVersionId",
                        column: x => x.CmsVersionId,
                        principalTable: "CmsVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Author",
                table: "Templates",
                column: "Author");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Category",
                table: "Templates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Category_Status",
                table: "Templates",
                columns: new[] { "Category", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Templates_IsPublic",
                table: "Templates",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Name",
                table: "Templates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Status",
                table: "Templates",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CmsAssets_Category",
                table: "CmsAssets",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_CmsAssets_UploadedByUserId",
                table: "CmsAssets",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CmsContentBlocks_CmsVersionId_BlockKey_Language",
                table: "CmsContentBlocks",
                columns: new[] { "CmsVersionId", "BlockKey", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsContentBlocks_SectionKey",
                table: "CmsContentBlocks",
                column: "SectionKey");

            migrationBuilder.CreateIndex(
                name: "IX_CmsVersions_Status",
                table: "CmsVersions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CmsAssets");

            migrationBuilder.DropTable(
                name: "CmsContentBlocks");

            migrationBuilder.DropTable(
                name: "CmsVersions");

            migrationBuilder.DropIndex(
                name: "IX_Templates_Author",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_Category",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_Category_Status",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_IsPublic",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_Name",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_Status",
                table: "Templates");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Templates",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            // Reverse: varchar back to integer with CASE mapping
            migrationBuilder.Sql(@"
                ALTER TABLE ""Templates""
                ALTER COLUMN ""Type"" TYPE integer
                USING CASE ""Type""
                    WHEN 'Standard' THEN 1
                    WHEN 'Premium' THEN 2
                    WHEN 'Custom' THEN 3
                    WHEN 'IndustrySpecific' THEN 4
                    WHEN 'Regional' THEN 5
                    WHEN 'LanguageSpecific' THEN 6
                    WHEN 'SizeSpecific' THEN 7
                    WHEN 'SectorSpecific' THEN 8
                    WHEN 'ComplianceSpecific' THEN 9
                    WHEN 'FundingSpecific' THEN 10
                    WHEN 'Other' THEN 11
                    ELSE 1
                END;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "Templates",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.Sql(@"
                ALTER TABLE ""Templates"" ALTER COLUMN ""Status"" DROP DEFAULT;
                ALTER TABLE ""Templates""
                ALTER COLUMN ""Status"" TYPE integer
                USING CASE ""Status""
                    WHEN 'Draft' THEN 1
                    WHEN 'Review' THEN 2
                    WHEN 'Approved' THEN 3
                    WHEN 'Published' THEN 4
                    WHEN 'Archived' THEN 5
                    WHEN 'Deprecated' THEN 6
                    WHEN 'UnderMaintenance' THEN 7
                    WHEN 'PendingApproval' THEN 8
                    WHEN 'Rejected' THEN 9
                    WHEN 'Other' THEN 10
                    ELSE 1
                END;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "PreviewImage",
                table: "Templates",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Templates",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Templates",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.Sql(@"
                ALTER TABLE ""Templates""
                ALTER COLUMN ""Category"" TYPE integer
                USING CASE ""Category""
                    WHEN 'BusinessPlan' THEN 1
                    WHEN 'FinancialProjection' THEN 2
                    WHEN 'MarketingPlan' THEN 3
                    WHEN 'OperationsPlan' THEN 4
                    WHEN 'RiskAssessment' THEN 5
                    WHEN 'ExecutiveSummary' THEN 6
                    WHEN 'CompanyProfile' THEN 7
                    WHEN 'MarketAnalysis' THEN 8
                    WHEN 'CompetitiveAnalysis' THEN 9
                    WHEN 'SalesPlan' THEN 10
                    WHEN 'HRPlan' THEN 11
                    WHEN 'TechnologyPlan' THEN 12
                    WHEN 'SustainabilityPlan' THEN 13
                    WHEN 'ExitStrategy' THEN 14
                    WHEN 'LegalCompliance' THEN 15
                    WHEN 'Other' THEN 16
                    ELSE 1
                END;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "CellType",
                table: "FinancialCells",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "number",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Number");
        }
    }
}
