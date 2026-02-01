namespace Sqordia.Contracts.Requests.Content;

/// <summary>
/// Request for generating enhanced section content with visual elements
/// </summary>
public class SectionGenerationRequest
{
    /// <summary>
    /// The business plan ID
    /// </summary>
    public required Guid BusinessPlanId { get; set; }

    /// <summary>
    /// The section type to generate (e.g., "ExecutiveSummary", "MarketAnalysis")
    /// </summary>
    public required string SectionType { get; set; }

    /// <summary>
    /// Options for content generation
    /// </summary>
    public GenerationOptionsDto Options { get; set; } = new();
}

/// <summary>
/// Options for content generation
/// </summary>
public class GenerationOptionsDto
{
    /// <summary>
    /// Language for content generation (default: "en")
    /// </summary>
    public string Language { get; set; } = "en";

    /// <summary>
    /// Whether to include visual elements (tables, charts, metrics)
    /// </summary>
    public bool IncludeVisualElements { get; set; } = true;

    /// <summary>
    /// Preferred prompt alias: production, staging, development, experimental
    /// </summary>
    public string? PreferredAlias { get; set; }

    /// <summary>
    /// Additional variables to pass to the prompt template
    /// </summary>
    public Dictionary<string, string> AdditionalVariables { get; set; } = new();

    /// <summary>
    /// Visual preferences: which types of visuals to include
    /// </summary>
    public List<string>? VisualPreferences { get; set; }
}

/// <summary>
/// Request for regenerating a section with enhanced content
/// </summary>
public class RegenerateSectionRequest
{
    /// <summary>
    /// Whether to include visual elements in regeneration
    /// </summary>
    public bool IncludeVisuals { get; set; } = true;

    /// <summary>
    /// Language for regeneration
    /// </summary>
    public string Language { get; set; } = "en";

    /// <summary>
    /// Optional feedback to incorporate in regeneration
    /// </summary>
    public string? Feedback { get; set; }

    /// <summary>
    /// Preferred prompt alias
    /// </summary>
    public string? PreferredAlias { get; set; }
}

/// <summary>
/// Request for improving an existing section
/// </summary>
public class ImproveSectionRequest
{
    /// <summary>
    /// The current content to improve
    /// </summary>
    public required string CurrentContent { get; set; }

    /// <summary>
    /// Type of improvement: enhance, expand, simplify, professionalize
    /// </summary>
    public required string ImprovementType { get; set; }

    /// <summary>
    /// Optional custom prompt for specific improvements
    /// </summary>
    public string? CustomPrompt { get; set; }

    /// <summary>
    /// Whether to include visual elements in the improved content
    /// </summary>
    public bool IncludeVisuals { get; set; } = true;

    /// <summary>
    /// Language for improvement
    /// </summary>
    public string Language { get; set; } = "en";
}

/// <summary>
/// Context data for plan and industry
/// </summary>
public class PlanContextDto
{
    /// <summary>
    /// Company name
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Industry category (NAICS code or description)
    /// </summary>
    public string? Industry { get; set; }

    /// <summary>
    /// Company/project description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Target market description
    /// </summary>
    public string? TargetMarket { get; set; }

    /// <summary>
    /// Products/services offered
    /// </summary>
    public string? Products { get; set; }

    /// <summary>
    /// Plan type: BusinessPlan, StrategicPlan, LeanCanvas
    /// </summary>
    public string? PlanType { get; set; }

    /// <summary>
    /// Questionnaire responses context
    /// </summary>
    public string? QuestionnaireContext { get; set; }
}

/// <summary>
/// Industry insights data for enhanced generation
/// </summary>
public class IndustryInsightsDto
{
    /// <summary>
    /// Total market size in USD
    /// </summary>
    public decimal? MarketSize { get; set; }

    /// <summary>
    /// Annual growth rate (as decimal, e.g., 0.125 for 12.5%)
    /// </summary>
    public decimal? GrowthRate { get; set; }

    /// <summary>
    /// Key industry trends
    /// </summary>
    public List<string> Trends { get; set; } = new();

    /// <summary>
    /// Major competitors in the industry
    /// </summary>
    public List<string> Competitors { get; set; } = new();

    /// <summary>
    /// Average industry margins
    /// </summary>
    public decimal? AverageMargin { get; set; }

    /// <summary>
    /// Key success factors
    /// </summary>
    public List<string> KeySuccessFactors { get; set; } = new();

    /// <summary>
    /// Data source for the insights
    /// </summary>
    public string? DataSource { get; set; }

    /// <summary>
    /// When the data was last updated
    /// </summary>
    public DateTime? LastUpdated { get; set; }
}
