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
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"ContentPages\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TemplateCustomizations\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TemplateFields\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TemplateRatings\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TemplateUsages\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"TemplateSections\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Templates\";");
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
