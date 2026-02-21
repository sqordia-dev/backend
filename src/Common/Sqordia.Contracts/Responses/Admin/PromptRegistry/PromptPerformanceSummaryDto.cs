namespace Sqordia.Contracts.Responses.Admin.PromptRegistry;

/// <summary>
/// Dashboard overview of prompt performance across all prompts
/// </summary>
public class PromptPerformanceSummaryDto
{
    // Overall statistics
    public int TotalPrompts { get; set; }
    public int ActivePrompts { get; set; }
    public int TotalUsage { get; set; }
    public double OverallAverageRating { get; set; }
    public double OverallAcceptanceRate { get; set; }
    public double OverallEditRate { get; set; }
    public double OverallRegenerateRate { get; set; }

    // Top performers
    public List<TopPerformerDto> TopPerformingPrompts { get; set; } = new();
    public List<TopPerformerDto> MostUsedPrompts { get; set; } = new();
    public List<TopPerformerDto> HighestRatedPrompts { get; set; } = new();

    // Usage trends (last 30 days, daily)
    public List<UsageTrendDto> UsageTrends { get; set; } = new();

    // By section breakdown
    public List<SectionPerformanceDto> PerformanceBySection { get; set; } = new();
}

/// <summary>
/// Top performing prompt summary
/// </summary>
public class TopPerformerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SectionTypeName { get; set; } = string.Empty;
    public string PlanTypeName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public double AverageRating { get; set; }
    public double AcceptanceRate { get; set; }
}

/// <summary>
/// Daily usage trend data point
/// </summary>
public class UsageTrendDto
{
    public DateTime Date { get; set; }
    public int UsageCount { get; set; }
    public int AcceptCount { get; set; }
    public int EditCount { get; set; }
    public int RegenerateCount { get; set; }
}

/// <summary>
/// Performance breakdown by section type
/// </summary>
public class SectionPerformanceDto
{
    public string SectionType { get; set; } = string.Empty;
    public string SectionTypeName { get; set; } = string.Empty;
    public int PromptCount { get; set; }
    public int UsageCount { get; set; }
    public double AverageRating { get; set; }
    public double AcceptanceRate { get; set; }
}
