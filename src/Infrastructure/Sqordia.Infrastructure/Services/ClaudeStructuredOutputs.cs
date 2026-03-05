namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Private DTO classes for JSON deserialization of Claude structured outputs.
/// These map to the JSON schemas specified in ClaudeService system prompts.
/// </summary>

// Question Suggestions
internal class QuestionSuggestionStructuredOutput
{
    public List<QuestionSuggestionItem>? Suggestions { get; set; }
}

internal class QuestionSuggestionItem
{
    public string? Answer { get; set; }
    public double Confidence { get; set; } = 0.8;
    public string? Reasoning { get; set; }
    public string? SuggestionType { get; set; }
}

// Section Improvement
internal class SectionImprovementStructuredOutput
{
    public string? ImprovedContent { get; set; }
    public string? Explanation { get; set; }
    public List<string>? FurtherSuggestions { get; set; }
    public string? ReadingLevel { get; set; }
}

// Section Expansion
internal class SectionExpansionStructuredOutput
{
    public string? ImprovedContent { get; set; }
    public string? Explanation { get; set; }
    public List<string>? AddedSubsections { get; set; }
    public List<string>? ExpandedPoints { get; set; }
}

// Section Simplification
internal class SectionSimplificationStructuredOutput
{
    public string? ImprovedContent { get; set; }
    public string? Explanation { get; set; }
    public List<string>? SimplifiedTerms { get; set; }
    public List<string>? RemovedJargon { get; set; }
    public double OriginalComplexity { get; set; } = 0.8;
    public double NewComplexity { get; set; } = 0.3;
}

// Strategy Suggestions
internal class StrategySuggestionsStructuredOutput
{
    public List<StrategySuggestionItem>? Suggestions { get; set; }
}

internal class StrategySuggestionItem
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Priority { get; set; }
    public string? ExpectedImpact { get; set; }
    public List<string>? ImplementationSteps { get; set; }
    public string? EstimatedTimeframe { get; set; }
    public string? Reasoning { get; set; }
}

// Risk Analysis
internal class RiskAnalysisStructuredOutput
{
    public List<RiskAnalysisItem>? Risks { get; set; }
    public string? OverallRiskLevel { get; set; }
}

internal class RiskAnalysisItem
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Likelihood { get; set; }
    public string? Impact { get; set; }
    public int RiskScore { get; set; } = 5;
    public List<MitigationStrategyItem>? MitigationStrategies { get; set; }
    public string? ContingencyPlan { get; set; }
}

internal class MitigationStrategyItem
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public string? Effectiveness { get; set; }
}

// Business Mentor Analysis
internal class BusinessMentorStructuredOutput
{
    public string? ExecutiveSummary { get; set; }
    public List<OpportunityItem>? Opportunities { get; set; }
    public List<WeaknessItem>? Weaknesses { get; set; }
    public List<RecommendationItem>? Recommendations { get; set; }
    public int HealthScore { get; set; } = 75;
    public List<string>? Strengths { get; set; }
    public List<string>? CriticalAreas { get; set; }
}

internal class OpportunityItem
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? PotentialImpact { get; set; }
    public List<string>? RecommendedActions { get; set; }
}

internal class WeaknessItem
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Severity { get; set; }
    public List<string>? RecommendedActions { get; set; }
    public string? ImpactIfNotAddressed { get; set; }
}

internal class RecommendationItem
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Priority { get; set; }
    public string? ExpectedBenefits { get; set; }
    public List<string>? ImplementationSteps { get; set; }
}
