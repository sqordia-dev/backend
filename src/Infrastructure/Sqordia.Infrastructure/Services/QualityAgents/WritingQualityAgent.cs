using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;

namespace Sqordia.Infrastructure.Services.QualityAgents;

public class WritingQualityAgent : BaseQualityAgent
{
    public override string AgentName => "WritingQuality";

    public WritingQualityAgent(IAIService aiService, ILogger<WritingQualityAgent> logger)
        : base(aiService, logger) { }

    public override async Task<AgentAnalysisResult> AnalyzeAsync(BusinessPlanContext context, CancellationToken ct)
    {
        var isFrench = context.Language.Equals("fr", StringComparison.OrdinalIgnoreCase);

        var systemPrompt = isFrench
            ? @"Vous êtes un rédacteur professionnel spécialisé dans les documents d'affaires. Analysez la qualité rédactionnelle du plan d'affaires. Répondez uniquement en JSON."
            : @"You are a professional writer specializing in business documents. Analyze the writing quality of the business plan. Respond only in JSON.";

        var userPrompt = $@"Analyze the following business plan sections for writing quality:

{BuildSectionsSummary(context)}

Evaluate: tone consistency, readability, professional language, jargon balance, grammar, and section coherence.

Respond with JSON:
{{
  ""score"": 85,
  ""findings"": [
    {{ ""section"": ""MarketAnalysis"", ""severity"": ""medium"", ""finding"": ""Inconsistent tone"", ""suggestion"": ""Use formal tone throughout"", ""autoFixable"": false }}
  ]
}}";

        var response = await AiService.GenerateContentWithRetryAsync(
            systemPrompt, userPrompt, maxTokens: 2000, temperature: 0.3f, maxRetries: 2, ct);

        return new AgentAnalysisResult
        {
            AgentName = AgentName,
            Score = ParseScore(response),
            Findings = ParseFindings(response, AgentName)
        };
    }
}
