using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Content;
using Sqordia.Contracts.Responses.Content;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Implementation of enhanced content generation service
/// Integrates Prompt Repository System with AI content generation for visual elements
/// </summary>
public class EnhancedContentGenerationService : IEnhancedContentGenerationService
{
    private readonly IApplicationDbContext _context;
    private readonly IPromptRepository _promptRepository;
    private readonly IPromptSelectorService _promptSelector;
    private readonly IAIService _aiService;
    private readonly ILogger<EnhancedContentGenerationService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public EnhancedContentGenerationService(
        IApplicationDbContext context,
        IPromptRepository promptRepository,
        IPromptSelectorService promptSelector,
        IAIService aiService,
        ILogger<EnhancedContentGenerationService> logger)
    {
        _context = context;
        _promptRepository = promptRepository;
        _promptSelector = promptSelector;
        _aiService = aiService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<EnhancedSectionContentResponse>> GenerateSectionAsync(
        Guid businessPlanId,
        SectionType sectionType,
        GenerationOptionsDto? options = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        options ??= new GenerationOptionsDto();

        try
        {
            _logger.LogInformation(
                "Starting enhanced content generation for BusinessPlan: {BusinessPlanId}, Section: {SectionType}",
                businessPlanId,
                sectionType);

            // 1. Load business plan with context
            var businessPlan = await _context.BusinessPlans
                .Include(bp => bp.QuestionnaireResponses)
                    .ThenInclude(qr => qr.QuestionTemplate)
                .Include(bp => bp.QuestionnaireResponses)
                    .ThenInclude(qr => qr.QuestionTemplateV2)
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<EnhancedSectionContentResponse>(
                    Error.NotFound("BusinessPlan.NotFound", $"Business plan with ID {businessPlanId} not found"));
            }

            // 2. Select prompt template
            var promptContext = new PromptSelectionContext
            {
                SectionType = sectionType,
                PlanType = businessPlan.PlanType,
                IndustryCategory = null, // Industry extracted from questionnaire responses
                Language = options.Language
            };

            // Try to get by alias if specified
            Result<PromptTemplate> promptResult;
            if (!string.IsNullOrEmpty(options.PreferredAlias) &&
                Enum.TryParse<PromptAlias>(options.PreferredAlias, true, out var alias))
            {
                promptResult = await _promptSelector.GetPromptByAliasAsync(promptContext, alias, cancellationToken);
            }
            else
            {
                promptResult = await _promptSelector.SelectPromptAsync(promptContext, cancellationToken);
            }

            // If no prompt template found, use fallback generation
            PromptTemplate? template = null;
            bool usingFallback = false;

            if (promptResult.IsFailure)
            {
                _logger.LogWarning(
                    "No prompt template found for {SectionType}, using fallback generation",
                    sectionType);
                usingFallback = true;
            }
            else
            {
                template = promptResult.Value;
            }

            // 3. Build variable dictionary from plan context
            var variables = BuildVariables(businessPlan, options.AdditionalVariables);

            // 4. Build the final prompt
            string systemPrompt;
            string userPrompt;

            if (template != null)
            {
                systemPrompt = template.SystemPrompt;
                userPrompt = _promptSelector.BuildPrompt(template, variables);
            }
            else
            {
                // Fallback to basic prompts
                systemPrompt = GetFallbackSystemPrompt(options.Language);
                userPrompt = GetFallbackUserPrompt(sectionType, variables, options);
            }

            // Add structured output instructions if visuals are requested
            if (options.IncludeVisualElements)
            {
                userPrompt = AppendStructuredOutputInstructions(userPrompt, sectionType);
            }

            // 5. Call AI service
            var aiResponse = await _aiService.GenerateContentWithRetryAsync(
                systemPrompt,
                userPrompt,
                maxTokens: 4000,
                temperature: 0.7f,
                maxRetries: 3,
                cancellationToken);

            stopwatch.Stop();

            // 6. Parse the response
            var content = ParseGeneratedContent(aiResponse, sectionType, options.IncludeVisualElements);

            // 7. Record usage if using a template
            if (template != null)
            {
                await _promptRepository.RecordUsageAsync(
                    template.Id,
                    UsageType.Generated,
                    null,
                    cancellationToken);
            }

            // 8. Build response with metadata
            var response = new EnhancedSectionContentResponse
            {
                SectionType = sectionType.ToString(),
                Version = "1.0",
                Content = content,
                Metadata = new GenerationMetadataDto
                {
                    PromptVersion = template?.Version ?? 0,
                    PromptAlias = template?.Alias?.ToString() ?? (usingFallback ? "fallback" : "default"),
                    ModelUsed = "gpt-4o",
                    GenerationTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    TokensUsed = EstimateTokens(aiResponse),
                    IncludesVisuals = content.VisualElements.Count > 0,
                    GeneratedAt = DateTime.UtcNow
                }
            };

            _logger.LogInformation(
                "Enhanced content generation completed for {SectionType} in {ElapsedMs}ms with {VisualCount} visual elements",
                sectionType,
                stopwatch.ElapsedMilliseconds,
                content.VisualElements.Count);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error generating enhanced content for BusinessPlan: {BusinessPlanId}, Section: {SectionType}",
                businessPlanId,
                sectionType);

            return Result.Failure<EnhancedSectionContentResponse>(
                Error.Failure("ContentGeneration.Error", $"Failed to generate content: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<Result<EnhancedSectionContentResponse>> RegenerateSectionAsync(
        Guid businessPlanId,
        SectionType sectionType,
        string? feedback = null,
        GenerationOptionsDto? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new GenerationOptionsDto();

        // Add feedback to additional variables if provided
        if (!string.IsNullOrEmpty(feedback))
        {
            options.AdditionalVariables["userFeedback"] = feedback;
            options.AdditionalVariables["regenerationContext"] =
                "This is a regeneration request. The user provided feedback to improve the previous version.";
        }

        _logger.LogInformation(
            "Regenerating section {SectionType} for BusinessPlan: {BusinessPlanId}",
            sectionType,
            businessPlanId);

        return await GenerateSectionAsync(businessPlanId, sectionType, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<EnhancedSectionContentResponse>> ImproveSectionAsync(
        Guid businessPlanId,
        SectionType sectionType,
        string currentContent,
        ImprovementType improvementType,
        string? customPrompt = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Improving section {SectionType} with {ImprovementType} for BusinessPlan: {BusinessPlanId}",
                sectionType,
                improvementType,
                businessPlanId);

            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<EnhancedSectionContentResponse>(
                    Error.NotFound("BusinessPlan.NotFound", $"Business plan with ID {businessPlanId} not found"));
            }

            var systemPrompt = GetImprovementSystemPrompt(improvementType);
            var userPrompt = BuildImprovementPrompt(currentContent, improvementType, customPrompt);

            var aiResponse = await _aiService.GenerateContentWithRetryAsync(
                systemPrompt,
                userPrompt,
                maxTokens: 4000,
                temperature: 0.5f,
                maxRetries: 3,
                cancellationToken);

            stopwatch.Stop();

            var content = ParseGeneratedContent(aiResponse, sectionType, true);

            var response = new EnhancedSectionContentResponse
            {
                SectionType = sectionType.ToString(),
                Version = "1.0",
                Content = content,
                Metadata = new GenerationMetadataDto
                {
                    PromptVersion = 0,
                    PromptAlias = $"improvement-{improvementType.ToString().ToLower()}",
                    ModelUsed = "gpt-4o",
                    GenerationTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    TokensUsed = EstimateTokens(aiResponse),
                    IncludesVisuals = content.VisualElements.Count > 0,
                    GeneratedAt = DateTime.UtcNow
                }
            };

            _logger.LogInformation(
                "Section improvement completed for {SectionType} in {ElapsedMs}ms",
                sectionType,
                stopwatch.ElapsedMilliseconds);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error improving section {SectionType} for BusinessPlan: {BusinessPlanId}",
                sectionType,
                businessPlanId);

            return Result.Failure<EnhancedSectionContentResponse>(
                Error.Failure("ContentGeneration.ImprovementError", $"Failed to improve content: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task RecordUsageFeedbackAsync(
        Guid promptId,
        UsageType usageType,
        int? rating = null,
        CancellationToken cancellationToken = default)
    {
        await _promptRepository.RecordUsageAsync(promptId, usageType, rating, cancellationToken);

        _logger.LogDebug(
            "Recorded usage feedback for prompt {PromptId}: {UsageType}, Rating: {Rating}",
            promptId,
            usageType,
            rating);
    }

    /// <inheritdoc />
    public SectionCapabilities GetSectionCapabilities(SectionType sectionType, BusinessPlanType planType)
    {
        return sectionType switch
        {
            SectionType.ExecutiveSummary => new SectionCapabilities
            {
                SectionType = sectionType,
                RecommendedVisuals = new List<string> { "metric" },
                RequiredVisuals = new List<string>(),
                OptionalVisuals = new List<string> { "chart", "table" },
                SupportsStructuredOutput = true,
                RecommendedWordCount = 500,
                DefaultTone = "compelling"
            },
            SectionType.MarketAnalysis => new SectionCapabilities
            {
                SectionType = sectionType,
                RecommendedVisuals = new List<string> { "chart", "table", "metric" },
                RequiredVisuals = new List<string> { "chart" },
                OptionalVisuals = new List<string> { "infographic" },
                SupportsStructuredOutput = true,
                RecommendedWordCount = 800,
                DefaultTone = "analytical"
            },
            SectionType.FinancialProjections => new SectionCapabilities
            {
                SectionType = sectionType,
                RecommendedVisuals = new List<string> { "chart", "table", "metric" },
                RequiredVisuals = new List<string> { "table", "chart" },
                OptionalVisuals = new List<string>(),
                SupportsStructuredOutput = true,
                RecommendedWordCount = 600,
                DefaultTone = "precise"
            },
            SectionType.SWOTAnalysis => new SectionCapabilities
            {
                SectionType = sectionType,
                RecommendedVisuals = new List<string> { "table" },
                RequiredVisuals = new List<string> { "table" },
                OptionalVisuals = new List<string> { "infographic" },
                SupportsStructuredOutput = true,
                RecommendedWordCount = 400,
                DefaultTone = "strategic"
            },
            SectionType.ProductsServices => new SectionCapabilities
            {
                SectionType = sectionType,
                RecommendedVisuals = new List<string> { "table", "infographic" },
                RequiredVisuals = new List<string>(),
                OptionalVisuals = new List<string> { "chart", "metric" },
                SupportsStructuredOutput = true,
                RecommendedWordCount = 600,
                DefaultTone = "descriptive"
            },
            SectionType.MarketingStrategy => new SectionCapabilities
            {
                SectionType = sectionType,
                RecommendedVisuals = new List<string> { "chart", "table" },
                RequiredVisuals = new List<string>(),
                OptionalVisuals = new List<string> { "infographic", "metric" },
                SupportsStructuredOutput = true,
                RecommendedWordCount = 700,
                DefaultTone = "action-oriented"
            },
            SectionType.OperationsPlan => new SectionCapabilities
            {
                SectionType = sectionType,
                RecommendedVisuals = new List<string> { "table", "infographic" },
                RequiredVisuals = new List<string>(),
                OptionalVisuals = new List<string> { "chart" },
                SupportsStructuredOutput = true,
                RecommendedWordCount = 600,
                DefaultTone = "practical"
            },
            SectionType.ManagementTeam => new SectionCapabilities
            {
                SectionType = sectionType,
                RecommendedVisuals = new List<string> { "table" },
                RequiredVisuals = new List<string>(),
                OptionalVisuals = new List<string> { "infographic" },
                SupportsStructuredOutput = true,
                RecommendedWordCount = 500,
                DefaultTone = "professional"
            },
            _ => new SectionCapabilities
            {
                SectionType = sectionType,
                RecommendedVisuals = new List<string>(),
                RequiredVisuals = new List<string>(),
                OptionalVisuals = new List<string> { "table", "chart", "metric", "infographic" },
                SupportsStructuredOutput = true,
                RecommendedWordCount = 500,
                DefaultTone = "professional"
            }
        };
    }

    #region Private Helper Methods

    private Dictionary<string, string> BuildVariables(BusinessPlan plan, Dictionary<string, string>? additionalVars)
    {
        var variables = new Dictionary<string, string>
        {
            { "companyName", plan.Title ?? "the company" },
            { "planType", plan.PlanType.ToString() },
            { "industry", "general" }, // Extracted from questionnaire responses
            { "targetMarket", "" }, // Extracted from questionnaire responses
            { "products", "" }, // Extracted from questionnaire responses
            { "description", plan.Description ?? "" },
            { "businessPlanTitle", plan.Title ?? "" }
        };

        // Extract additional context from questionnaire responses
        if (plan.QuestionnaireResponses?.Any() == true)
        {
            var questionnaireContext = BuildQuestionnaireContext(plan.QuestionnaireResponses);
            variables["questionnaireContext"] = questionnaireContext;

            // Try to extract key values from responses
            ExtractKeyVariablesFromResponses(plan.QuestionnaireResponses, variables);
        }

        // Merge additional variables
        if (additionalVars != null)
        {
            foreach (var (key, value) in additionalVars)
            {
                variables[key] = value;
            }
        }

        return variables;
    }

    private void ExtractKeyVariablesFromResponses(ICollection<QuestionnaireResponse> responses, Dictionary<string, string> variables)
    {
        // Keywords to look for in questions to extract values
        var keywordMappings = new Dictionary<string, string[]>
        {
            { "companyName", new[] { "company name", "business name", "nom de l'entreprise", "nom de la compagnie" } },
            { "industry", new[] { "industry", "sector", "industrie", "secteur" } },
            { "targetMarket", new[] { "target market", "target customers", "marché cible", "clientèle cible" } },
            { "products", new[] { "products", "services", "produits", "offerings" } }
        };

        foreach (var response in responses)
        {
            var questionText = (response.QuestionTemplate?.QuestionText
                ?? response.QuestionTemplateV2?.QuestionText
                ?? "").ToLowerInvariant();

            var answerText = response.ResponseText ?? response.NumericValue?.ToString() ?? "";

            if (string.IsNullOrEmpty(answerText)) continue;

            foreach (var (variableName, keywords) in keywordMappings)
            {
                if (keywords.Any(keyword => questionText.Contains(keyword)))
                {
                    // Only update if current value is empty or default
                    if (string.IsNullOrEmpty(variables[variableName]) ||
                        variables[variableName] == "general" ||
                        variables[variableName] == "the company")
                    {
                        variables[variableName] = TruncateText(answerText, 200) ?? "";
                    }
                }
            }
        }
    }

    private string BuildQuestionnaireContext(ICollection<QuestionnaireResponse> responses)
    {
        var sb = new StringBuilder();

        var validResponses = responses
            .Where(r => r.QuestionTemplate != null || r.QuestionTemplateV2 != null)
            .OrderBy(r => r.QuestionTemplate?.Order ?? r.QuestionTemplateV2?.Order ?? 0)
            .Take(20) // Limit to prevent token overflow
            .ToList();

        foreach (var response in validResponses)
        {
            var questionText = response.QuestionTemplate?.QuestionText
                ?? response.QuestionTemplateV2?.QuestionText
                ?? "Question";

            var answer = response.ResponseText ?? response.NumericValue?.ToString() ?? "";

            if (!string.IsNullOrEmpty(answer))
            {
                sb.AppendLine($"Q: {TruncateText(questionText, 100)}");
                sb.AppendLine($"A: {TruncateText(answer, 300)}");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static string TruncateText(string? text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
    }

    private ContentDataDto ParseGeneratedContent(string aiResponse, SectionType sectionType, bool expectStructured)
    {
        if (!expectStructured)
        {
            return CreateProseOnlyContent(aiResponse);
        }

        // Try to parse as JSON first
        try
        {
            // Look for JSON in the response
            var jsonStart = aiResponse.IndexOf('{');
            var jsonEnd = aiResponse.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var parsed = JsonSerializer.Deserialize<ContentDataDto>(jsonContent, JsonOptions);

                if (parsed != null && (parsed.Prose.Count > 0 || parsed.VisualElements.Count > 0))
                {
                    _logger.LogDebug("Successfully parsed structured content for {SectionType}", sectionType);
                    return parsed;
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse JSON response for {SectionType}, falling back to prose", sectionType);
        }

        // Fallback to prose-only
        return CreateProseOnlyContent(aiResponse);
    }

    private static ContentDataDto CreateProseOnlyContent(string content)
    {
        return new ContentDataDto
        {
            Prose = new List<ProsePartDto>
            {
                new ProsePartDto
                {
                    Id = "main-content",
                    Content = content,
                    Position = 1
                }
            },
            VisualElements = new List<VisualElementDto>(),
            KeyMetrics = new List<MetricDto>(),
            Highlights = new List<string>()
        };
    }

    private string AppendStructuredOutputInstructions(string prompt, SectionType sectionType)
    {
        var capabilities = GetSectionCapabilities(sectionType, BusinessPlanType.BusinessPlan);

        var instructions = new StringBuilder();
        instructions.AppendLine();
        instructions.AppendLine("=== OUTPUT FORMAT ===");
        instructions.AppendLine("Return your response as a valid JSON object with this structure:");
        instructions.AppendLine(@"{
  ""prose"": [
    { ""id"": ""unique-id"", ""content"": ""<p>HTML paragraph content</p>"", ""position"": 1, ""visualAfter"": ""chart-id"" }
  ],
  ""visualElements"": [
    { ""id"": ""chart-id"", ""type"": ""chart"", ""title"": ""Chart Title"", ""data"": { ... } }
  ],
  ""keyMetrics"": [
    { ""label"": ""Metric Name"", ""value"": 12345, ""format"": ""currency"" }
  ],
  ""highlights"": [""Key point 1"", ""Key point 2""]
}");
        instructions.AppendLine();
        instructions.AppendLine($"Recommended visual elements for this section: {string.Join(", ", capabilities.RecommendedVisuals)}");

        if (capabilities.RequiredVisuals.Count > 0)
        {
            instructions.AppendLine($"Required visual elements: {string.Join(", ", capabilities.RequiredVisuals)}");
        }

        instructions.AppendLine(@"
Visual element data formats:
- Chart: { ""chartType"": ""line|bar|pie"", ""labels"": [""Label1"", ""Label2""], ""datasets"": [{ ""label"": ""Series"", ""data"": [10, 20, 30] }] }
- Table: { ""tableType"": ""comparison|financial|custom"", ""headers"": [""Col1"", ""Col2""], ""rows"": [{ ""cells"": [{ ""value"": ""Cell"" }] }] }
- Metric: { ""metrics"": [{ ""label"": ""TAM"", ""value"": 1000000, ""format"": ""currency"", ""trend"": ""up"" }], ""layout"": ""row"" }

Generate realistic data that matches the business context.");

        return prompt + instructions.ToString();
    }

    private string GetFallbackSystemPrompt(string language)
    {
        return language.ToLower() == "en"
            ? @"You are an expert business plan consultant with 20 years of experience. Generate professional, comprehensive business plan content with:
1. Clear, compelling prose in HTML format
2. Relevant visual elements (charts, tables, metrics) where appropriate
3. Data-driven insights and actionable recommendations

Always aim for clarity, professionalism, and persuasiveness."
            : @"Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience. Générez un contenu de plan d'affaires professionnel et complet avec :
1. Une prose claire et convaincante au format HTML
2. Des éléments visuels pertinents (graphiques, tableaux, métriques) le cas échéant
3. Des informations basées sur les données et des recommandations actionnables

Visez toujours la clarté, le professionnalisme et la persuasion.";
    }

    private string GetFallbackUserPrompt(SectionType sectionType, Dictionary<string, string> variables, GenerationOptionsDto options)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Generate a {sectionType} section for the following business plan:");
        sb.AppendLine();

        if (variables.TryGetValue("companyName", out var companyName))
            sb.AppendLine($"Company: {companyName}");

        if (variables.TryGetValue("industry", out var industry))
            sb.AppendLine($"Industry: {industry}");

        if (variables.TryGetValue("description", out var description) && !string.IsNullOrEmpty(description))
            sb.AppendLine($"Description: {description}");

        if (variables.TryGetValue("targetMarket", out var targetMarket) && !string.IsNullOrEmpty(targetMarket))
            sb.AppendLine($"Target Market: {targetMarket}");

        if (variables.TryGetValue("products", out var products) && !string.IsNullOrEmpty(products))
            sb.AppendLine($"Products/Services: {products}");

        if (variables.TryGetValue("questionnaireContext", out var context) && !string.IsNullOrEmpty(context))
        {
            sb.AppendLine();
            sb.AppendLine("=== QUESTIONNAIRE RESPONSES ===");
            sb.AppendLine(context);
        }

        sb.AppendLine();
        sb.AppendLine($"Generate a comprehensive, professional {sectionType} section.");
        sb.AppendLine("Aim for 500-700 words of engaging prose with relevant data points.");

        return sb.ToString();
    }

    private string GetImprovementSystemPrompt(ImprovementType improvementType)
    {
        return improvementType switch
        {
            ImprovementType.Enhance => @"You are an expert editor improving business plan content. Enhance the quality, clarity, and impact of the provided content while maintaining its core message. Add relevant data points and visual elements.",

            ImprovementType.Expand => @"You are an expert business writer expanding content. Add more depth, detail, and supporting data to the provided content. Include additional visual elements to illustrate key points.",

            ImprovementType.Simplify => @"You are a communication expert simplifying business content. Make the language clearer, remove jargon, and improve readability while preserving key information. Keep visual elements simple and intuitive.",

            ImprovementType.Professionalize => @"You are a senior business consultant. Transform the content into highly professional, investor-ready language. Ensure the tone is confident, data-driven, and compelling.",

            ImprovementType.AddData => @"You are a business analyst enhancing content with data. Add relevant statistics, market data, financial figures, and industry benchmarks. Include charts and tables to visualize the data.",

            ImprovementType.EnhanceVisuals => @"You are a data visualization expert. Focus on adding or improving visual elements (charts, tables, infographics) to better communicate the information. Each visual should tell a story.",

            _ => @"You are an expert business plan consultant improving content quality. Enhance the provided content while maintaining its core message and adding relevant visual elements."
        };
    }

    private string BuildImprovementPrompt(string currentContent, ImprovementType improvementType, string? customPrompt)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== CURRENT CONTENT ===");
        sb.AppendLine(currentContent);
        sb.AppendLine();
        sb.AppendLine("=== IMPROVEMENT REQUEST ===");
        sb.AppendLine($"Improvement type: {improvementType}");

        if (!string.IsNullOrEmpty(customPrompt))
        {
            sb.AppendLine($"Specific instructions: {customPrompt}");
        }

        sb.AppendLine();
        sb.AppendLine("Please improve the content according to the specified type. Return the improved content in the structured JSON format with prose and visual elements.");

        return sb.ToString();
    }

    private static int EstimateTokens(string text)
    {
        // Rough estimate: ~4 characters per token
        return text.Length / 4;
    }

    #endregion
}
