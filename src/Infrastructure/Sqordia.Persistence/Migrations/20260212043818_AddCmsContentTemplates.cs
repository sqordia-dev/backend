using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCmsContentTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CmsContentTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PageKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SectionKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TemplateData = table.Column<string>(type: "jsonb", nullable: false),
                    PreviewImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_CmsContentTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CmsContentTemplates_CreatedByUserId",
                table: "CmsContentTemplates",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CmsContentTemplates_IsPublic",
                table: "CmsContentTemplates",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_CmsContentTemplates_PageKey",
                table: "CmsContentTemplates",
                column: "PageKey");

            migrationBuilder.CreateIndex(
                name: "IX_CmsContentTemplates_PageKey_SectionKey",
                table: "CmsContentTemplates",
                columns: new[] { "PageKey", "SectionKey" });

            migrationBuilder.CreateIndex(
                name: "IX_CmsContentTemplates_SectionKey",
                table: "CmsContentTemplates",
                column: "SectionKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CmsContentTemplates");
        }
    }
}
