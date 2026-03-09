using System.Text.Json;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;

namespace Sqordia.Infrastructure.Services.QualityAgents;

/// <summary>
/// Base class for quality agents providing common AI interaction patterns.
/// </summary>
public abstract class BaseQualityAgent : IQualityAgent
{
    protected readonly IAIService AiService;
    protected readonly ILogger Logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public abstract string AgentName { get; }

    protected BaseQualityAgent(IAIService aiService, ILogger logger)
    {
        AiService = aiService;
        Logger = logger;
    }

    public abstract Task<AgentAnalysisResult> AnalyzeAsync(BusinessPlanContext context, CancellationToken ct);

    protected string BuildSectionsSummary(BusinessPlanContext context, int maxCharsPerSection = 1000)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var (name, content) in context.Sections)
        {
            if (string.IsNullOrWhiteSpace(content)) continue;
            sb.AppendLine($"--- {name} ---");
            sb.AppendLine(content.Length > maxCharsPerSection
                ? content.Substring(0, maxCharsPerSection) + "..."
                : content);
            sb.AppendLine();
        }
        return sb.ToString();
    }

    protected List<AgentFinding> ParseFindings(string aiResponse, string agentName)
    {
        try
        {
            var json = ExtractJson(aiResponse);
            var parsed = JsonSerializer.Deserialize<AgentResponseDto>(json, JsonOptions);
            if (parsed?.Findings == null) return new List<AgentFinding>();

            return parsed.Findings.Select(f => new AgentFinding
            {
                AgentName = agentName,
                Section = f.Section ?? "",
                Severity = ParseSeverity(f.Severity),
                Finding = f.Finding ?? "",
                Suggestion = f.Suggestion ?? "",
                AutoFixable = f.AutoFixable
            }).ToList();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to parse agent findings for {Agent}", agentName);
            return new List<AgentFinding>();
        }
    }

    protected decimal ParseScore(string aiResponse)
    {
        try
        {
            var json = ExtractJson(aiResponse);
            var parsed = JsonSerializer.Deserialize<AgentResponseDto>(json, JsonOptions);
            return parsed?.Score ?? 50;
        }
        catch
        {
            return 50;
        }
    }

    private static string ExtractJson(string response)
    {
        var trimmed = response.Trim();
        if (trimmed.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            var end = trimmed.LastIndexOf("```");
            if (end > 7) trimmed = trimmed.Substring(7, end - 7).Trim();
        }
        else if (trimmed.StartsWith("```"))
        {
            var nl = trimmed.IndexOf('\n');
            var end = trimmed.LastIndexOf("```");
            if (nl > 0 && end > nl) trimmed = trimmed.Substring(nl + 1, end - nl - 1).Trim();
        }

        var start = trimmed.IndexOf('{');
        var endBrace = trimmed.LastIndexOf('}');
        if (start >= 0 && endBrace > start) return trimmed.Substring(start, endBrace - start + 1);
        return trimmed;
    }

    private static FindingSeverity ParseSeverity(string? severity)
    {
        return severity?.ToLowerInvariant() switch
        {
            "critical" => FindingSeverity.Critical,
            "high" => FindingSeverity.High,
            "medium" => FindingSeverity.Medium,
            "low" => FindingSeverity.Low,
            _ => FindingSeverity.Medium
        };
    }

    private class AgentResponseDto
    {
        public decimal Score { get; set; }
        public List<FindingDto>? Findings { get; set; }
    }

    private class FindingDto
    {
        public string? Section { get; set; }
        public string? Severity { get; set; }
        public string? Finding { get; set; }
        public string? Suggestion { get; set; }
        public bool AutoFixable { get; set; }
    }
}
