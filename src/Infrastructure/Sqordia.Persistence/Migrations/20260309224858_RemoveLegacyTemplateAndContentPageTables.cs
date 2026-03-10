using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyTemplateAndContentPageTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentPages");

            migrationBuilder.DropTable(
                name: "TemplateCustomizations");

            migrationBuilder.DropTable(
                name: "TemplateFields");

            migrationBuilder.DropTable(
                name: "TemplateRatings");

            migrationBuilder.DropTable(
                name: "TemplateUsages");

            migrationBuilder.DropTable(
                name: "TemplateSections");

            migrationBuilder.DropTable(
                name: "Templates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // These tables were legacy/unused and have been intentionally removed.
            // Re-creating them would require the full schema from the initial migration.
            // If needed, restore from: 20251231183954_InitialCreate (Templates)
            //                          20260104153401_AddSmartObjectivesPlanCommentsAndContentPages (ContentPages)
        }
    }
}
