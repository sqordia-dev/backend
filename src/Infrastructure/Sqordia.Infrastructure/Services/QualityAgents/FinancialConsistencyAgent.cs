using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;

namespace Sqordia.Infrastructure.Services.QualityAgents;

public class FinancialConsistencyAgent : BaseQualityAgent
{
    public override string AgentName => "FinancialConsistency";

    public FinancialConsistencyAgent(IAIService aiService, ILogger<FinancialConsistencyAgent> logger)
        : base(aiService, logger) { }

    public override async Task<AgentAnalysisResult> AnalyzeAsync(BusinessPlanContext context, CancellationToken ct)
    {
        var isFrench = context.Language.Equals("fr", StringComparison.OrdinalIgnoreCase);

        var systemPrompt = isFrench
            ? @"Vous êtes un analyste financier expert. Vérifiez la cohérence des chiffres et projections dans le plan d'affaires. Répondez uniquement en JSON."
            : @"You are an expert financial analyst. Verify the consistency of numbers and projections in the business plan. Respond only in JSON.";

        // Focus on financial sections
        var financialSections = new[] { "FinancialProjections", "FundingRequirements", "BusinessModel", "OperationsPlan" };
        var sb = new System.Text.StringBuilder();
        foreach (var section in financialSections)
        {
            if (context.Sections.TryGetValue(section, out var content) && !string.IsNullOrWhiteSpace(content))
            {
                sb.AppendLine($"--- {section} ---");
                sb.AppendLine(content.Length > 2000 ? content.Substring(0, 2000) + "..." : content);
                sb.AppendLine();
            }
        }

        var userPrompt = $@"Analyze financial consistency across these business plan sections:

{sb}

Check for: numbers matching across sections, realistic projections, formulas validity, consistent currency usage, revenue/cost alignment.

Respond with JSON:
{{
  ""score"": 72,
  ""findings"": [
    {{ ""section"": ""FinancialProjections"", ""severity"": ""high"", ""finding"": ""Revenue projection inconsistent with market size"", ""suggestion"": ""Align revenue with TAM/SAM analysis"", ""autoFixable"": false }}
  ]
}}";

        var response = await AiService.GenerateContentWithRetryAsync(
            systemPrompt, userPrompt, maxTokens: 2000, temperature: 0.2f, maxRetries: 2, ct);

        return new AgentAnalysisResult
        {
            AgentName = AgentName,
            Score = ParseScore(response),
            Findings = ParseFindings(response, AgentName)
        };
    }
}
