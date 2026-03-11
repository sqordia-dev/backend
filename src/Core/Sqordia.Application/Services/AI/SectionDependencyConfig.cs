using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.AI;

/// <summary>
/// Data-driven section dependency configuration.
/// Replaces hardcoded tier arrays and cross-reference switch expressions.
/// </summary>
public static class SectionDependencyConfig
{
    /// <summary>
    /// Section dependency tiers for standard business plans.
    /// Sections within a tier can be generated in parallel.
    /// Later tiers depend on earlier tiers being complete.
    /// </summary>
    private static readonly string[][] StandardTiers =
    {
        // Tier 1: Foundation (no dependencies)
        new[] { "MarketAnalysis", "ManagementTeam", "OperationsPlan" },
        // Tier 2: Build on market + team
        new[] { "ProblemStatement", "Solution", "CompetitiveAnalysis", "SwotAnalysis", "BusinessModel" },
        // Tier 3: Build on competitive + business model
        new[] { "MarketingStrategy", "BrandingStrategy", "FinancialProjections" },
        // Tier 4: Build on everything above
        new[] { "RiskAnalysis", "FundingRequirements", "ExitStrategy" },
        // Tier 5: Synthesis (must come last)
        new[] { "ExecutiveSummary" }
    };

    /// <summary>
    /// OBNL-specific tiers with non-profit sections.
    /// </summary>
    private static readonly string[][] ObnlTiers =
    {
        new[] { "MarketAnalysis", "ManagementTeam", "OperationsPlan", "MissionStatement" },
        new[] { "ProblemStatement", "Solution", "CompetitiveAnalysis", "SwotAnalysis", "BusinessModel", "SocialImpact", "BeneficiaryProfile" },
        new[] { "MarketingStrategy", "BrandingStrategy", "FinancialProjections" },
        new[] { "RiskAnalysis", "FundingRequirements", "GrantStrategy", "SustainabilityPlan" },
        new[] { "ExecutiveSummary" }
    };

    /// <summary>
    /// Cross-reference map: which previously-generated sections each section should reference.
    /// </summary>
    private static readonly Dictionary<string, string[]> CrossReferences = new()
    {
        ["CompetitiveAnalysis"] = new[] { "MarketAnalysis" },
        ["SwotAnalysis"] = new[] { "MarketAnalysis", "CompetitiveAnalysis" },
        ["BusinessModel"] = new[] { "MarketAnalysis", "CompetitiveAnalysis" },
        ["MarketingStrategy"] = new[] { "MarketAnalysis", "CompetitiveAnalysis", "BusinessModel" },
        ["BrandingStrategy"] = new[] { "MarketingStrategy" },
        ["FinancialProjections"] = new[] { "BusinessModel", "OperationsPlan", "ManagementTeam" },
        ["FundingRequirements"] = new[] { "FinancialProjections" },
        ["RiskAnalysis"] = new[] { "SwotAnalysis", "FinancialProjections" },
        ["ExitStrategy"] = new[] { "FinancialProjections", "BusinessModel" },
        ["GrantStrategy"] = new[] { "FinancialProjections", "SocialImpact" },
        ["SustainabilityPlan"] = new[] { "FinancialProjections", "GrantStrategy" },
    };

    /// <summary>
    /// Gets the generation tiers for a plan type.
    /// </summary>
    public static string[][] GetTiersForPlanType(BusinessPlanType planType)
    {
        return planType == BusinessPlanType.StrategicPlan ? ObnlTiers : StandardTiers;
    }

    /// <summary>
    /// Gets the cross-reference section names for a given section.
    /// For ExecutiveSummary, returns all generated sections except itself.
    /// </summary>
    public static string[] GetCrossReferences(string sectionName, IEnumerable<string> generatedSectionNames)
    {
        if (sectionName == "ExecutiveSummary")
            return generatedSectionNames.Where(k => k != "ExecutiveSummary").ToArray();

        return CrossReferences.GetValueOrDefault(sectionName, Array.Empty<string>());
    }

    /// <summary>
    /// Gets all available sections for a plan type string.
    /// </summary>
    public static List<string> GetAvailableSections(string planType)
    {
        var commonSections = new List<string>
        {
            "ExecutiveSummary", "ProblemStatement", "Solution",
            "MarketAnalysis", "CompetitiveAnalysis", "SwotAnalysis",
            "BusinessModel", "MarketingStrategy", "BrandingStrategy",
            "OperationsPlan", "ManagementTeam", "FinancialProjections",
            "FundingRequirements", "RiskAnalysis"
        };

        if (planType is "StrategicPlan" or "2")
        {
            commonSections.AddRange(new[] { "MissionStatement", "SocialImpact", "BeneficiaryProfile", "GrantStrategy", "SustainabilityPlan" });
        }
        else if (planType is "BusinessPlan" or "0")
        {
            commonSections.Add("ExitStrategy");
        }

        return commonSections;
    }
}
