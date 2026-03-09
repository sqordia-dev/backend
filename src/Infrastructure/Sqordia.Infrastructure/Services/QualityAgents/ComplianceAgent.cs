using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;

namespace Sqordia.Infrastructure.Services.QualityAgents;

public class ComplianceAgent : BaseQualityAgent
{
    public override string AgentName => "Compliance";

    public ComplianceAgent(IAIService aiService, ILogger<ComplianceAgent> logger)
        : base(aiService, logger) { }

    public override async Task<AgentAnalysisResult> AnalyzeAsync(BusinessPlanContext context, CancellationToken ct)
    {
        var isFrench = context.Language.Equals("fr", StringComparison.OrdinalIgnoreCase);

        var systemPrompt = isFrench
            ? @"Vous êtes un expert en conformité réglementaire québécoise et canadienne. Vérifiez que le plan d'affaires respecte les exigences légales, incluant la Loi 25 (protection des renseignements personnels). Répondez uniquement en JSON."
            : @"You are a Quebec and Canadian regulatory compliance expert. Verify the business plan meets legal requirements, including Law 25 (personal information protection). Respond only in JSON.";

        var userPrompt = $@"Analyze compliance of this business plan:

Plan Type: {context.PlanType}

{BuildSectionsSummary(context, 800)}

Check for:
- Quebec Law 25 compliance (data privacy, consent, breach notification)
- Industry-specific regulatory mentions
- Legal structure appropriateness
- Tax compliance considerations
- Employment law awareness
- Charter of the French Language compliance

Respond with JSON:
{{
  ""score"": 90,
  ""findings"": [
    {{ ""section"": ""OperationsPlan"", ""severity"": ""high"", ""finding"": ""No mention of Law 25 compliance"", ""suggestion"": ""Add privacy impact assessment plan"", ""autoFixable"": false }}
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
