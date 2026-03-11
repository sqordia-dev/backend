using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Service for extracting structured data from business plan content using AI.
/// Uses Claude tool_use for reliable JSON extraction (SWOT, financials, risks, etc.)
/// instead of fragile prompt-based JSON parsing.
/// </summary>
public interface IStructuredExtractionService
{
    /// <summary>
    /// Extract SWOT analysis from section content
    /// </summary>
    Task<Result<SwotData>> ExtractSwotAsync(string content, string language, CancellationToken ct = default);

    /// <summary>
    /// Extract financial metrics from section content
    /// </summary>
    Task<Result<FinancialMetricsData>> ExtractFinancialMetricsAsync(string content, string language, CancellationToken ct = default);

    /// <summary>
    /// Extract risk-mitigation pairs from section content
    /// </summary>
    Task<Result<RiskMitigationData>> ExtractRiskPairsAsync(string content, string language, CancellationToken ct = default);

    /// <summary>
    /// Extract key highlights/bullet points from section content for executive summary
    /// </summary>
    Task<Result<SectionHighlightsData>> ExtractHighlightsAsync(string sectionTitle, string content, string language, CancellationToken ct = default);
}

public class SwotData
{
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> Opportunities { get; set; } = new();
    public List<string> Threats { get; set; } = new();
}

public class FinancialMetricsData
{
    public string? Revenue { get; set; }
    public string? Expenses { get; set; }
    public string? Profit { get; set; }
    public string? BreakEvenPoint { get; set; }
    public string? GrossMargin { get; set; }
    public string? NetMargin { get; set; }
    public List<FinancialLineItem> LineItems { get; set; } = new();
}

public class FinancialLineItem
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Period { get; set; }
}

public class RiskMitigationData
{
    public List<RiskMitigationPair> Pairs { get; set; } = new();
}

public class RiskMitigationPair
{
    public string Risk { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // "high", "medium", "low"
    public string Mitigation { get; set; } = string.Empty;
}

public class SectionHighlightsData
{
    public string SectionTitle { get; set; } = string.Empty;
    public List<string> KeyPoints { get; set; } = new();
    public string? OneLinerSummary { get; set; }
}
