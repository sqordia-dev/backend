using Sqordia.Application.Common.Interfaces;

namespace Sqordia.Infrastructure.Services.QualityAgents;

/// <summary>
/// Interface for a specialized quality analysis agent.
/// Each agent focuses on a specific aspect of business plan quality.
/// </summary>
public interface IQualityAgent
{
    string AgentName { get; }
    Task<AgentAnalysisResult> AnalyzeAsync(BusinessPlanContext context, CancellationToken ct);
}

/// <summary>
/// Context passed to each quality agent containing the full plan content.
/// </summary>
public class BusinessPlanContext
{
    public Guid BusinessPlanId { get; set; }
    public string PlanType { get; set; } = string.Empty;
    public string? BusinessBriefJson { get; set; }
    public string Language { get; set; } = "fr";
    public Dictionary<string, string?> Sections { get; set; } = new();
}

/// <summary>
/// Result from a single quality agent analysis.
/// </summary>
public class AgentAnalysisResult
{
    public string AgentName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public List<AgentFinding> Findings { get; set; } = new();
}
