using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services.AI;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// 3-pass generation pipeline:
/// Pass 1: Analysis — generates a generation plan from the Business Brief
/// Pass 2: Section Generation — dependency-ordered, parallel within tiers
/// Pass 3: Review &amp; Synthesis — coherence check, executive summary, quality scoring
/// </summary>
public class GenerationPipelineService : IGenerationPipelineService
{
    private readonly IApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly IAIPythonService _pythonService;
    private readonly IFeatureFlagsService _featureFlags;
    private readonly IAIPromptService _aiPromptService;
    private readonly IBusinessBriefService _businessBriefService;
    private readonly IEmailService _emailService;
    private readonly ILogger<GenerationPipelineService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Section dependency tiers. Sections within a tier can be generated in parallel.
    /// Later tiers depend on earlier tiers being complete.
    /// </summary>
    private static readonly string[][] SectionTiers = new[]
    {
        // Tier 1: Foundation sections (no dependencies)
        new[] { "MarketAnalysis", "ManagementTeam", "OperationsPlan" },
        // Tier 2: Build on market + team understanding
        new[] { "ProblemStatement", "Solution", "CompetitiveAnalysis", "SwotAnalysis", "BusinessModel" },
        // Tier 3: Build on competitive + business model
        new[] { "MarketingStrategy", "BrandingStrategy", "FinancialProjections" },
        // Tier 4: Build on everything above
        new[] { "RiskAnalysis", "FundingRequirements", "ExitStrategy" },
        // Tier 5: Synthesis (must come last) — Executive Summary generated from ALL sections
        new[] { "ExecutiveSummary" }
    };

    /// <summary>
    /// OBNL-specific sections inserted into appropriate tiers.
    /// </summary>
    private static readonly string[][] ObnlSectionTiers = new[]
    {
        new[] { "MarketAnalysis", "ManagementTeam", "OperationsPlan", "MissionStatement" },
        new[] { "ProblemStatement", "Solution", "CompetitiveAnalysis", "SwotAnalysis", "BusinessModel", "SocialImpact", "BeneficiaryProfile" },
        new[] { "MarketingStrategy", "BrandingStrategy", "FinancialProjections" },
        new[] { "RiskAnalysis", "FundingRequirements", "GrantStrategy", "SustainabilityPlan" },
        new[] { "ExecutiveSummary" }
    };

    public GenerationPipelineService(
        IApplicationDbContext context,
        IAIService aiService,
        IAIPythonService pythonService,
        IFeatureFlagsService featureFlags,
        IAIPromptService aiPromptService,
        IBusinessBriefService businessBriefService,
        IEmailService emailService,
        ILogger<GenerationPipelineService> logger)
    {
        _context = context;
        _aiService = aiService;
        _pythonService = pythonService;
        _featureFlags = featureFlags;
        _aiPromptService = aiPromptService;
        _businessBriefService = businessBriefService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<Domain.Entities.BusinessPlan.BusinessPlan>> ExecutePipelineAsync(
        Guid businessPlanId,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting multi-pass generation pipeline for plan {PlanId}", businessPlanId);

            var businessPlan = await _context.BusinessPlans
                .Include(bp => bp.Organization)
                .Include(bp => bp.QuestionnaireResponses)
                    .ThenInclude(qr => qr.QuestionTemplate)
                .Include(bp => bp.QuestionnaireResponses)
                    .ThenInclude(qr => qr.QuestionTemplateV2)
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<Domain.Entities.BusinessPlan.BusinessPlan>(
                    Error.NotFound("BusinessPlan.Error.NotFound", $"Business plan with ID {businessPlanId} not found."));
            }

            // Validate status
            if (businessPlan.Status == BusinessPlanStatus.Draft)
            {
                var template = await _context.QuestionnaireTemplates
                    .Include(qt => qt.Questions)
                    .Where(qt => qt.PlanType == businessPlan.PlanType && qt.IsActive)
                    .OrderByDescending(qt => qt.Version)
                    .FirstOrDefaultAsync(cancellationToken);

                if (template != null)
                {
                    var requiredQuestions = template.Questions.Where(q => q.IsRequired).Select(q => q.Id).ToList();
                    var answeredCount = await _context.QuestionnaireResponses
                        .Where(qr => qr.BusinessPlanId == businessPlanId &&
                                     qr.QuestionTemplateId.HasValue &&
                                     requiredQuestions.Contains(qr.QuestionTemplateId.Value))
                        .CountAsync(cancellationToken);

                    if (answeredCount == requiredQuestions.Count && requiredQuestions.Count > 0)
                    {
                        businessPlan.MarkQuestionnaireComplete();
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        return Result.Failure<Domain.Entities.BusinessPlan.BusinessPlan>(
                            Error.Validation("BusinessPlan.QuestionnaireIncomplete", "Questionnaire must be completed before generation."));
                    }
                }
            }
            else if (businessPlan.Status != BusinessPlanStatus.QuestionnaireComplete &&
                     businessPlan.Status != BusinessPlanStatus.Generating)
            {
                return Result.Failure<Domain.Entities.BusinessPlan.BusinessPlan>(
                    Error.Validation("BusinessPlan.InvalidStatus", $"Invalid status for generation: {businessPlan.Status}"));
            }

            if (businessPlan.Status != BusinessPlanStatus.Generating)
            {
                businessPlan.StartGeneration("AI-Pipeline");
                await _context.SaveChangesAsync(cancellationToken);
            }

            var answersByQuestionNumber = BuildAnswersDictionary(businessPlan.QuestionnaireResponses);

            // ===== PASS 1: Business Brief + Analysis =====
            businessPlan.UpdateGenerationProgress("Analyzing", 5);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Pass 1/3: Generating Business Brief and analysis for plan {PlanId}", businessPlanId);

            string? briefJson = businessPlan.BusinessBriefJson;
            try
            {
                var briefResult = await _businessBriefService.GenerateBusinessBriefAsync(businessPlanId, language, cancellationToken);
                if (briefResult.IsSuccess)
                {
                    // Re-read the updated brief
                    var refreshed = await _context.BusinessPlans.FirstAsync(bp => bp.Id == businessPlanId, cancellationToken);
                    briefJson = refreshed.BusinessBriefJson;
                }
                else
                {
                    _logger.LogWarning("Business Brief generation failed: {Error}. Proceeding without brief.", briefResult.Error?.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Business Brief generation failed. Proceeding without brief.");
            }

            // Generate analysis/generation plan
            var generationPlan = await GenerateAnalysisPlanAsync(briefJson, businessPlan, answersByQuestionNumber, language, cancellationToken);
            if (generationPlan != null)
            {
                businessPlan.SetGenerationPlan(generationPlan);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // ===== PASS 2: Tiered Section Generation =====
            _logger.LogInformation("Pass 2/3: Generating sections in dependency order for plan {PlanId}", businessPlanId);

            var tiers = GetTiersForPlanType(businessPlan.PlanType);
            var availableSections = GetAvailableSections(businessPlan.PlanType.ToString());
            var totalSections = availableSections.Count;
            var completedSections = 0;
            var generatedContent = new Dictionary<string, string>();

            foreach (var (tier, tierIndex) in tiers.Select((t, i) => (t, i)))
            {
                // Filter tier to only include sections available for this plan type
                var tierSections = tier.Where(s => availableSections.Contains(s)).ToList();
                if (!tierSections.Any()) continue;

                _logger.LogInformation("Generating tier {TierIndex}/{TotalTiers} with {SectionCount} sections in parallel",
                    tierIndex + 1, tiers.Length, tierSections.Count);

                // Generate sections within this tier in parallel
                var tasks = tierSections.Select(section =>
                    GenerateSectionWithContextAsync(
                        businessPlan.PlanType, section, answersByQuestionNumber,
                        briefJson, generatedContent, language, cancellationToken));

                var results = await Task.WhenAll(tasks);

                // Store results
                for (var i = 0; i < tierSections.Count; i++)
                {
                    var sectionName = tierSections[i];
                    var content = results[i];
                    generatedContent[sectionName] = content;
                    SetSectionContent(businessPlan, sectionName, content);

                    completedSections++;
                    var progress = 10 + (int)((decimal)completedSections / totalSections * 75); // 10-85%
                    businessPlan.UpdateGenerationProgress(sectionName, progress);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }

            // ===== PASS 3: Review & Synthesis =====
            _logger.LogInformation("Pass 3/3: Review and synthesis for plan {PlanId}", businessPlanId);
            businessPlan.UpdateGenerationProgress("Reviewing", 90);
            await _context.SaveChangesAsync(cancellationToken);

            var qualityReport = await RunReviewAndSynthesisAsync(
                businessPlan, generatedContent, briefJson, language, cancellationToken);

            if (qualityReport != null)
            {
                var reportJson = JsonSerializer.Serialize(qualityReport, JsonOptions);
                businessPlan.SetQualityReport(
                    reportJson,
                    qualityReport.BankReadinessScore,
                    qualityReport.CoherenceScore);

                // If review generated an improved executive summary, update it
                if (!string.IsNullOrWhiteSpace(qualityReport.SynthesizedExecutiveSummary))
                {
                    businessPlan.UpdateExecutiveSummary(qualityReport.SynthesizedExecutiveSummary);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }

            // LLM-as-Judge evaluation via Python service (when feature flag is enabled)
            await RunLlmJudgeIfEnabledAsync(businessPlan, generatedContent, briefJson, language, cancellationToken);

            // Complete generation
            businessPlan.UpdateGenerationProgress(null, 100);
            businessPlan.CompleteGeneration();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Multi-pass pipeline completed for plan {PlanId}", businessPlanId);

            // Send notification email
            await SendCompletionEmailAsync(businessPlan, cancellationToken);

            return Result.Success(businessPlan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in generation pipeline for plan {PlanId}", businessPlanId);
            return Result.Failure<Domain.Entities.BusinessPlan.BusinessPlan>($"Pipeline failed: {ex.Message}");
        }
    }

    #region Pass 1: Analysis

    private async Task<string?> GenerateAnalysisPlanAsync(
        string? briefJson,
        Domain.Entities.BusinessPlan.BusinessPlan businessPlan,
        Dictionary<int, string> answers,
        string language,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(briefJson))
            return null;

        try
        {
            var isFrench = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
            var systemPrompt = isFrench
                ? @"Vous êtes un stratège de plans d'affaires. Analysez le Business Brief et créez un plan de génération structuré.
Répondez UNIQUEMENT avec un JSON valide."
                : @"You are a business plan strategist. Analyze the Business Brief and create a structured generation plan.
Respond ONLY with valid JSON.";

            var availableSections = GetAvailableSections(businessPlan.PlanType.ToString());
            var sectionList = string.Join(", ", availableSections);

            var userPrompt = new StringBuilder();
            userPrompt.AppendLine(isFrench ? "Business Brief:" : "Business Brief:");
            userPrompt.AppendLine(briefJson);
            userPrompt.AppendLine();
            userPrompt.AppendLine(isFrench
                ? $"Sections à générer: {sectionList}"
                : $"Sections to generate: {sectionList}");
            userPrompt.AppendLine();
            userPrompt.AppendLine(isFrench
                ? "Créez un plan de génération JSON avec cette structure:"
                : "Create a generation plan JSON with this structure:");
            userPrompt.AppendLine(@"{
  ""overallTheme"": ""The unifying business narrative"",
  ""narrativeArc"": ""How sections should flow together"",
  ""sectionGuidance"": {
    ""SectionName"": {
      ""keyPoints"": [""Point 1"", ""Point 2""],
      ""dataToEmphasize"": [""Data aspect 1""],
      ""crossReferences"": [""RelatedSection1""],
      ""tone"": ""data-driven, confident"",
      ""estimatedLength"": ""600-900 words""
    }
  }
}");

            var response = await _aiService.GenerateContentWithRetryAsync(
                systemPrompt, userPrompt.ToString(),
                maxTokens: 3000, temperature: 0.3f, maxRetries: 2, ct);

            return ExtractJsonFromResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate analysis plan. Proceeding without it.");
            return null;
        }
    }

    #endregion

    #region Pass 2: Section Generation

    private async Task<string> GenerateSectionWithContextAsync(
        BusinessPlanType planType,
        string sectionName,
        Dictionary<int, string> answers,
        string? briefJson,
        Dictionary<string, string> generatedContent,
        string language,
        CancellationToken ct)
    {
        // Build enriched context
        string context;
        if (!string.IsNullOrWhiteSpace(briefJson))
        {
            context = QuestionContextMapper.BuildSectionContextWithBrief(
                sectionName, answers, briefJson, language);
        }
        else
        {
            context = QuestionContextMapper.BuildSectionContext(sectionName, answers, language);
        }

        // Add cross-references from already-generated sections
        var crossRefContext = BuildCrossReferenceContext(sectionName, generatedContent, language);
        if (!string.IsNullOrWhiteSpace(crossRefContext))
        {
            context += "\n\n" + crossRefContext;
        }

        var systemPrompt = await GetSystemPromptAsync(language, planType, ct);
        var userPrompt = await GetSectionPromptAsync(planType, sectionName, context, language, ct);

        return await _aiService.GenerateContentWithRetryAsync(
            systemPrompt, userPrompt,
            maxTokens: 4000, temperature: 0.7f, maxRetries: 3, ct);
    }

    private static string? BuildCrossReferenceContext(
        string sectionName,
        Dictionary<string, string> generatedContent,
        string language)
    {
        // Define which sections each section should reference
        var crossRefs = sectionName switch
        {
            "CompetitiveAnalysis" => new[] { "MarketAnalysis" },
            "SwotAnalysis" => new[] { "MarketAnalysis", "CompetitiveAnalysis" },
            "BusinessModel" => new[] { "MarketAnalysis", "CompetitiveAnalysis" },
            "MarketingStrategy" => new[] { "MarketAnalysis", "CompetitiveAnalysis", "BusinessModel" },
            "BrandingStrategy" => new[] { "MarketingStrategy" },
            "FinancialProjections" => new[] { "BusinessModel", "OperationsPlan", "ManagementTeam" },
            "FundingRequirements" => new[] { "FinancialProjections" },
            "RiskAnalysis" => new[] { "SwotAnalysis", "FinancialProjections" },
            "ExitStrategy" => new[] { "FinancialProjections", "BusinessModel" },
            "ExecutiveSummary" => generatedContent.Keys.Where(k => k != "ExecutiveSummary").ToArray(),
            "GrantStrategy" => new[] { "FinancialProjections", "SocialImpact" },
            "SustainabilityPlan" => new[] { "FinancialProjections", "GrantStrategy" },
            _ => Array.Empty<string>()
        };

        var isFrench = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
        var refs = new List<string>();

        foreach (var refSection in crossRefs)
        {
            if (generatedContent.TryGetValue(refSection, out var content) && !string.IsNullOrWhiteSpace(content))
            {
                // Include a truncated summary of the referenced section
                var truncated = content.Length > 500 ? content.Substring(0, 500) + "..." : content;
                var label = isFrench
                    ? $"[Référence - Section déjà générée: {refSection}]"
                    : $"[Reference - Already generated section: {refSection}]";
                refs.Add($"{label}\n{truncated}");
            }
        }

        if (!refs.Any()) return null;

        var header = isFrench
            ? "=== SECTIONS CONNEXES (pour cohérence) ==="
            : "=== RELATED SECTIONS (for coherence) ===";

        return $"{header}\n\n{string.Join("\n\n", refs)}";
    }

    #endregion

    #region Pass 3: Review & Synthesis

    private async Task<QualityReport?> RunReviewAndSynthesisAsync(
        Domain.Entities.BusinessPlan.BusinessPlan businessPlan,
        Dictionary<string, string> generatedContent,
        string? briefJson,
        string language,
        CancellationToken ct)
    {
        try
        {
            var isFrench = language.Equals("fr", StringComparison.OrdinalIgnoreCase);

            var systemPrompt = isFrench
                ? @"Vous êtes un expert en révision de plans d'affaires pour les institutions bancaires.
Analysez le plan d'affaires complet, identifiez les incohérences, et synthétisez un résumé exécutif.
Répondez UNIQUEMENT avec un JSON valide."
                : @"You are a business plan review expert for banking institutions.
Analyze the complete business plan, identify inconsistencies, and synthesize an executive summary.
Respond ONLY with valid JSON.";

            var userPrompt = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(briefJson))
            {
                userPrompt.AppendLine(isFrench ? "=== BUSINESS BRIEF ===" : "=== BUSINESS BRIEF ===");
                userPrompt.AppendLine(briefJson);
                userPrompt.AppendLine();
            }

            userPrompt.AppendLine(isFrench ? "=== SECTIONS GÉNÉRÉES ===" : "=== GENERATED SECTIONS ===");
            foreach (var (section, content) in generatedContent)
            {
                userPrompt.AppendLine($"\n--- {section} ---");
                // Truncate each section to keep input manageable
                var truncated = content.Length > 1500 ? content.Substring(0, 1500) + "..." : content;
                userPrompt.AppendLine(truncated);
            }

            userPrompt.AppendLine();
            if (isFrench)
            {
                userPrompt.AppendLine(@"Analysez le plan et produisez un rapport JSON avec cette structure.
IMPORTANT : Le résumé exécutif (synthesizedExecutiveSummary) DOIT être rédigé UNIQUEMENT en français.");
                userPrompt.AppendLine(@"{
  ""coherenceScore"": 85,
  ""bankReadinessScore"": 72,
  ""issues"": [
    { ""section"": ""FinancialProjections"", ""type"": ""inconsistency"", ""description"": ""Description du problème en français..."" }
  ],
  ""improvements"": [
    { ""section"": ""MarketAnalysis"", ""suggestion"": ""Ajouter des données sur la taille du marché"" }
  ],
  ""synthesizedExecutiveSummary"": ""Un résumé exécutif complet synthétisé à partir de toutes les sections, rédigé en français..."",
  ""executiveSummaryGenerated"": true
}");
            }
            else
            {
                userPrompt.AppendLine(@"Analyze the plan and produce a JSON report with this structure:");
                userPrompt.AppendLine(@"{
  ""coherenceScore"": 85,
  ""bankReadinessScore"": 72,
  ""issues"": [
    { ""section"": ""FinancialProjections"", ""type"": ""inconsistency"", ""description"": ""..."" }
  ],
  ""improvements"": [
    { ""section"": ""MarketAnalysis"", ""suggestion"": ""Add market size data"" }
  ],
  ""synthesizedExecutiveSummary"": ""A comprehensive executive summary synthesized from all sections..."",
  ""executiveSummaryGenerated"": true
}");
            }

            var response = await _aiService.GenerateContentWithRetryAsync(
                systemPrompt, userPrompt.ToString(),
                maxTokens: 4000, temperature: 0.3f, maxRetries: 2, ct);

            var json = ExtractJsonFromResponse(response);
            return JsonSerializer.Deserialize<QualityReport>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Review and synthesis failed. Plan generated without quality report.");
            return null;
        }
    }

    #endregion

    #region Python LLM-as-Judge Integration

    /// <summary>
    /// Runs the Python LLM-as-Judge evaluation on the executive summary if the feature flag is enabled.
    /// Results are appended to the quality report. Failures are non-fatal.
    /// </summary>
    private async Task RunLlmJudgeIfEnabledAsync(
        Domain.Entities.BusinessPlan.BusinessPlan businessPlan,
        Dictionary<string, string> generatedContent,
        string? briefJson,
        string language,
        CancellationToken ct)
    {
        try
        {
            var useJudge = await _featureFlags.IsEnabledAsync("AI.UseLLMJudge", ct);
            if (!useJudge.IsSuccess || !useJudge.Value)
                return;

            if (!await _pythonService.IsAvailableAsync(ct))
            {
                _logger.LogWarning("Python AI service unavailable for LLM-as-Judge evaluation");
                return;
            }

            // Run judge evaluation on the executive summary (most representative section)
            if (generatedContent.TryGetValue("ExecutiveSummary", out var execSummary) && !string.IsNullOrWhiteSpace(execSummary))
            {
                var judgeResult = await _pythonService.RunJudgeEvaluationAsync(
                    new JudgeEvaluationRequest(
                        SectionName: "ExecutiveSummary",
                        SectionContent: execSummary,
                        BusinessBrief: briefJson ?? "{}",
                        Language: language
                    ), ct);

                _logger.LogInformation(
                    "LLM-as-Judge evaluation complete for plan {PlanId}: overall score {Score}, MLflow run {RunId}",
                    businessPlan.Id, judgeResult.OverallScore, judgeResult.MlflowRunId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LLM-as-Judge evaluation failed (non-fatal) for plan {PlanId}", businessPlan.Id);
        }
    }

    #endregion

    #region Helpers

    private string[][] GetTiersForPlanType(BusinessPlanType planType)
    {
        return planType == BusinessPlanType.StrategicPlan ? ObnlSectionTiers : SectionTiers;
    }

    private List<string> GetAvailableSections(string planType)
    {
        var commonSections = new List<string>
        {
            "ExecutiveSummary", "ProblemStatement", "Solution",
            "MarketAnalysis", "CompetitiveAnalysis", "SwotAnalysis",
            "BusinessModel", "MarketingStrategy", "BrandingStrategy",
            "OperationsPlan", "ManagementTeam", "FinancialProjections",
            "FundingRequirements", "RiskAnalysis"
        };

        if (planType == "StrategicPlan" || planType == "2")
        {
            commonSections.AddRange(new[] { "MissionStatement", "SocialImpact", "BeneficiaryProfile", "GrantStrategy", "SustainabilityPlan" });
        }
        else if (planType == "BusinessPlan" || planType == "0")
        {
            commonSections.Add("ExitStrategy");
        }

        return commonSections;
    }

    private async Task<string> GetSystemPromptAsync(string language, BusinessPlanType planType, CancellationToken ct)
    {
        try
        {
            var promptDto = await _aiPromptService.GetSystemPromptAsync(planType.ToString(), language, ct);
            if (promptDto != null)
                return promptDto.SystemPrompt;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load system prompt from database, using fallback");
        }

        return GetFallbackSystemPrompt(language);
    }

    private static string GetFallbackSystemPrompt(string language)
    {
        return language.Equals("fr", StringComparison.OrdinalIgnoreCase)
            ? @"Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience. Rédigez UNIQUEMENT en français dans un ton professionnel, clair et convaincant. Ne produisez JAMAIS de contenu en anglais."
            : @"You are an expert business plan consultant with 20 years of experience. Write ONLY in English in a professional, clear, and compelling tone. Never produce content in any other language.";
    }

    private async Task<string> GetSectionPromptAsync(
        BusinessPlanType planType, string sectionName, string context, string language, CancellationToken ct)
    {
        var normalizedName = NormalizeSectionName(sectionName);

        try
        {
            var promptDto = await _aiPromptService.GetPromptBySectionAsync(
                normalizedName, planType.ToString(), language, "ContentGeneration", ct);

            if (promptDto != null)
            {
                return promptDto.UserPromptTemplate
                    .Replace("{sectionName}", sectionName)
                    .Replace("{questionnaireContext}", context)
                    .Replace("{context}", context);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load section prompt for {Section}", sectionName);
        }

        // Fallback — language-aware
        if (language.Equals("fr", StringComparison.OrdinalIgnoreCase))
            return $"Rédigez la section « {sectionName} » pour ce plan d'affaires en vous basant sur le contexte suivant.\nRépondez UNIQUEMENT en français.\n\n{context}\n\nRédigez une section professionnelle et convaincante de 400 à 800 mots.";
        return $"Generate the {sectionName} section for this business plan based on the following context:\n\n{context}\n\nWrite a comprehensive, professional section of 400-800 words.";
    }

    private static string NormalizeSectionName(string sectionName)
    {
        if (string.IsNullOrEmpty(sectionName) || !sectionName.Contains('-'))
            return sectionName;

        var parts = sectionName.Split('-');
        return string.Concat(parts.Select(part =>
            string.IsNullOrEmpty(part) ? part :
            char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant()));
    }

    private void SetSectionContent(Domain.Entities.BusinessPlan.BusinessPlan businessPlan, string sectionName, string content)
    {
        var normalizedName = NormalizeSectionName(sectionName);
        var property = typeof(Domain.Entities.BusinessPlan.BusinessPlan).GetProperty(normalizedName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(businessPlan, content);
        }
        else
        {
            _logger.LogWarning("Unknown or read-only section: {Section}", sectionName);
        }
    }

    private Dictionary<int, string> BuildAnswersDictionary(
        ICollection<Domain.Entities.BusinessPlan.QuestionnaireResponse> responses)
    {
        var answers = new Dictionary<int, string>();
        if (responses == null || !responses.Any()) return answers;

        var responsesWithTemplates = responses
            .Where(r => r.QuestionTemplate != null || r.QuestionTemplateV2 != null)
            .ToList();

        if (responsesWithTemplates.Any())
        {
            foreach (var response in responsesWithTemplates)
            {
                var questionNumber = response.QuestionTemplate?.Order
                    ?? response.QuestionTemplateV2?.Order ?? 0;
                if (questionNumber <= 0) continue;

                var answer = ExtractAnswer(response);
                if (!string.IsNullOrWhiteSpace(answer))
                    answers[questionNumber] = answer;
            }
        }

        if (!answers.Any())
        {
            var ordered = responses.Where(r => !string.IsNullOrWhiteSpace(r.ResponseText))
                .OrderBy(r => r.Created).ToList();
            var num = 1;
            foreach (var r in ordered)
            {
                if (!string.IsNullOrWhiteSpace(r.ResponseText))
                    answers[num] = r.ResponseText;
                num++;
            }
        }

        return answers;
    }

    private static string ExtractAnswer(Domain.Entities.BusinessPlan.QuestionnaireResponse response)
    {
        var qt = response.QuestionTemplate?.QuestionType
            ?? response.QuestionTemplateV2?.QuestionType
            ?? Domain.Enums.QuestionType.LongText;

        return qt switch
        {
            QuestionType.ShortText or QuestionType.LongText => response.ResponseText ?? "",
            QuestionType.Number => response.NumericValue?.ToString() ?? "",
            QuestionType.Currency => response.NumericValue.HasValue ? $"${response.NumericValue:N2}" : "",
            QuestionType.Percentage => response.NumericValue.HasValue ? $"{response.NumericValue}%" : "",
            QuestionType.Date => response.DateValue?.ToString("yyyy-MM-dd") ?? "",
            QuestionType.YesNo => response.BooleanValue?.ToString() ?? "",
            QuestionType.SingleChoice or QuestionType.MultipleChoice => response.SelectedOptions ?? "",
            QuestionType.Scale => response.NumericValue?.ToString() ?? "",
            _ => response.ResponseText ?? ""
        };
    }

    private static string ExtractJsonFromResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response)) return "{}";
        var trimmed = response.Trim();

        if (trimmed.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            var endIdx = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (endIdx > 7) trimmed = trimmed.Substring(7, endIdx - 7).Trim();
        }
        else if (trimmed.StartsWith("```"))
        {
            var nl = trimmed.IndexOf('\n');
            var endIdx = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (nl > 0 && endIdx > nl) trimmed = trimmed.Substring(nl + 1, endIdx - nl - 1).Trim();
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start >= 0 && end > start) return trimmed.Substring(start, end - start + 1);

        return trimmed;
    }

    private async Task SendCompletionEmailAsync(
        Domain.Entities.BusinessPlan.BusinessPlan businessPlan, CancellationToken ct)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id.ToString() == businessPlan.CreatedBy, ct);

            if (user?.Email != null && !string.IsNullOrEmpty(user.Email.Value))
            {
                var userName = !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : user.UserName ?? "User";
                await _emailService.SendBusinessPlanGeneratedAsync(
                    user.Email.Value, userName, businessPlan.Id.ToString(), businessPlan.Title);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send completion email for plan {PlanId}", businessPlan.Id);
        }
    }

    #endregion
}

/// <summary>
/// Quality report from Pass 3 review and synthesis.
/// </summary>
internal class QualityReport
{
    public decimal CoherenceScore { get; set; }
    public decimal BankReadinessScore { get; set; }
    public List<QualityIssue>? Issues { get; set; }
    public List<QualityImprovement>? Improvements { get; set; }
    public string? SynthesizedExecutiveSummary { get; set; }
    public bool ExecutiveSummaryGenerated { get; set; }
}

internal class QualityIssue
{
    public string? Section { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
}

internal class QualityImprovement
{
    public string? Section { get; set; }
    public string? Suggestion { get; set; }
}
