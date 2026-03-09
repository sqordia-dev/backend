using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;

namespace Sqordia.Infrastructure.Services.QualityAgents;

public class BankReadinessAgent : BaseQualityAgent
{
    public override string AgentName => "BankReadiness";

    public BankReadinessAgent(IAIService aiService, ILogger<BankReadinessAgent> logger)
        : base(aiService, logger) { }

    public override async Task<AgentAnalysisResult> AnalyzeAsync(BusinessPlanContext context, CancellationToken ct)
    {
        var isFrench = context.Language.Equals("fr", StringComparison.OrdinalIgnoreCase);

        var systemPrompt = isFrench
            ? @"Vous êtes un conseiller bancaire principal spécialisé dans l'évaluation de plans d'affaires pour le financement. Évaluez ce plan selon les critères bancaires. Répondez uniquement en JSON."
            : @"You are a senior banking advisor specializing in business plan evaluation for financing. Evaluate this plan against banking criteria. Respond only in JSON.";

        var userPrompt = $@"Evaluate this business plan for bank submission readiness:

{(context.BusinessBriefJson != null ? $"Business Brief:\n{context.BusinessBriefJson}\n\n" : "")}

{BuildSectionsSummary(context)}

Evaluate against bank submission criteria:
1. Executive Summary completeness and persuasiveness
2. Market analysis depth and credibility
3. Financial projections realism and detail
4. Management team credibility
5. Risk mitigation comprehensiveness
6. Funding requirements clarity and justification
7. Overall professionalism and completeness

Respond with JSON:
{{
  ""score"": 78,
  ""findings"": [
    {{ ""section"": ""FinancialProjections"", ""severity"": ""critical"", ""finding"": ""Missing break-even analysis"", ""suggestion"": ""Add detailed break-even calculation with timeline"", ""autoFixable"": false }},
    {{ ""section"": ""ExecutiveSummary"", ""severity"": ""medium"", ""finding"": ""Missing funding ask amount"", ""suggestion"": ""State the specific funding amount requested"", ""autoFixable"": true }}
  ]
}}";

        var response = await AiService.GenerateContentWithRetryAsync(
            systemPrompt, userPrompt, maxTokens: 3000, temperature: 0.2f, maxRetries: 2, ct);

        return new AgentAnalysisResult
        {
            AgentName = AgentName,
            Score = ParseScore(response),
            Findings = ParseFindings(response, AgentName)
        };
    }
}
