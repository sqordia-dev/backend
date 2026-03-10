using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingDatabaseIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TableOfContentsSettings already created by AddTableOfContentsSettings migration.
            // Just add the missing columns (DeletedAt, DeletedBy) if they don't exist.
            migrationBuilder.Sql(@"
                ALTER TABLE ""TableOfContentsSettings"" ADD COLUMN IF NOT EXISTS ""DeletedAt"" timestamp with time zone;
                ALTER TABLE ""TableOfContentsSettings"" ADD COLUMN IF NOT EXISTS ""DeletedBy"" text;
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_Users_IsActive"" ON ""Users"" (""IsActive"");
                CREATE INDEX IF NOT EXISTS ""IX_Users_IsEmailConfirmed"" ON ""Users"" (""IsEmailConfirmed"");
                CREATE INDEX IF NOT EXISTS ""IX_Organizations_IsActive_CreatedBy"" ON ""Organizations"" (""IsActive"", ""CreatedBy"");
                CREATE INDEX IF NOT EXISTS ""IX_FinancialCells_IsCalculated"" ON ""FinancialCells"" (""IsCalculated"");
                CREATE INDEX IF NOT EXISTS ""IX_BusinessPlans_IsTemplate"" ON ""BusinessPlans"" (""IsTemplate"");
                CREATE INDEX IF NOT EXISTS ""IX_BusinessPlans_OrganizationId_Persona"" ON ""BusinessPlans"" (""OrganizationId"", ""Persona"");
                CREATE INDEX IF NOT EXISTS ""IX_BusinessPlans_Persona"" ON ""BusinessPlans"" (""Persona"");
                CREATE INDEX IF NOT EXISTS ""IX_BusinessPlans_ReadinessScore"" ON ""BusinessPlans"" (""ReadinessScore"");
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_TableOfContentsSettings_BusinessPlanId"" ON ""TableOfContentsSettings"" (""BusinessPlanId"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_Users_IsActive"";
                DROP INDEX IF EXISTS ""IX_Users_IsEmailConfirmed"";
                DROP INDEX IF EXISTS ""IX_Organizations_IsActive_CreatedBy"";
                DROP INDEX IF EXISTS ""IX_FinancialCells_IsCalculated"";
                DROP INDEX IF EXISTS ""IX_BusinessPlans_IsTemplate"";
                DROP INDEX IF EXISTS ""IX_BusinessPlans_OrganizationId_Persona"";
                DROP INDEX IF EXISTS ""IX_BusinessPlans_Persona"";
                DROP INDEX IF EXISTS ""IX_BusinessPlans_ReadinessScore"";
                ALTER TABLE ""TableOfContentsSettings"" DROP COLUMN IF EXISTS ""DeletedAt"";
                ALTER TABLE ""TableOfContentsSettings"" DROP COLUMN IF EXISTS ""DeletedBy"";
            ");
        }
    }
}
