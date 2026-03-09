using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations;

/// <summary>
/// Data migration: backfills Organization business context fields from BusinessPlan.OnboardingContextJson
/// for existing users who completed onboarding before V2.
/// Only updates Organizations that have no business context fields set yet.
/// </summary>
public partial class BackfillOrganizationProfileFromOnboarding : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Backfill Organization fields from BusinessPlan.OnboardingContextJson
        // Only updates organizations where ALL profile fields are currently null (not yet populated by V2)
        // Uses PostgreSQL JSON functions to parse the onboarding context
        // OnboardingContextJson is a jsonb column — use jsonb operators
        migrationBuilder.Sql(@"
            UPDATE ""Organizations"" org
            SET
                ""Industry"" = COALESCE(org.""Industry"", sub.""Industry""),
                ""TeamSize"" = COALESCE(org.""TeamSize"", sub.""TeamSize""),
                ""FundingStatus"" = COALESCE(org.""FundingStatus"", sub.""FundingStatus""),
                ""TargetMarket"" = COALESCE(org.""TargetMarket"", sub.""TargetMarket""),
                ""BusinessStage"" = COALESCE(org.""BusinessStage"", sub.""BusinessStage""),
                ""GoalsJson"" = COALESCE(org.""GoalsJson"", sub.""GoalsJson""),
                ""LastModified"" = NOW()
            FROM (
                SELECT DISTINCT ON (bp.""OrganizationId"")
                    bp.""OrganizationId"",
                    NULLIF(TRIM(bp.""OnboardingContextJson""->>'industry'), '') AS ""Industry"",
                    NULLIF(TRIM(bp.""OnboardingContextJson""->>'teamSize'), '') AS ""TeamSize"",
                    NULLIF(TRIM(bp.""OnboardingContextJson""->>'fundingStatus'), '') AS ""FundingStatus"",
                    NULLIF(TRIM(bp.""OnboardingContextJson""->>'targetMarket'), '') AS ""TargetMarket"",
                    NULLIF(TRIM(bp.""OnboardingContextJson""->>'businessStage'), '') AS ""BusinessStage"",
                    CASE
                        WHEN bp.""OnboardingContextJson""->>'goals' IS NOT NULL
                             AND bp.""OnboardingContextJson""->>'goals' != 'null'
                             AND bp.""OnboardingContextJson""->>'goals' != '[]'
                        THEN bp.""OnboardingContextJson""->>'goals'
                        ELSE NULL
                    END AS ""GoalsJson""
                FROM ""BusinessPlans"" bp
                WHERE bp.""OnboardingContextJson"" IS NOT NULL
                  AND bp.""OnboardingContextJson"" != 'null'::jsonb
                  AND bp.""IsDeleted"" = false
                ORDER BY bp.""OrganizationId"", bp.""Created"" DESC
            ) sub
            WHERE org.""Id"" = sub.""OrganizationId""
              AND org.""IsDeleted"" = false
              AND org.""Industry"" IS NULL
              AND org.""TeamSize"" IS NULL
              AND org.""FundingStatus"" IS NULL
              AND org.""TargetMarket"" IS NULL
              AND org.""BusinessStage"" IS NULL;
        ");

        // Recalculate ProfileCompletenessScore for updated organizations
        migrationBuilder.Sql(@"
            UPDATE ""Organizations""
            SET ""ProfileCompletenessScore"" = (
                (CASE WHEN ""Name"" IS NOT NULL AND ""Name"" != '' THEN 1 ELSE 0 END) +
                (CASE WHEN ""Industry"" IS NOT NULL AND ""Industry"" != '' THEN 1 ELSE 0 END) +
                (CASE WHEN ""Sector"" IS NOT NULL AND ""Sector"" != '' THEN 1 ELSE 0 END) +
                (CASE WHEN ""TeamSize"" IS NOT NULL AND ""TeamSize"" != '' THEN 1 ELSE 0 END) +
                (CASE WHEN ""FundingStatus"" IS NOT NULL AND ""FundingStatus"" != '' THEN 1 ELSE 0 END) +
                (CASE WHEN ""TargetMarket"" IS NOT NULL AND ""TargetMarket"" != '' THEN 1 ELSE 0 END) +
                (CASE WHEN ""BusinessStage"" IS NOT NULL AND ""BusinessStage"" != '' THEN 1 ELSE 0 END) +
                (CASE WHEN ""GoalsJson"" IS NOT NULL AND ""GoalsJson"" != '' AND ""GoalsJson"" != '[]' THEN 1 ELSE 0 END) +
                (CASE WHEN ""City"" IS NOT NULL AND ""City"" != '' THEN 1 ELSE 0 END) +
                (CASE WHEN ""Province"" IS NOT NULL AND ""Province"" != '' THEN 1 ELSE 0 END) +
                (CASE WHEN ""Country"" IS NOT NULL AND ""Country"" != '' THEN 1 ELSE 0 END)
            ) * 100 / 11
            WHERE ""IsDeleted"" = false
              AND (""Industry"" IS NOT NULL OR ""TeamSize"" IS NOT NULL OR ""FundingStatus"" IS NOT NULL
                   OR ""TargetMarket"" IS NOT NULL OR ""BusinessStage"" IS NOT NULL);
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Data migration — no automatic rollback.
        // The original OnboardingContextJson data is preserved on BusinessPlan entities.
    }
}
