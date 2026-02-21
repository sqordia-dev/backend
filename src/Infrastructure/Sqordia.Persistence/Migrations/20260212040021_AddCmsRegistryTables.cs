using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCmsRegistryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CmsPages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IconName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SpecialRenderer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_CmsPages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CmsSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CmsPageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IconName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_CmsSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsSections_CmsPages_CmsPageId",
                        column: x => x.CmsPageId,
                        principalTable: "CmsPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CmsBlockDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CmsSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BlockType = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DefaultContent = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ValidationRules = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    MetadataSchema = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Placeholder = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MaxLength = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_CmsBlockDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsBlockDefinitions_CmsSections_CmsSectionId",
                        column: x => x.CmsSectionId,
                        principalTable: "CmsSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CmsBlockDefinitions_BlockType",
                table: "CmsBlockDefinitions",
                column: "BlockType");

            migrationBuilder.CreateIndex(
                name: "IX_CmsBlockDefinitions_CmsSectionId_BlockKey",
                table: "CmsBlockDefinitions",
                columns: new[] { "CmsSectionId", "BlockKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsBlockDefinitions_CmsSectionId_SortOrder",
                table: "CmsBlockDefinitions",
                columns: new[] { "CmsSectionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CmsBlockDefinitions_IsActive",
                table: "CmsBlockDefinitions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CmsPages_IsActive",
                table: "CmsPages",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CmsPages_Key",
                table: "CmsPages",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsPages_SortOrder",
                table: "CmsPages",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_CmsSections_CmsPageId_SortOrder",
                table: "CmsSections",
                columns: new[] { "CmsPageId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CmsSections_IsActive",
                table: "CmsSections",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CmsSections_Key",
                table: "CmsSections",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CmsBlockDefinitions");

            migrationBuilder.DropTable(
                name: "CmsSections");

            migrationBuilder.DropTable(
                name: "CmsPages");
        }
    }
}
