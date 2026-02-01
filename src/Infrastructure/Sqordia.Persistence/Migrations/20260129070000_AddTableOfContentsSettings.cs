using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTableOfContentsSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TableOfContentsSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Style = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "classic"),
                    ShowPageNumbers = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ShowIcons = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ShowCategoryHeaders = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    StyleSettingsJson = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableOfContentsSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TableOfContentsSettings_BusinessPlans_BusinessPlanId",
                        column: x => x.BusinessPlanId,
                        principalTable: "BusinessPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TableOfContentsSettings_BusinessPlanId",
                table: "TableOfContentsSettings",
                column: "BusinessPlanId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TableOfContentsSettings");
        }
    }
}
