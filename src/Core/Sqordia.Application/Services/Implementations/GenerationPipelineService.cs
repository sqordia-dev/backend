using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Constants;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Domain.Constants;
using Sqordia.Application.Services.AI;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// 3-pass generation pipeline:
/// Pass 1: Analysis — generates a generation plan from the Business Brief
/// Pass 2: Section Generation — dependency-ordered, parallel within tiers
/// Pass 3: Review &amp; Synthesis — coherence check, executive summary, quality scoring
///
/// Quality improvements:
/// - Section-specific rubrics and few-shot examples injected into prompts
/// - Context-aware temperature per section type
/// - Smart relevance-based context truncation (replaces arbitrary Substring)
/// - AI response validation (schema, refusal detection, language check)
/// - Telemetry logging for every AI call
/// - Data-driven section dependencies (replaces hardcoded switch)
/// - Business Brief as hard prerequisite (with feature flag)
/// - Org profile conflict detection
/// </summary>
public class GenerationPipelineService : IGenerationPipelineService
{
    private readonly IApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly IAIPythonService _pythonService;
    private readonly IFeatureFlagsService _featureFlags;
    private readonly IFeatureGateService _featureGate;
    private readonly IAIPromptService _aiPromptService;
    private readonly IBusinessBriefService _businessBriefService;
    private readonly IEmailService _emailService;
    private readonly IAITelemetryService _telemetry;
    private readonly IMLPredictionService _mlPrediction;
    private readonly IQuestionnaireContextService _questionnaireContext;
    private readonly ILogger<GenerationPipelineService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public GenerationPipelineService(
        IApplicationDbContext context,
        IAIService aiService,
        IAIPythonService pythonService,
        IFeatureFlagsService featureFlags,
        IFeatureGateService featureGate,
        IAIPromptService aiPromptService,
        IBusinessBriefService businessBriefService,
        IEmailService emailService,
        IAITelemetryService telemetry,
        IMLPredictionService mlPrediction,
        IQuestionnaireContextService questionnaireContext,
        ILogger<GenerationPipelineService> logger)
    {
        _context = context;
        _aiService = aiService;
        _pythonService = pythonService;
        _featureFlags = featureFlags;
        _featureGate = featureGate;
        _aiPromptService = aiPromptService;
        _businessBriefService = businessBriefService;
        _emailService = emailService;
        _telemetry = telemetry;
        _mlPrediction = mlPrediction;
        _questionnaireContext = questionnaireContext;
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
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<Domain.Entities.BusinessPlan.BusinessPlan>(
                    Error.NotFound("BusinessPlan.Error.NotFound", $"Business plan with ID {businessPlanId} not found."));
            }

            // Check plan generation usage limit
            var usageCheck = await _featureGate.CheckUsageLimitAsync(
                businessPlan.OrganizationId, PlanFeatures.MaxAiGenerationsMonthly, cancellationToken);
            if (usageCheck.IsSuccess && !usageCheck.Value!.Allowed)
            {
                return Result.Failure<Domain.Entities.BusinessPlan.BusinessPlan>(
                    Error.Failure("Generation.LimitReached", usageCheck.Value.DenialReason
                        ?? "You have reached your monthly plan generation limit. Upgrade your plan to continue."));
            }

            // Validate status
            if (businessPlan.Status == BusinessPlanStatus.Draft)
            {
                var requiredQuestionIds = await _context.QuestionTemplates
                    .Where(qt => qt.IsActive && qt.IsRequired)
                    .Select(qt => qt.Id)
                    .ToListAsync(cancellationToken);

                if (requiredQuestionIds.Count > 0)
                {
                    var answeredCount = await _context.QuestionnaireResponses
                        .Where(qr => qr.BusinessPlanId == businessPlanId &&
                                     qr.QuestionTemplateId.HasValue &&
                                     requiredQuestionIds.Contains(qr.QuestionTemplateId.Value))
                        .CountAsync(cancellationToken);

                    if (answeredCount >= requiredQuestionIds.Count)
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

            var answersByQuestionNumber = _questionnaireContext.BuildAnswersDictionary(businessPlan.QuestionnaireResponses);

            // Detect org profile conflicts and prepare context note
            var orgIndustry = businessPlan.Organization?.Industry;
            var orgTeamSize = businessPlan.Organization?.TeamSize;
            var orgStage = businessPlan.Organization?.BusinessStage;
            var conflicts = OrgProfileConflictDetector.DetectConflicts(orgIndustry, orgTeamSize, orgStage, answersByQuestionNumber);
            if (conflicts.Count > 0)
            {
                _logger.LogWarning("Detected {Count} org profile conflicts for plan {PlanId}", conflicts.Count, businessPlanId);
            }
            var conflictNote = OrgProfileConflictDetector.FormatForPrompt(conflicts, language);

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
                    var refreshed = await _context.BusinessPlans.FirstAsync(bp => bp.Id == businessPlanId, cancellationToken);
                    briefJson = refreshed.BusinessBriefJson;
                }
                else
                {
                    _logger.LogWarning("Business Brief generation failed: {Error}", briefResult.Error?.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Business Brief generation threw exception");
            }

            // Hard prerequisite: fail if brief is missing and feature flag requires it
            if (string.IsNullOrWhiteSpace(briefJson))
            {
                var requireBrief = await _featureFlags.IsEnabledAsync("AI.RequireBusinessBrief", cancellationToken);
                if (requireBrief.IsSuccess && requireBrief.Value)
                {
                    _logger.LogError("Business Brief is required but generation failed for plan {PlanId}", businessPlanId);
                    businessPlan.UpdateGenerationProgress(null, 0);
                    await _context.SaveChangesAsync(cancellationToken);
                    return Result.Failure<Domain.Entities.BusinessPlan.BusinessPlan>(
                        Error.Failure("Pipeline.Error.BriefGenerationFailed",
                            "Business Brief generation failed. Cannot proceed without business context."));
                }

                _logger.LogWarning("Proceeding without Business Brief (feature flag disabled) for plan {PlanId}", businessPlanId);
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

            var tiers = SectionDependencyConfig.GetTiersForPlanType(businessPlan.PlanType);
            var availableSections = SectionDependencyConfig.GetAvailableSections(businessPlan.PlanType.ToString());
            var totalSections = availableSections.Count;
            var completedSections = 0;
            var generatedContent = new Dictionary<string, string>();

            foreach (var (tier, tierIndex) in tiers.Select((t, i) => (t, i)))
            {
                var tierSections = tier.Where(s => availableSections.Contains(s)).ToList();
                if (!tierSections.Any()) continue;

                _logger.LogInformation("Generating tier {TierIndex}/{TotalTiers} with {SectionCount} sections in parallel",
                    tierIndex + 1, tiers.Length, tierSections.Count);

                var completeness = businessPlan.CompletionPercentage > 0
                    ? (double)businessPlan.CompletionPercentage / 100.0
                    : (double)answersByQuestionNumber.Count / PipelineConstants.TotalQuestionnaireQuestions;

                // Pre-load all DB-dependent data sequentially to avoid DbContext concurrency issues
                var systemPrompt = await GetSystemPromptAsync(language, businessPlan.PlanType, cancellationToken);
                var preloadedData = new Dictionary<string, (string context, string userPrompt, List<LearnedPreferenceDto>? preferences)>();
                foreach (var section in tierSections)
                {
                    var (ctx, userPrompt, prefs) = await PreloadSectionDataAsync(
                        businessPlan.PlanType, section, answersByQuestionNumber,
                        briefJson, conflictNote, generatedContent, language,
                        completeness, cancellationToken);
                    preloadedData[section] = (ctx, userPrompt, prefs);
                }

                // Now run AI calls in parallel (no DB access needed)
                var tasks = tierSections.Select(section =>
                    GenerateSectionFromPreloadedAsync(
                        section, systemPrompt, preloadedData[section].userPrompt,
                        preloadedData[section].preferences, language, cancellationToken));

                var results = await Task.WhenAll(tasks);

                // Log telemetry sequentially (DbContext-safe)
                foreach (var (_, telemetry) in results)
                {
                    await _telemetry.LogCallAsync(telemetry, cancellationToken);
                }

                for (var i = 0; i < tierSections.Count; i++)
                {
                    var sectionName = tierSections[i];
                    var content = results[i].content;
                    generatedContent[sectionName] = content;
                    SetSectionContent(businessPlan, sectionName, content);

                    completedSections++;
                    var progress = 10 + (int)((decimal)completedSections / totalSections * 75);
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

                if (!string.IsNullOrWhiteSpace(qualityReport.SynthesizedExecutiveSummary))
                {
                    businessPlan.UpdateExecutiveSummary(qualityReport.SynthesizedExecutiveSummary);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }

            // LLM-as-Judge evaluation via Python service
            await RunLlmJudgeIfEnabledAsync(businessPlan, generatedContent, briefJson, language, cancellationToken);

            // Complete generation
            businessPlan.UpdateGenerationProgress(null, 100);
            businessPlan.CompleteGeneration();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Multi-pass pipeline completed for plan {PlanId}", businessPlanId);

            await SendCompletionEmailAsync(businessPlan, cancellationToken);

            // Record usage for the successful generation
            await _featureGate.RecordUsageAsync(
                businessPlan.OrganizationId, PlanFeatures.MaxAiGenerationsMonthly, 1, cancellationToken);

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

            var availableSections = SectionDependencyConfig.GetAvailableSections(businessPlan.PlanType.ToString());
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

            var result = await _aiService.GenerateContentWithMetadataAsync(
                systemPrompt, userPrompt.ToString(),
                maxTokens: PipelineConstants.AnalysisMaxTokens,
                temperature: PipelineConstants.AnalysisTemperature,
                maxRetries: PipelineConstants.ReducedMaxRetries, ct);

            await _telemetry.LogCallAsync(new AICallTelemetry(
                null, "primary", result.ModelUsed, result.InputTokens, result.OutputTokens,
                result.LatencyMs, null, language, PipelineConstants.PipelinePass.AnalysisPlan, DateTime.UtcNow,
                Temperature: PipelineConstants.AnalysisTemperature), ct);

            var json = ExtractJsonFromResponse(result.Content);

            // Validate generation plan
            var validation = AIResponseValidator.ValidateGenerationPlan(json);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Generation plan validation failed: {Errors}", string.Join("; ", validation.Errors));
                return null;
            }

            return json;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate analysis plan. Proceeding without it.");
            return null;
        }
    }

    #endregion

    #region Pass 2: Section Generation

    /// <summary>
    /// Pre-loads all DB-dependent data for a section sequentially (DbContext-safe).
    /// Returns (context, userPrompt, learnedPreferences).
    /// </summary>
    private async Task<(string context, string userPrompt, List<LearnedPreferenceDto>? preferences)> PreloadSectionDataAsync(
        BusinessPlanType planType,
        string sectionName,
        Dictionary<int, string> answers,
        string? briefJson,
        string? conflictNote,
        Dictionary<string, string> generatedContent,
        string language,
        double questionnaireCompleteness,
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

        if (!string.IsNullOrWhiteSpace(conflictNote))
        {
            context = conflictNote + "\n\n" + context;
        }

        var crossRefContext = BuildCrossReferenceContext(sectionName, generatedContent, language);
        if (!string.IsNullOrWhiteSpace(crossRefContext))
        {
            context += "\n\n" + crossRefContext;
        }

        // DB call: mapping enrichment
        try
        {
            var mappingContext = await _questionnaireContext.GetSectionMappingContextAsync(sectionName, language, ct);
            if (mappingContext != null)
            {
                context += FormatMappingEnrichment(mappingContext, answers, language);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "DB-driven mapping enrichment unavailable for {Section}", sectionName);
        }

        // DB call: section prompt
        var userPrompt = await GetSectionPromptAsync(planType, sectionName, context, language, ct);

        // DB call: ML preferences
        List<LearnedPreferenceDto>? preferences = null;
        try
        {
            var prefs = await _mlPrediction.GetLearnedPreferencesAsync(
                sectionName, language: language, cancellationToken: ct);
            if (prefs.Count > 0)
            {
                preferences = prefs;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "ML learned preferences unavailable for {Section}", sectionName);
        }

        return (context, userPrompt, preferences);
    }

    /// <summary>
    /// Generates a section using pre-loaded prompts (no DB access — safe for parallel execution).
    /// Returns (content, telemetryData) so telemetry can be logged sequentially after.
    /// </summary>
    private async Task<(string content, AICallTelemetry telemetry)> GenerateSectionFromPreloadedAsync(
        string sectionName,
        string systemPrompt,
        string userPrompt,
        List<LearnedPreferenceDto>? learnedPreferences,
        string language,
        CancellationToken ct)
    {
        if (learnedPreferences is { Count: > 0 })
        {
            userPrompt = AppendLearnedPreferences(userPrompt, learnedPreferences, language);
        }

        var temperature = SectionTemperatureConfig.GetTemperature(sectionName);

        var result = await _aiService.GenerateContentWithMetadataAsync(
            systemPrompt, userPrompt,
            maxTokens: PipelineConstants.SectionMaxTokens,
            temperature: temperature,
            maxRetries: PipelineConstants.DefaultMaxRetries, ct);

        var telemetry = new AICallTelemetry(
            null, "primary", result.ModelUsed, result.InputTokens, result.OutputTokens,
            result.LatencyMs, sectionName, language, PipelineConstants.PipelinePass.Section, DateTime.UtcNow,
            Temperature: temperature);

        var content = result.Content;

        var validation = AIResponseValidator.ValidateSectionContent(content, sectionName);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Section {Section} validation failed: {Errors}. Using content anyway.",
                sectionName, string.Join("; ", validation.Errors));
        }

        return (content, telemetry);
    }

    private async Task<string> GenerateSectionWithContextAsync(
        BusinessPlanType planType,
        string sectionName,
        Dictionary<int, string> answers,
        string? briefJson,
        string? conflictNote,
        Dictionary<string, string> generatedContent,
        string language,
        double questionnaireCompleteness,
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

        // Prepend conflict notes if any
        if (!string.IsNullOrWhiteSpace(conflictNote))
        {
            context = conflictNote + "\n\n" + context;
        }

        // Add cross-references with smart truncation
        var crossRefContext = BuildCrossReferenceContext(sectionName, generatedContent, language);
        if (!string.IsNullOrWhiteSpace(crossRefContext))
        {
            context += "\n\n" + crossRefContext;
        }

        // Enrich with DB-driven mapping weights and transformation hints
        try
        {
            var mappingContext = await _questionnaireContext.GetSectionMappingContextAsync(sectionName, language, ct);
            if (mappingContext != null)
            {
                context += FormatMappingEnrichment(mappingContext, answers, language);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "DB-driven mapping enrichment unavailable for {Section}", sectionName);
        }

        var systemPrompt = await GetSystemPromptAsync(language, planType, ct);
        var userPrompt = await GetSectionPromptAsync(planType, sectionName, context, language, ct);

        // Inject learned preferences from ML feedback loop
        try
        {
            var preferences = await _mlPrediction.GetLearnedPreferencesAsync(
                sectionName, language: language, cancellationToken: ct);

            if (preferences.Count > 0)
            {
                userPrompt = AppendLearnedPreferences(userPrompt, preferences, language);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "ML learned preferences unavailable for {Section}", sectionName);
        }

        // Context-aware temperature
        var temperature = SectionTemperatureConfig.GetTemperature(sectionName);

        var result = await _aiService.GenerateContentWithMetadataAsync(
            systemPrompt, userPrompt,
            maxTokens: PipelineConstants.SectionMaxTokens,
            temperature: temperature,
            maxRetries: PipelineConstants.DefaultMaxRetries, ct);

        await _telemetry.LogCallAsync(new AICallTelemetry(
            null, "primary", result.ModelUsed, result.InputTokens, result.OutputTokens,
            result.LatencyMs, sectionName, language, PipelineConstants.PipelinePass.Section, DateTime.UtcNow,
            Temperature: temperature), ct);

        var content = result.Content;

        // Validate section content
        var validation = AIResponseValidator.ValidateSectionContent(content, sectionName);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Section {Section} validation failed: {Errors}. Using content anyway.",
                sectionName, string.Join("; ", validation.Errors));
        }

        // Language detection — retry once if mismatch detected
        var langCheck = LanguageDetector.DetectLanguageMismatch(content, language);
        if (!langCheck.IsCorrectLanguage)
        {
            _logger.LogWarning("Language mismatch for {Section}: expected {Expected}, detected {Detected} (confidence {Confidence:F2}). Retrying.",
                sectionName, language, langCheck.DetectedLanguage, langCheck.Confidence);

            var isFr = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
            var langInstruction = isFr
                ? "CRITIQUE : Vous DEVEZ rédiger ENTIÈREMENT en français. Votre réponse précédente contenait du contenu en anglais.\n\n"
                : "CRITICAL: You MUST write entirely in English. Your previous response contained French content.\n\n";

            var retryResult = await _aiService.GenerateContentWithMetadataAsync(
                systemPrompt, langInstruction + userPrompt,
                maxTokens: PipelineConstants.SectionMaxTokens,
                temperature: temperature,
                maxRetries: PipelineConstants.ReducedMaxRetries, ct);

            await _telemetry.LogCallAsync(new AICallTelemetry(
                null, "primary", retryResult.ModelUsed, retryResult.InputTokens, retryResult.OutputTokens,
                retryResult.LatencyMs, sectionName, language, PipelineConstants.PipelinePass.LanguageRetry, DateTime.UtcNow,
                Temperature: temperature), ct);

            content = retryResult.Content;
        }

        // ML quality prediction — auto-regenerate if predicted score is too low
        try
        {
            var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            var prediction = await _mlPrediction.PredictQualityAsync(new QualityPredictionRequest(
                SectionType: sectionName,
                Industry: null,
                PlanType: planType.ToString(),
                Language: language,
                WordCount: wordCount,
                Temperature: temperature,
                Provider: result.ModelUsed ?? "unknown",
                Model: result.ModelUsed ?? "unknown",
                InputTokens: result.InputTokens,
                OutputTokens: result.OutputTokens,
                QuestionnaireCompleteness: questionnaireCompleteness,
                HasBusinessBrief: !string.IsNullOrWhiteSpace(briefJson)), ct);

            if (prediction.ShouldRegenerate && prediction.Confidence > PipelineConstants.MinRegenerationConfidence)
            {
                _logger.LogWarning(
                    "ML predicted low quality for {Section}: score={Score:F1}, reason={Reason}. Auto-regenerating.",
                    sectionName, prediction.PredictedScore, prediction.Reason);

                var regenTemp = Math.Max(PipelineConstants.MinRegenerationTemperature,
                    temperature - PipelineConstants.RegenerationTemperatureReduction);

                var regenResult = await _aiService.GenerateContentWithMetadataAsync(
                    systemPrompt, userPrompt,
                    maxTokens: PipelineConstants.SectionMaxTokens,
                    temperature: regenTemp,
                    maxRetries: PipelineConstants.ReducedMaxRetries, ct);

                await _telemetry.LogCallAsync(new AICallTelemetry(
                    null, "primary", regenResult.ModelUsed, regenResult.InputTokens, regenResult.OutputTokens,
                    regenResult.LatencyMs, sectionName, language, PipelineConstants.PipelinePass.MlRegeneration, DateTime.UtcNow,
                    Temperature: regenTemp), ct);

                content = regenResult.Content;
            }
        }
        catch (Exception ex)
        {
            // ML prediction is non-critical — never block generation
            _logger.LogDebug(ex, "ML quality prediction unavailable for {Section}", sectionName);
        }

        return content;
    }

    private static string? BuildCrossReferenceContext(
        string sectionName,
        Dictionary<string, string> generatedContent,
        string language)
    {
        // Use data-driven dependency config
        var crossRefs = SectionDependencyConfig.GetCrossReferences(sectionName, generatedContent.Keys);

        var isFrench = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
        var refs = new List<string>();

        foreach (var refSection in crossRefs)
        {
            if (generatedContent.TryGetValue(refSection, out var content) && !string.IsNullOrWhiteSpace(content))
            {
                // Smart truncation: keep relevant paragraphs instead of arbitrary Substring(0, 500)
                var truncated = SmartContextTruncator.TruncateWithRelevance(content, sectionName, 600, language);
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
                // Smart truncation: preserve topic sentences and numeric data
                var summarized = SmartContextTruncator.SummarizeForReview(content, 1500);
                userPrompt.AppendLine(summarized);
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

            var result = await _aiService.GenerateContentWithMetadataAsync(
                systemPrompt, userPrompt.ToString(),
                maxTokens: PipelineConstants.SectionMaxTokens,
                temperature: PipelineConstants.AnalysisTemperature,
                maxRetries: PipelineConstants.ReducedMaxRetries, ct);

            await _telemetry.LogCallAsync(new AICallTelemetry(
                null, "primary", result.ModelUsed, result.InputTokens, result.OutputTokens,
                result.LatencyMs, null, language, PipelineConstants.PipelinePass.Review, DateTime.UtcNow,
                Temperature: PipelineConstants.AnalysisTemperature), ct);

            var json = ExtractJsonFromResponse(result.Content);

            // Validate quality report
            var validation = AIResponseValidator.ValidateQualityReport(json);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Quality report validation failed: {Errors}. Attempting to deserialize anyway.",
                    string.Join("; ", validation.Errors));
            }

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
        var normalizedName = SectionNames.ToPascalCase(sectionName);

        try
        {
            var promptDto = await _aiPromptService.GetPromptBySectionAsync(
                normalizedName, planType.ToString(), language, "ContentGeneration", ct);

            if (promptDto != null)
            {
                var prompt = promptDto.UserPromptTemplate
                    .Replace("{sectionName}", sectionName)
                    .Replace("{questionnaireContext}", context)
                    .Replace("{context}", context);

                // Append rubrics and examples to DB-driven prompts too
                return AppendQualityEnhancements(prompt, sectionName, language);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load section prompt for {Section}", sectionName);
        }

        // Fallback — language-aware, enriched with rubrics and examples
        var isFr = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
        var rubric = SectionRubrics.GetRubric(sectionName, language);
        var wordRange = rubric != null
            ? $"{rubric.MinWordCount}-{rubric.MaxWordCount}"
            : "400-800";

        var basePrompt = isFr
            ? $"Rédigez la section « {sectionName} » pour ce plan d'affaires en vous basant sur le contexte suivant.\nRépondez UNIQUEMENT en français.\n\n{context}\n\nRédigez une section professionnelle et convaincante de {wordRange} mots."
            : $"Generate the {sectionName} section for this business plan based on the following context:\n\n{context}\n\nWrite a comprehensive, professional section of {wordRange} words.";

        return AppendQualityEnhancements(basePrompt, sectionName, language);
    }

    /// <summary>
    /// Appends rubrics and few-shot examples to any prompt (DB-driven or fallback).
    /// </summary>
    private static string AppendQualityEnhancements(string prompt, string sectionName, string language)
    {
        var rubricBlock = SectionRubrics.FormatForPrompt(sectionName, language);
        var examplesBlock = FewShotExamples.FormatForPrompt(sectionName, language);

        var sb = new StringBuilder(prompt);
        if (rubricBlock != null)
        {
            sb.AppendLine();
            sb.AppendLine();
            sb.Append(rubricBlock);
        }
        if (examplesBlock != null)
        {
            sb.AppendLine();
            sb.AppendLine();
            sb.Append(examplesBlock);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Formats DB-driven QuestionSectionMapping enrichment (weights and transformation hints)
    /// as additional context for the AI prompt.
    /// </summary>
    private static string FormatMappingEnrichment(
        SectionMappingContext mappingContext,
        Dictionary<int, string> answers,
        string language)
    {
        var isFr = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
        var sb = new StringBuilder();

        var hintsWithAnswers = mappingContext.TransformationHints
            .Where(kv => answers.ContainsKey(kv.Key))
            .ToList();

        if (hintsWithAnswers.Count == 0) return string.Empty;

        sb.AppendLine();
        sb.AppendLine(isFr
            ? "=== INSTRUCTIONS DE TRANSFORMATION (basées sur le mappage des questions) ==="
            : "=== TRANSFORMATION INSTRUCTIONS (based on question mapping) ===");

        foreach (var (questionNum, hint) in hintsWithAnswers.OrderByDescending(kv =>
            mappingContext.Weights.GetValueOrDefault(kv.Key, 1.0m)))
        {
            var weight = mappingContext.Weights.GetValueOrDefault(questionNum, 1.0m);
            var priority = weight >= 0.8m
                ? (isFr ? "PRIORITAIRE" : "HIGH PRIORITY")
                : weight >= 0.5m
                    ? (isFr ? "Important" : "Important")
                    : (isFr ? "Contexte" : "Context");

            sb.AppendLine($"- Q{questionNum} [{priority}]: {hint}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Appends ML-learned user preferences to the prompt so the AI adapts its output style.
    /// </summary>
    private static string AppendLearnedPreferences(
        string prompt,
        List<LearnedPreferenceDto> preferences,
        string language)
    {
        var isFr = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
        var sb = new StringBuilder(prompt);

        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine(isFr
            ? "=== PRÉFÉRENCES APPRISES (basées sur les modifications des utilisateurs) ==="
            : "=== LEARNED PREFERENCES (based on user edit patterns) ===");

        foreach (var pref in preferences.Where(p =>
            p.Confidence >= PipelineConstants.MinPreferenceConfidence &&
            p.SampleCount >= PipelineConstants.MinPreferenceSamples))
        {
            sb.AppendLine($"- [{pref.PreferenceType}] {pref.PreferenceJson} (confidence: {pref.Confidence:F1}, samples: {pref.SampleCount})");
        }

        sb.AppendLine(isFr
            ? "Adaptez votre contenu en tenant compte de ces préférences."
            : "Adapt your content taking these preferences into account.");

        return sb.ToString();
    }

    private void SetSectionContent(Domain.Entities.BusinessPlan.BusinessPlan businessPlan, string sectionName, string content)
    {
        var normalizedName = SectionNames.ToPascalCase(sectionName);
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
