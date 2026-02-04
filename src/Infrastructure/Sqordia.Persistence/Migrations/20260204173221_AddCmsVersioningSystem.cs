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

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Templates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "Templates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Templates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Draft",
                oldClrType: typeof(int),
                oldType: "integer");

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

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Templates",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

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

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Templates",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "Templates",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Templates",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Draft");

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

            migrationBuilder.AlterColumn<int>(
                name: "Category",
                table: "Templates",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

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
