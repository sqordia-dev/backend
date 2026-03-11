namespace Sqordia.Application.Common.Constants;

/// <summary>
/// Canonical section name constants used across the application.
/// Eliminates magic strings for section identification.
/// </summary>
public static class SectionNames
{
    // Standard business plan sections (kebab-case for API, PascalCase for entity properties)
    public const string ExecutiveSummary = "executive-summary";
    public const string ProblemStatement = "problem-statement";
    public const string Solution = "solution";
    public const string MarketAnalysis = "market-analysis";
    public const string CompetitiveAnalysis = "competitive-analysis";
    public const string SwotAnalysis = "swot-analysis";
    public const string BusinessModel = "business-model";
    public const string MarketingStrategy = "marketing-strategy";
    public const string BrandingStrategy = "branding-strategy";
    public const string OperationsPlan = "operations-plan";
    public const string ManagementTeam = "management-team";
    public const string FinancialProjections = "financial-projections";
    public const string FundingRequirements = "funding-requirements";
    public const string RiskAnalysis = "risk-analysis";
    public const string ExitStrategy = "exit-strategy";
    public const string AppendixData = "appendix-data";

    // OBNL (non-profit) sections
    public const string MissionStatement = "mission-statement";
    public const string SocialImpact = "social-impact";
    public const string BeneficiaryProfile = "beneficiary-profile";
    public const string GrantStrategy = "grant-strategy";
    public const string SustainabilityPlan = "sustainability-plan";

    /// <summary>
    /// All standard business plan section names.
    /// </summary>
    public static readonly IReadOnlyList<string> BusinessPlanSections = new[]
    {
        ExecutiveSummary, ProblemStatement, Solution, MarketAnalysis,
        CompetitiveAnalysis, SwotAnalysis, BusinessModel, MarketingStrategy,
        BrandingStrategy, OperationsPlan, ManagementTeam, FinancialProjections,
        FundingRequirements, RiskAnalysis, ExitStrategy, AppendixData
    };

    /// <summary>
    /// OBNL-specific section names.
    /// </summary>
    public static readonly IReadOnlyList<string> ObnlSections = new[]
    {
        MissionStatement, SocialImpact, BeneficiaryProfile,
        GrantStrategy, SustainabilityPlan
    };

    /// <summary>
    /// All section names (standard + OBNL).
    /// </summary>
    public static readonly IReadOnlyList<string> AllSections =
        BusinessPlanSections.Concat(ObnlSections).ToList();

    /// <summary>
    /// Converts kebab-case section name to PascalCase entity property name.
    /// e.g., "executive-summary" → "ExecutiveSummary"
    /// </summary>
    public static string ToPascalCase(string kebabName)
    {
        if (string.IsNullOrEmpty(kebabName) || !kebabName.Contains('-'))
            return kebabName;

        return string.Concat(
            kebabName.Split('-')
                .Select(part => string.IsNullOrEmpty(part)
                    ? part
                    : char.ToUpperInvariant(part[0]) + part[1..]));
    }

    /// <summary>
    /// Checks if a section name is valid (case-insensitive).
    /// </summary>
    public static bool IsValid(string sectionName)
        => AllSections.Contains(sectionName.ToLowerInvariant());
}
