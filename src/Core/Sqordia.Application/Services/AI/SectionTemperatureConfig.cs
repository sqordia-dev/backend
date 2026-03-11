namespace Sqordia.Application.Services.AI;

/// <summary>
/// Maps section names to recommended AI temperature settings.
/// Lower temperature = more precise/consistent; higher = more creative.
/// </summary>
public static class SectionTemperatureConfig
{
    private static readonly Dictionary<string, float> Temperatures = new()
    {
        // Low temp (0.3-0.4): Precision-critical sections
        ["FinancialProjections"] = 0.3f,
        ["FundingRequirements"] = 0.3f,
        ["RiskAnalysis"] = 0.4f,
        ["GrantStrategy"] = 0.4f,

        // Medium temp (0.5): Analytical sections
        ["MarketAnalysis"] = 0.5f,
        ["CompetitiveAnalysis"] = 0.5f,
        ["SwotAnalysis"] = 0.5f,
        ["OperationsPlan"] = 0.5f,
        ["ManagementTeam"] = 0.5f,
        ["BusinessModel"] = 0.5f,
        ["BeneficiaryProfile"] = 0.5f,
        ["SustainabilityPlan"] = 0.5f,

        // Higher temp (0.6-0.7): Creative sections
        ["ExecutiveSummary"] = 0.6f,
        ["ProblemStatement"] = 0.6f,
        ["Solution"] = 0.6f,
        ["MarketingStrategy"] = 0.65f,
        ["BrandingStrategy"] = 0.7f,
        ["ExitStrategy"] = 0.6f,
        ["MissionStatement"] = 0.6f,
        ["SocialImpact"] = 0.65f,
    };

    private const float DefaultTemperature = 0.6f;

    /// <summary>
    /// Gets the recommended temperature for a section.
    /// If a prompt template specifies a temperature override, that takes priority.
    /// </summary>
    public static float GetTemperature(string sectionName, float? promptTemplateOverride = null)
    {
        if (promptTemplateOverride.HasValue)
            return promptTemplateOverride.Value;

        return Temperatures.GetValueOrDefault(sectionName, DefaultTemperature);
    }
}
