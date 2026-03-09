using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Orchestrates multiple specialized quality agents that analyze a generated business plan
/// for writing quality, financial consistency, compliance, and bank-readiness.
/// Agents run in parallel and their findings are aggregated.
/// </summary>
public interface IQualityAgentOrchestrator
{
    /// <summary>
    /// Runs all quality agents against a business plan and returns aggregated findings.
    /// </summary>
    Task<Result<QualityAgentReport>> RunQualityAgentsAsync(
        Guid businessPlanId,
        string language = "fr",
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Aggregated report from all quality agents.
/// </summary>
public class QualityAgentReport
{
    public Guid BusinessPlanId { get; set; }
    public decimal OverallScore { get; set; }
    public decimal WritingScore { get; set; }
    public decimal FinancialScore { get; set; }
    public decimal ComplianceScore { get; set; }
    public decimal BankReadinessScore { get; set; }
    public List<AgentFinding> Findings { get; set; } = new();
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A finding from a quality agent.
/// </summary>
public class AgentFinding
{
    public string AgentName { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public FindingSeverity Severity { get; set; }
    public string Finding { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public bool AutoFixable { get; set; }
}

public enum FindingSeverity
{
    Low,
    Medium,
    High,
    Critical
}
