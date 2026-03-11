using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services.AI;
using Sqordia.Contracts.Responses.BusinessPlan;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Generates a unified Business Brief from all questionnaire answers and onboarding context.
/// The brief provides holistic business understanding that enriches every section's generation context.
/// </summary>
public class BusinessBriefService : IBusinessBriefService
{
    private readonly IApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly IAITelemetryService _telemetry;
    private readonly IQuestionnaireContextService _questionnaireContext;
    private readonly ILogger<BusinessBriefService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public BusinessBriefService(
        IApplicationDbContext context,
        IAIService aiService,
        IAITelemetryService telemetry,
        IQuestionnaireContextService questionnaireContext,
        ILogger<BusinessBriefService> logger)
    {
        _context = context;
        _aiService = aiService;
        _telemetry = telemetry;
        _questionnaireContext = questionnaireContext;
        _logger = logger;
    }

    public async Task<Result<BusinessBriefDto>> GenerateBusinessBriefAsync(
        Guid businessPlanId,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating Business Brief for plan {PlanId}", businessPlanId);

            var businessPlan = await _context.BusinessPlans
                .Include(bp => bp.Organization)
                .Include(bp => bp.QuestionnaireResponses)
                    .ThenInclude(qr => qr.QuestionTemplate)
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<BusinessBriefDto>(
                    Error.NotFound("BusinessBrief.Error.NotFound", $"Business plan with ID {businessPlanId} not found."));
            }

            // Build complete context from ALL questionnaire answers
            var answersByQuestionNumber = _questionnaireContext.BuildAnswersDictionary(businessPlan.QuestionnaireResponses);
            var fullContext = QuestionContextMapper.BuildFullContext(answersByQuestionNumber, language);

            // Build onboarding context: prefer Organization entity fields, fall back to OnboardingContextJson
            var onboardingContext = BuildOnboardingContext(businessPlan.Organization, businessPlan.OnboardingContextJson);

            // Build the AI prompt for brief generation
            var systemPrompt = GetBriefSystemPrompt(language);
            var userPrompt = BuildBriefUserPrompt(fullContext, onboardingContext, businessPlan, language);

            _logger.LogInformation(
                "Calling AI for Business Brief generation with {AnswerCount} answers and onboarding context",
                answersByQuestionNumber.Count);

            var aiResult = await _aiService.GenerateContentWithMetadataAsync(
                systemPrompt,
                userPrompt,
                maxTokens: Common.Constants.PipelineConstants.SectionMaxTokens,
                temperature: Common.Constants.PipelineConstants.AnalysisTemperature,
                maxRetries: Common.Constants.PipelineConstants.ReducedMaxRetries,
                cancellationToken);

            await _telemetry.LogCallAsync(new AICallTelemetry(
                PromptTemplateId: null,
                Provider: aiResult.ModelUsed ?? "unknown",
                ModelUsed: aiResult.ModelUsed ?? "unknown",
                InputTokens: aiResult.InputTokens,
                OutputTokens: aiResult.OutputTokens,
                LatencyMs: aiResult.LatencyMs,
                SectionType: null,
                Language: language,
                PipelinePass: Common.Constants.PipelineConstants.PipelinePass.BusinessBrief,
                Timestamp: DateTime.UtcNow,
                Temperature: Common.Constants.PipelineConstants.AnalysisTemperature), cancellationToken);

            // Parse the AI response into structured brief
            var briefDto = ParseBriefResponse(aiResult.Content, businessPlanId);

            // Store the brief on the entity
            var briefJson = JsonSerializer.Serialize(briefDto, JsonOptions);
            businessPlan.SetBusinessBrief(briefJson);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Business Brief generated and stored for plan {PlanId}", businessPlanId);

            return Result.Success(briefDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Business Brief for plan {PlanId}", businessPlanId);
            return Result.Failure<BusinessBriefDto>($"Failed to generate Business Brief: {ex.Message}");
        }
    }

    public async Task<Result<BusinessBriefDto>> GetBusinessBriefAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        var businessPlan = await _context.BusinessPlans
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

        if (businessPlan == null)
        {
            return Result.Failure<BusinessBriefDto>(
                Error.NotFound("BusinessBrief.Error.NotFound", $"Business plan with ID {businessPlanId} not found."));
        }

        if (string.IsNullOrWhiteSpace(businessPlan.BusinessBriefJson))
        {
            return Result.Failure<BusinessBriefDto>(
                Error.NotFound("BusinessBrief.Error.NotGenerated", "Business Brief has not been generated yet."));
        }

        try
        {
            var briefDto = JsonSerializer.Deserialize<BusinessBriefDto>(businessPlan.BusinessBriefJson, JsonOptions);
            if (briefDto == null)
            {
                return Result.Failure<BusinessBriefDto>("Failed to deserialize Business Brief.");
            }

            return Result.Success(briefDto);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize Business Brief for plan {PlanId}", businessPlanId);
            return Result.Failure<BusinessBriefDto>("Business Brief data is corrupted.");
        }
    }

    private string GetBriefSystemPrompt(string language)
    {
        return language.Equals("fr", StringComparison.OrdinalIgnoreCase)
            ? @"Vous êtes un analyste d'affaires expert spécialisé dans la synthèse de données d'entreprise.
Votre rôle est d'analyser toutes les réponses au questionnaire et le contexte d'onboarding pour produire un résumé structuré (Business Brief) qui capture l'essence complète de l'entreprise.

INSTRUCTIONS CRITIQUES:
- Analysez TOUTES les réponses fournies, pas seulement certaines
- Identifiez les connexions entre les différentes réponses
- Évaluez la maturité globale du projet
- Identifiez les forces et les lacunes dans les informations fournies
- Fournissez des recommandations pour la génération du plan d'affaires

Répondez UNIQUEMENT avec un objet JSON valide suivant exactement la structure demandée. Pas de texte avant ou après le JSON."
            : @"You are an expert business analyst specializing in business data synthesis.
Your role is to analyze all questionnaire responses and onboarding context to produce a structured summary (Business Brief) that captures the complete essence of the business.

CRITICAL INSTRUCTIONS:
- Analyze ALL provided responses, not just some
- Identify connections between different responses
- Assess the overall project maturity
- Identify strengths and gaps in the provided information
- Provide recommendations for business plan generation

Respond ONLY with a valid JSON object following exactly the requested structure. No text before or after the JSON.";
    }

    private string BuildBriefUserPrompt(
        string fullContext,
        OnboardingContextInfo? onboardingContext,
        Domain.Entities.BusinessPlan.BusinessPlan businessPlan,
        string language)
    {
        var isFrench = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
        var sb = new System.Text.StringBuilder();

        // Add onboarding context if available
        if (onboardingContext != null)
        {
            sb.AppendLine(isFrench ? "=== CONTEXTE D'ONBOARDING ===" : "=== ONBOARDING CONTEXT ===");
            if (!string.IsNullOrWhiteSpace(onboardingContext.Industry))
                sb.AppendLine($"- {(isFrench ? "Industrie" : "Industry")}: {onboardingContext.Industry}");
            if (!string.IsNullOrWhiteSpace(onboardingContext.BusinessStage))
                sb.AppendLine($"- {(isFrench ? "Stade" : "Stage")}: {onboardingContext.BusinessStage}");
            if (!string.IsNullOrWhiteSpace(onboardingContext.TeamSize))
                sb.AppendLine($"- {(isFrench ? "Taille de l'équipe" : "Team Size")}: {onboardingContext.TeamSize}");
            if (!string.IsNullOrWhiteSpace(onboardingContext.FundingStatus))
                sb.AppendLine($"- {(isFrench ? "Statut de financement" : "Funding Status")}: {onboardingContext.FundingStatus}");
            if (!string.IsNullOrWhiteSpace(onboardingContext.TargetMarket))
                sb.AppendLine($"- {(isFrench ? "Marché cible" : "Target Market")}: {onboardingContext.TargetMarket}");
            if (onboardingContext.Goals?.Any() == true)
                sb.AppendLine($"- {(isFrench ? "Objectifs" : "Goals")}: {string.Join(", ", onboardingContext.Goals)}");
            sb.AppendLine();
        }

        // Add persona info
        if (businessPlan.Persona.HasValue)
        {
            sb.AppendLine($"{(isFrench ? "Persona" : "Persona")}: {businessPlan.Persona}");
            sb.AppendLine();
        }

        // Add plan type
        sb.AppendLine($"{(isFrench ? "Type de plan" : "Plan Type")}: {businessPlan.PlanType}");
        sb.AppendLine($"{(isFrench ? "Organisation" : "Organization")}: {businessPlan.Organization?.Name ?? "N/A"}");
        sb.AppendLine();

        // Add all questionnaire responses
        sb.AppendLine(fullContext);
        sb.AppendLine();

        // Request structured output
        sb.AppendLine(isFrench
            ? "Analysez toutes les informations ci-dessus et produisez un Business Brief structuré au format JSON suivant:"
            : "Analyze all the information above and produce a structured Business Brief in the following JSON format:");

        sb.AppendLine(@"{
  ""companyProfile"": {
    ""name"": ""Company name extracted from answers"",
    ""legalStructure"": ""Legal structure if mentioned"",
    ""industry"": ""Industry/sector"",
    ""sector"": ""Specific sector or niche"",
    ""stage"": ""Business stage (idea, startup, growth, etc.)""
  },
  ""businessConcept"": {
    ""problem"": ""The core problem being solved"",
    ""solution"": ""The proposed solution"",
    ""valueProposition"": ""Unique value proposition"",
    ""differentiators"": [""Key differentiator 1"", ""Key differentiator 2""]
  },
  ""marketContext"": {
    ""targetCustomers"": ""Target customer description"",
    ""marketSize"": ""Market size assessment"",
    ""competitors"": ""Key competitive landscape summary"",
    ""positioning"": ""Market positioning strategy""
  },
  ""operationalContext"": {
    ""team"": ""Team composition and capabilities"",
    ""resources"": ""Key resources needed"",
    ""timeline"": ""Launch timeline and milestones"",
    ""location"": ""Business location/geography""
  },
  ""financialContext"": {
    ""personalInvestment"": ""Personal investment details"",
    ""fundingNeeds"": ""Total funding requirements"",
    ""revenueModel"": ""How the business makes money"",
    ""pricing"": ""Pricing strategy summary""
  },
  ""strategicContext"": {
    ""objectives"": ""Key Year 1 objectives"",
    ""swot"": {
      ""strengths"": [""Strength 1""],
      ""weaknesses"": [""Weakness 1""],
      ""opportunities"": [""Opportunity 1""],
      ""threats"": [""Threat 1""]
    },
    ""risks"": ""Key risks identified"",
    ""growthStrategy"": ""Growth strategy summary""
  },
  ""maturityAssessment"": {
    ""score"": 65,
    ""strengths"": [""Well-defined target market""],
    ""gaps"": [""Missing financial projections detail""],
    ""recommendations"": [""Add more competitive analysis data""]
  },
  ""generationGuidance"": {
    ""tone"": ""Professional and data-driven"",
    ""focusAreas"": [""Market validation"", ""Financial projections""],
    ""cautionAreas"": [""Avoid overestimating market size""],
    ""personaSpecificNotes"": ""Notes specific to this persona type""
  }
}");

        return sb.ToString();
    }

    private BusinessBriefDto ParseBriefResponse(string aiResponse, Guid businessPlanId)
    {
        // Extract JSON from the response (AI might wrap it in markdown code blocks)
        var jsonContent = ExtractJsonFromResponse(aiResponse);

        try
        {
            var briefDto = JsonSerializer.Deserialize<BusinessBriefDto>(jsonContent, JsonOptions);
            if (briefDto != null)
            {
                briefDto.BusinessPlanId = businessPlanId;
                briefDto.GeneratedAt = DateTime.UtcNow;
                return briefDto;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse AI response as JSON, creating brief from raw response");
        }

        // Fallback: create a minimal brief from the raw response
        return new BusinessBriefDto
        {
            BusinessPlanId = businessPlanId,
            GeneratedAt = DateTime.UtcNow,
            GenerationGuidance = new GenerationGuidanceDto
            {
                PersonaSpecificNotes = aiResponse
            }
        };
    }

    private static string ExtractJsonFromResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return "{}";

        var trimmed = response.Trim();

        // Remove markdown code block wrappers if present
        if (trimmed.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            var endIdx = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (endIdx > 7)
            {
                trimmed = trimmed.Substring(7, endIdx - 7).Trim();
            }
        }
        else if (trimmed.StartsWith("```"))
        {
            var firstNewline = trimmed.IndexOf('\n');
            var endIdx = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (firstNewline > 0 && endIdx > firstNewline)
            {
                trimmed = trimmed.Substring(firstNewline + 1, endIdx - firstNewline - 1).Trim();
            }
        }

        // Find the first { and last } to extract the JSON object
        var startBrace = trimmed.IndexOf('{');
        var endBrace = trimmed.LastIndexOf('}');
        if (startBrace >= 0 && endBrace > startBrace)
        {
            return trimmed.Substring(startBrace, endBrace - startBrace + 1);
        }

        return trimmed;
    }

    /// <summary>
    /// Builds onboarding context from Organization entity fields first, falling back to OnboardingContextJson for old plans.
    /// </summary>
    private static OnboardingContextInfo? BuildOnboardingContext(Domain.Entities.Organization? organization, string? onboardingContextJson)
    {
        // Try Organization entity fields first (new V2 onboarding stores data here)
        if (organization != null)
        {
            var hasOrgData = !string.IsNullOrWhiteSpace(organization.Industry)
                          || !string.IsNullOrWhiteSpace(organization.BusinessStage)
                          || !string.IsNullOrWhiteSpace(organization.TeamSize)
                          || !string.IsNullOrWhiteSpace(organization.FundingStatus)
                          || !string.IsNullOrWhiteSpace(organization.TargetMarket)
                          || !string.IsNullOrWhiteSpace(organization.GoalsJson);

            if (hasOrgData)
            {
                List<string>? goals = null;
                if (!string.IsNullOrWhiteSpace(organization.GoalsJson))
                {
                    try { goals = JsonSerializer.Deserialize<List<string>>(organization.GoalsJson); } catch { /* ignore */ }
                }

                return new OnboardingContextInfo
                {
                    Industry = organization.Industry,
                    BusinessStage = organization.BusinessStage,
                    TeamSize = organization.TeamSize,
                    FundingStatus = organization.FundingStatus,
                    TargetMarket = organization.TargetMarket,
                    Goals = goals
                };
            }
        }

        // Fall back to parsing OnboardingContextJson (old V1 plans)
        return ParseOnboardingContextJson(onboardingContextJson);
    }

    private static OnboardingContextInfo? ParseOnboardingContextJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<OnboardingContextInfo>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    private class OnboardingContextInfo
    {
        public string? Industry { get; set; }
        public string? BusinessStage { get; set; }
        public string? TeamSize { get; set; }
        public string? FundingStatus { get; set; }
        public string? TargetMarket { get; set; }
        public List<string>? Goals { get; set; }
    }
}
