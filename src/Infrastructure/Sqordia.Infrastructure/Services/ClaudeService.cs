using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Anthropic;
using Anthropic.Models.Messages;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Contracts.Requests.Questionnaire;
using Sqordia.Contracts.Responses.Questionnaire;
using Sqordia.Contracts.Requests.Sections;
using Sqordia.Contracts.Responses.Sections;
using Sqordia.Contracts.Requests.AI;
using Sqordia.Contracts.Responses.AI;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Sqordia.Infrastructure.Services;

public class ClaudeSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4-6";
    public string HeavyModel { get; set; } = "claude-opus-4-6";
    public int MaxTokens { get; set; } = 4000;
    public bool EnableExtendedThinking { get; set; } = false;
    public int ExtendedThinkingBudgetTokens { get; set; } = 8000;
    public bool EnablePromptCaching { get; set; } = true;
}

public class ClaudeService : IAIService
{
    private readonly ILogger<ClaudeService> _logger;
    private readonly ClaudeSettings _settings;
    private readonly AnthropicClient? _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ClaudeService(
        IOptions<ClaudeSettings> settings,
        ILogger<ClaudeService> logger)
    {
        _logger = logger;
        _settings = settings.Value;

        _logger.LogInformation("Claude Settings - ApiKey configured: {HasKey}, Model: {Model}",
            !string.IsNullOrEmpty(_settings.ApiKey), _settings.Model);

        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            try
            {
                _logger.LogInformation("Initializing Claude client with model: {Model}", _settings.Model);
                _client = new AnthropicClient(new Anthropic.Core.ClientOptions
                {
                    ApiKey = _settings.ApiKey
                });
                _logger.LogInformation("Claude service initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Claude client");
                _client = null;
            }
        }
        else
        {
            _logger.LogWarning("Claude API key not configured");
        }
    }

    #region Core Methods (Phase 1)

    public async Task<string> GenerateContentAsync(
        string systemPrompt,
        string userPrompt,
        int maxTokens = 2000,
        float temperature = 0.7f,
        CancellationToken cancellationToken = default)
    {
        EnsureClientConfigured();

        try
        {
            _logger.LogInformation("Generating content with Claude");

            var parameters = new MessageCreateParams
            {
                Model = _settings.Model,
                MaxTokens = maxTokens,
                Temperature = temperature,
                System = BuildSystemParam(systemPrompt),
                Messages = new List<MessageParam>
                {
                    new() { Role = Role.User, Content = userPrompt }
                }
            };

            var response = await _client!.Messages.Create(parameters);
            var content = ExtractTextContent(response);

            LogCacheMetrics(response);
            _logger.LogInformation("Content generated successfully ({OutputTokens} tokens)",
                response.Usage.OutputTokens);
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating content with Claude");
            throw;
        }
    }

    public Task<string> GenerateContentWithRetryAsync(
        string systemPrompt,
        string userPrompt,
        int maxTokens = 2000,
        float temperature = 0.7f,
        int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        // Official SDK handles retries via MaxRetries on the client constructor
        return GenerateContentAsync(systemPrompt, userPrompt, maxTokens, temperature, cancellationToken);
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (_client == null)
        {
            _logger.LogWarning("Claude service unavailable: client is null");
            return false;
        }

        try
        {
            // Ping the API with a minimal request
            var parameters = new MessageCreateParams
            {
                Model = _settings.Model,
                MaxTokens = 10,
                Messages = new List<MessageParam>
                {
                    new() { Role = Role.User, Content = "ping" }
                }
            };

            await _client.Messages.Create(parameters);
            _logger.LogDebug("Claude service is available");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Claude service availability check failed");
            return false;
        }
    }

    public async Task<(string Content, int TokenCount)> GenerateChatResponseAsync(
        string systemPrompt,
        List<AIChatMessage> conversationHistory,
        int maxTokens = 2000,
        CancellationToken cancellationToken = default)
    {
        EnsureClientConfigured();

        try
        {
            _logger.LogInformation("Generating chat response with {MessageCount} messages in history",
                conversationHistory.Count);

            var messages = conversationHistory.Select(m => new MessageParam
            {
                Role = m.Role.ToLowerInvariant() == "user" ? Role.User : Role.Assistant,
                Content = m.Content
            }).ToList();

            var parameters = new MessageCreateParams
            {
                Model = _settings.Model,
                Messages = messages,
                MaxTokens = maxTokens,
                Temperature = 0.7,
                System = BuildSystemParam(systemPrompt)
            };

            var response = await _client!.Messages.Create(parameters);
            var content = ExtractTextContent(response);
            var tokenCount = (int)response.Usage.OutputTokens;

            LogCacheMetrics(response);
            _logger.LogInformation("Chat response generated successfully with {TokenCount} tokens", tokenCount);
            return (content, tokenCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chat response with Claude");
            throw;
        }
    }

    #endregion

    #region Streaming (Phase 5)

    public async IAsyncEnumerable<string> StreamChatResponseAsync(
        string systemPrompt,
        List<AIChatMessage> conversationHistory,
        int maxTokens = 2000,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        EnsureClientConfigured();

        _logger.LogInformation("Streaming chat response with {MessageCount} messages", conversationHistory.Count);

        var messages = conversationHistory.Select(m => new MessageParam
        {
            Role = m.Role.ToLowerInvariant() == "user" ? Role.User : Role.Assistant,
            Content = m.Content
        }).ToList();

        var parameters = new MessageCreateParams
        {
            Model = _settings.Model,
            Messages = messages,
            MaxTokens = maxTokens,
            Temperature = 0.7,
            System = BuildSystemParam(systemPrompt)
        };

        await foreach (var rawEvent in _client!.Messages.CreateStreaming(parameters)
                            .WithCancellation(cancellationToken))
        {
            if (rawEvent.TryPickContentBlockDelta(out var delta))
            {
                if (delta.Delta.TryPickText(out var textDelta))
                {
                    yield return textDelta.Text;
                }
            }
        }
    }

    #endregion

    #region Structured Output Methods (Phase 2) + Extended Thinking (Phase 3)

    public async Task<QuestionSuggestionResponse> GenerateQuestionSuggestionsAsync(
        QuestionSuggestionRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureClientConfigured();

        try
        {
            _logger.LogInformation("Generating question suggestions");

            var systemPrompt = @"You are an expert business consultant helping with questionnaire responses for business plan creation.
You MUST respond with valid JSON matching this exact structure:
{
  ""suggestions"": [
    {
      ""answer"": ""string: the suggested answer"",
      ""confidence"": 0.85,
      ""reasoning"": ""string: why this suggestion is relevant"",
      ""suggestionType"": ""Detailed""
    }
  ]
}
Generate 3-5 helpful, specific suggestions. Only output valid JSON, no other text.";

            var userPrompt = $"Question: {request.QuestionText}\nPlan Type: {request.PlanType}\nLanguage: {request.Language}";

            var content = await GenerateContentAsync(systemPrompt, userPrompt, 2000, 0.8f, cancellationToken);
            var parsed = ParseJson<QuestionSuggestionStructuredOutput>(content);

            return new QuestionSuggestionResponse
            {
                QuestionText = request.QuestionText,
                PlanType = request.PlanType,
                Suggestions = parsed?.Suggestions?.Select(s => new QuestionSuggestion
                {
                    Answer = s.Answer ?? string.Empty,
                    Confidence = s.Confidence,
                    Reasoning = s.Reasoning ?? "Generated by Claude",
                    SuggestionType = s.SuggestionType ?? "Detailed"
                }).ToList() ?? new List<QuestionSuggestion>
                {
                    new() { Answer = content, Confidence = 0.8, Reasoning = "Generated by Claude", SuggestionType = "Detailed" }
                },
                GeneratedAt = DateTime.UtcNow,
                Model = _settings.Model,
                Language = request.Language
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating question suggestions");
            throw;
        }
    }

    public async Task<SectionImprovementResponse> ImproveSectionAsync(
        SectionImprovementRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureClientConfigured();

        try
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("Improving section");

            var systemPrompt = @"You are an expert business consultant. Improve the content to make it more professional and impactful.
You MUST respond with valid JSON matching this exact structure:
{
  ""improvedContent"": ""string: the improved content"",
  ""explanation"": ""string: what was improved and why"",
  ""furtherSuggestions"": [""string""],
  ""readingLevel"": ""Professional""
}
Only output valid JSON, no other text.";

            var userPrompt = $"Current Content:\n{request.CurrentContent}\n\nImprovement Type: {request.ImprovementType}\nLanguage: {request.Language}";

            var content = await GenerateContentAsync(systemPrompt, userPrompt, request.MaxLength ?? 2000, 0.7f, cancellationToken);
            var endTime = DateTime.UtcNow;
            var parsed = ParseJson<SectionImprovementStructuredOutput>(content);

            var improvedContent = parsed?.ImprovedContent ?? content;
            return new SectionImprovementResponse
            {
                OriginalContent = request.CurrentContent,
                ImprovedContent = improvedContent,
                ImprovementType = request.ImprovementType,
                Language = request.Language,
                PlanType = request.PlanType,
                Model = _settings.Model,
                GeneratedAt = endTime,
                Confidence = 0.85,
                ImprovementExplanation = parsed?.Explanation ?? "Content improved by Claude",
                FurtherSuggestions = parsed?.FurtherSuggestions ?? new List<string> { "Consider adding examples", "Include data" },
                WordCount = improvedContent.Split(' ').Length,
                ReadingLevel = parsed?.ReadingLevel ?? "Professional",
                ProcessingTime = endTime - startTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error improving section");
            throw;
        }
    }

    public async Task<SectionExpansionResponse> ExpandSectionAsync(
        SectionImprovementRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureClientConfigured();

        try
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("Expanding section");

            var systemPrompt = @"You are an expert business consultant. Expand the content with more details, examples, and supporting arguments.
You MUST respond with valid JSON matching this exact structure:
{
  ""improvedContent"": ""string: the expanded content"",
  ""explanation"": ""string: what was expanded"",
  ""addedSubsections"": [""string""],
  ""expandedPoints"": [""string""]
}
Only output valid JSON, no other text.";

            var userPrompt = $"Current Content:\n{request.CurrentContent}\n\nLanguage: {request.Language}";

            var content = await GenerateContentAsync(systemPrompt, userPrompt, _settings.MaxTokens, 0.7f, cancellationToken);
            var endTime = DateTime.UtcNow;
            var parsed = ParseJson<SectionExpansionStructuredOutput>(content);

            var improvedContent = parsed?.ImprovedContent ?? content;
            return new SectionExpansionResponse
            {
                OriginalContent = request.CurrentContent,
                ImprovedContent = improvedContent,
                ImprovementType = "expand",
                Language = request.Language,
                PlanType = request.PlanType,
                Model = _settings.Model,
                GeneratedAt = endTime,
                Confidence = 0.85,
                ImprovementExplanation = parsed?.Explanation ?? "Content expanded by Claude",
                FurtherSuggestions = new List<string>(),
                WordCount = improvedContent.Split(' ').Length,
                ReadingLevel = "Professional",
                ProcessingTime = endTime - startTime,
                AddedSubsections = parsed?.AddedSubsections ?? new List<string> { "Additional details" },
                ExpandedPoints = parsed?.ExpandedPoints ?? new List<string> { "Key points expanded" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error expanding section");
            throw;
        }
    }

    public async Task<SectionSimplificationResponse> SimplifySectionAsync(
        SectionImprovementRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureClientConfigured();

        try
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("Simplifying section");

            var systemPrompt = @"You are an expert business consultant. Simplify the content to make it more accessible while preserving key information.
You MUST respond with valid JSON matching this exact structure:
{
  ""improvedContent"": ""string: the simplified content"",
  ""explanation"": ""string: what was simplified"",
  ""simplifiedTerms"": [""string: terms that were simplified""],
  ""removedJargon"": [""string: jargon that was removed""],
  ""originalComplexity"": 0.8,
  ""newComplexity"": 0.3
}
Only output valid JSON, no other text.";

            var userPrompt = $"Current Content:\n{request.CurrentContent}\n\nLanguage: {request.Language}";

            var content = await GenerateContentAsync(systemPrompt, userPrompt, request.MaxLength ?? 1500, 0.6f, cancellationToken);
            var endTime = DateTime.UtcNow;
            var parsed = ParseJson<SectionSimplificationStructuredOutput>(content);

            var improvedContent = parsed?.ImprovedContent ?? content;
            return new SectionSimplificationResponse
            {
                OriginalContent = request.CurrentContent,
                ImprovedContent = improvedContent,
                ImprovementType = "simplify",
                Language = request.Language,
                PlanType = request.PlanType,
                Model = _settings.Model,
                GeneratedAt = endTime,
                Confidence = 0.9,
                ImprovementExplanation = parsed?.Explanation ?? "Content simplified by Claude",
                FurtherSuggestions = new List<string>(),
                WordCount = improvedContent.Split(' ').Length,
                ReadingLevel = "General",
                ProcessingTime = endTime - startTime,
                SimplifiedTerms = parsed?.SimplifiedTerms ?? new List<string> { "Technical terms" },
                RemovedJargon = parsed?.RemovedJargon ?? new List<string> { "Complex terminology" },
                OriginalComplexity = parsed?.OriginalComplexity ?? 0.8,
                NewComplexity = parsed?.NewComplexity ?? 0.3
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error simplifying section");
            throw;
        }
    }

    public async Task<StrategySuggestionResponse> GenerateStrategySuggestionsAsync(
        StrategySuggestionRequest request,
        string businessPlanContext,
        CancellationToken cancellationToken = default)
    {
        EnsureClientConfigured();

        try
        {
            _logger.LogInformation("Generating strategy suggestions");

            var systemPrompt = @"You are an expert business strategy consultant. Analyze the business context and generate actionable growth strategies.
You MUST respond with valid JSON matching this exact structure:
{
  ""suggestions"": [
    {
      ""title"": ""string: strategy title"",
      ""description"": ""string: detailed description"",
      ""category"": ""string: growth|marketing|operations|financial|innovation"",
      ""priority"": ""High|Medium|Low"",
      ""expectedImpact"": ""string: expected impact description"",
      ""implementationSteps"": [""string""],
      ""estimatedTimeframe"": ""string: e.g. 3-6 months"",
      ""reasoning"": ""string: why this strategy is recommended""
    }
  ]
}
Only output valid JSON, no other text.";

            var userPrompt = $"Business Plan Context:\n{businessPlanContext}\n\nGenerate {request.SuggestionCount} strategy suggestions.\nFocus Area: {request.FocusArea ?? "growth"}\nLanguage: {request.Language}";

            // Phase 3: Extended thinking for complex analysis
            string content;
            if (_settings.EnableExtendedThinking)
            {
                content = await GenerateWithThinkingAsync(systemPrompt, userPrompt, 7000, 4000, cancellationToken);
            }
            else
            {
                content = await GenerateContentAsync(systemPrompt, userPrompt, 3000, 0.8f, cancellationToken);
            }

            var parsed = ParseJson<StrategySuggestionsStructuredOutput>(content);

            var suggestions = parsed?.Suggestions?.Select(s => new StrategySuggestion
            {
                Title = s.Title ?? "Strategy",
                Description = s.Description ?? string.Empty,
                Category = s.Category ?? request.FocusArea ?? "growth",
                Priority = s.Priority ?? "Medium",
                ExpectedImpact = s.ExpectedImpact ?? "Significant improvement",
                ImplementationSteps = s.ImplementationSteps ?? new List<string> { "Step 1", "Step 2" },
                EstimatedTimeframe = s.EstimatedTimeframe ?? "3-6 months",
                Reasoning = s.Reasoning ?? "Based on business plan analysis"
            }).ToList();

            // Fallback if parsing failed
            if (suggestions == null || suggestions.Count == 0)
            {
                suggestions = new List<StrategySuggestion>();
                for (int i = 0; i < request.SuggestionCount; i++)
                {
                    suggestions.Add(new StrategySuggestion
                    {
                        Title = $"Growth Strategy {i + 1}",
                        Description = content.Length > 200 ? content[..200] : content,
                        Category = request.FocusArea ?? "growth",
                        Priority = i == 0 ? "High" : "Medium",
                        ExpectedImpact = "Significant improvement",
                        ImplementationSteps = new List<string> { "Step 1", "Step 2" },
                        EstimatedTimeframe = "3-6 months",
                        Reasoning = "Based on business plan analysis"
                    });
                }
            }

            return new StrategySuggestionResponse
            {
                BusinessPlanId = request.BusinessPlanId,
                Suggestions = suggestions,
                GeneratedAt = DateTime.UtcNow,
                Model = _settings.Model,
                Language = request.Language
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating strategy suggestions");
            throw;
        }
    }

    public async Task<RiskMitigationResponse> AnalyzeRisksAsync(
        RiskMitigationRequest request,
        string businessPlanContext,
        CancellationToken cancellationToken = default)
    {
        EnsureClientConfigured();

        try
        {
            _logger.LogInformation("Analyzing risks");

            var systemPrompt = @"You are an expert in business risk management. Identify risks and provide detailed mitigation strategies.
You MUST respond with valid JSON matching this exact structure:
{
  ""risks"": [
    {
      ""title"": ""string: risk title"",
      ""description"": ""string: detailed risk description"",
      ""category"": ""string: financial|operational|market|legal|technological"",
      ""likelihood"": ""High|Medium|Low"",
      ""impact"": ""High|Medium|Low"",
      ""riskScore"": 5,
      ""mitigationStrategies"": [
        {
          ""title"": ""string"",
          ""description"": ""string"",
          ""priority"": ""High|Medium|Low"",
          ""effectiveness"": ""High|Medium|Low""
        }
      ],
      ""contingencyPlan"": ""string""
    }
  ],
  ""overallRiskLevel"": ""High|Medium|Low""
}
Only output valid JSON, no other text.";

            var userPrompt = $"Business Plan Context:\n{businessPlanContext}\n\nIdentify at least {request.MinRiskCount} risks.\nRisk Categories: {string.Join(", ", request.RiskCategories ?? new List<string> { "all" })}\nLanguage: {request.Language}";

            // Phase 3: Extended thinking for risk analysis
            string content;
            if (_settings.EnableExtendedThinking)
            {
                content = await GenerateWithThinkingAsync(systemPrompt, userPrompt, 8000, 5000, cancellationToken);
            }
            else
            {
                content = await GenerateContentAsync(systemPrompt, userPrompt, 4000, 0.7f, cancellationToken);
            }

            var parsed = ParseJson<RiskAnalysisStructuredOutput>(content);

            var risks = parsed?.Risks?.Select(r => new RiskAnalysis
            {
                Title = r.Title ?? "Risk",
                Description = r.Description ?? string.Empty,
                Category = r.Category ?? request.RiskCategories?.FirstOrDefault() ?? "financial",
                Likelihood = r.Likelihood ?? "Medium",
                Impact = r.Impact ?? "Medium",
                RiskScore = r.RiskScore,
                MitigationStrategies = r.MitigationStrategies?.Select(m => new MitigationStrategy
                {
                    Title = m.Title ?? "Mitigation",
                    Description = m.Description ?? string.Empty,
                    Priority = m.Priority ?? "High",
                    Effectiveness = m.Effectiveness ?? "High"
                }).ToList() ?? new List<MitigationStrategy>
                {
                    new() { Title = "Mitigation Strategy", Description = "Mitigation approach", Priority = "High", Effectiveness = "High" }
                },
                ContingencyPlan = r.ContingencyPlan ?? "Contingency plan if risk materializes"
            }).ToList();

            // Fallback
            if (risks == null || risks.Count == 0)
            {
                risks = new List<RiskAnalysis>();
                for (int i = 0; i < request.MinRiskCount; i++)
                {
                    risks.Add(new RiskAnalysis
                    {
                        Title = $"Risk {i + 1}",
                        Description = content.Length > 200 ? content[..200] : content,
                        Category = request.RiskCategories?.FirstOrDefault() ?? "financial",
                        Likelihood = "Medium",
                        Impact = "Medium",
                        RiskScore = 5,
                        MitigationStrategies = new List<MitigationStrategy>
                        {
                            new() { Title = "Mitigation Strategy", Description = "Mitigation approach", Priority = "High", Effectiveness = "High" }
                        },
                        ContingencyPlan = "Contingency plan if risk materializes"
                    });
                }
            }

            return new RiskMitigationResponse
            {
                BusinessPlanId = request.BusinessPlanId,
                Risks = risks,
                Summary = "Risk analysis completed",
                OverallRiskLevel = parsed?.OverallRiskLevel ?? "Medium",
                GeneratedAt = DateTime.UtcNow,
                Model = _settings.Model,
                Language = request.Language
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing risks");
            throw;
        }
    }

    public async Task<BusinessMentorResponse> PerformBusinessMentorAnalysisAsync(
        BusinessMentorRequest request,
        string businessPlanContext,
        CancellationToken cancellationToken = default)
    {
        EnsureClientConfigured();

        try
        {
            _logger.LogInformation("Performing business mentor analysis");

            var systemPrompt = @"You are an experienced business mentor with decades of experience across multiple industries. Analyze the business plan comprehensively.
You MUST respond with valid JSON matching this exact structure:
{
  ""executiveSummary"": ""string: comprehensive executive analysis"",
  ""opportunities"": [
    {
      ""title"": ""string"",
      ""description"": ""string"",
      ""potentialImpact"": ""High|Medium|Low"",
      ""recommendedActions"": [""string""]
    }
  ],
  ""weaknesses"": [
    {
      ""title"": ""string"",
      ""description"": ""string"",
      ""severity"": ""High|Medium|Low"",
      ""recommendedActions"": [""string""],
      ""impactIfNotAddressed"": ""string""
    }
  ],
  ""recommendations"": [
    {
      ""title"": ""string"",
      ""description"": ""string"",
      ""category"": ""strategy|operations|financial|marketing"",
      ""priority"": ""High|Medium|Low"",
      ""expectedBenefits"": ""string"",
      ""implementationSteps"": [""string""]
    }
  ],
  ""healthScore"": 75,
  ""strengths"": [""string""],
  ""criticalAreas"": [""string""]
}
Only output valid JSON, no other text.";

            var userPrompt = $"Business Plan Context:\n{businessPlanContext}\n\nAnalysis Depth: comprehensive\nLanguage: {request.Language}";

            // Phase 3: Extended thinking with heavy model for deep analysis
            string content;
            if (_settings.EnableExtendedThinking)
            {
                content = await GenerateWithThinkingAsync(
                    systemPrompt, userPrompt, 12000, 8000, cancellationToken, _settings.HeavyModel);
            }
            else
            {
                content = await GenerateContentAsync(systemPrompt, userPrompt, 5000, 0.75f, cancellationToken);
            }

            var parsed = ParseJson<BusinessMentorStructuredOutput>(content);

            return new BusinessMentorResponse
            {
                BusinessPlanId = request.BusinessPlanId,
                ExecutiveSummary = parsed?.ExecutiveSummary ?? (content.Length > 500 ? content[..500] : content),
                Opportunities = parsed?.Opportunities?.Select(o => new Opportunity
                {
                    Title = o.Title ?? "Opportunity",
                    Description = o.Description ?? string.Empty,
                    PotentialImpact = o.PotentialImpact ?? "High",
                    RecommendedActions = o.RecommendedActions ?? new List<string> { "Action 1" }
                }).ToList() ?? new List<Opportunity>
                {
                    new() { Title = "Growth Opportunity", Description = "Identified opportunity", PotentialImpact = "High", RecommendedActions = new List<string> { "Action 1", "Action 2" } }
                },
                Weaknesses = parsed?.Weaknesses?.Select(w => new Weakness
                {
                    Title = w.Title ?? "Weakness",
                    Description = w.Description ?? string.Empty,
                    Severity = w.Severity ?? "Medium",
                    RecommendedActions = w.RecommendedActions ?? new List<string> { "Corrective action" },
                    ImpactIfNotAddressed = w.ImpactIfNotAddressed ?? "Negative impact"
                }).ToList() ?? new List<Weakness>
                {
                    new() { Title = "Identified Weakness", Description = "Weakness description", Severity = "Medium", RecommendedActions = new List<string> { "Corrective action 1" }, ImpactIfNotAddressed = "Negative impact on performance" }
                },
                Recommendations = parsed?.Recommendations?.Select(r => new StrategicRecommendation
                {
                    Title = r.Title ?? "Recommendation",
                    Description = r.Description ?? string.Empty,
                    Category = r.Category ?? "strategy",
                    Priority = r.Priority ?? "High",
                    ExpectedBenefits = r.ExpectedBenefits ?? "Expected benefits",
                    ImplementationSteps = r.ImplementationSteps ?? new List<string> { "Step 1" }
                }).ToList() ?? new List<StrategicRecommendation>
                {
                    new() { Title = "Strategic Recommendation", Description = "Recommendation description", Category = "strategy", Priority = "High", ExpectedBenefits = "Expected benefits", ImplementationSteps = new List<string> { "Step 1", "Step 2" } }
                },
                HealthScore = parsed?.HealthScore ?? 75,
                Strengths = parsed?.Strengths ?? new List<string> { "Strength 1", "Strength 2" },
                CriticalAreas = parsed?.CriticalAreas ?? new List<string> { "Critical area 1" },
                GeneratedAt = DateTime.UtcNow,
                Model = _settings.EnableExtendedThinking ? _settings.HeavyModel : _settings.Model,
                Language = request.Language
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing business mentor analysis");
            throw;
        }
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Phase 3: Generate content with extended thinking enabled
    /// </summary>
    private async Task<string> GenerateWithThinkingAsync(
        string systemPrompt,
        string userPrompt,
        int maxTokens,
        int thinkingBudget,
        CancellationToken cancellationToken,
        string? modelOverride = null)
    {
        // Guard: MaxTokens must exceed BudgetTokens
        var effectiveMaxTokens = maxTokens <= thinkingBudget ? thinkingBudget + 4000 : maxTokens;

        var parameters = new MessageCreateParams
        {
            Model = modelOverride ?? _settings.Model,
            MaxTokens = effectiveMaxTokens,
            Temperature = 1.0, // Required: must be 1.0 when thinking is enabled
            System = BuildSystemParam(systemPrompt),
            Messages = new List<MessageParam>
            {
                new() { Role = Role.User, Content = userPrompt }
            },
            Thinking = new ThinkingConfigEnabled
            {
                BudgetTokens = thinkingBudget
            }
        };

        var response = await _client!.Messages.Create(parameters);

        // Skip ThinkingBlock, extract TextBlock content
        var textContent = string.Empty;
        if (response.Content != null)
        {
            foreach (var block in response.Content)
            {
                if (block.TryPickThinking(out var thinkingBlock))
                {
                    _logger.LogDebug("Extended thinking used {Tokens} characters of reasoning",
                        thinkingBlock.Thinking?.Length ?? 0);
                }
                else if (block.TryPickText(out var textBlock))
                {
                    textContent = textBlock.Text;
                }
            }
        }

        LogCacheMetrics(response);
        return textContent;
    }

    private MessageCreateParamsSystem BuildSystemParam(string systemPrompt)
    {
        if (_settings.EnablePromptCaching && systemPrompt.Length > 500)
        {
            return new List<TextBlockParam>
            {
                new(systemPrompt) { CacheControl = new CacheControlEphemeral() }
            };
        }
        return systemPrompt;
    }

    private void EnsureClientConfigured()
    {
        if (_client == null)
        {
            throw new InvalidOperationException("Claude service is not configured. Please provide a valid API key.");
        }
    }

    private static string ExtractTextContent(Message response)
    {
        if (response.Content == null) return string.Empty;

        foreach (var block in response.Content)
        {
            if (block.TryPickText(out var textBlock))
            {
                return textBlock.Text;
            }
        }

        return string.Empty;
    }

    private void LogCacheMetrics(Message response)
    {
        var cacheCreation = response.Usage?.CacheCreationInputTokens;
        var cacheRead = response.Usage?.CacheReadInputTokens;

        if (cacheCreation > 0)
        {
            _logger.LogInformation("Cache created: {CacheCreationTokens} input tokens cached", cacheCreation);
        }

        if (cacheRead > 0)
        {
            _logger.LogInformation("Cache hit: {CacheReadTokens} input tokens read from cache", cacheRead);
        }
    }

    private static T? ParseJson<T>(string content) where T : class
    {
        try
        {
            // Try to extract JSON from the response (handle markdown code blocks)
            var jsonContent = content.Trim();
            if (jsonContent.StartsWith("```json"))
            {
                jsonContent = jsonContent[7..];
            }
            else if (jsonContent.StartsWith("```"))
            {
                jsonContent = jsonContent[3..];
            }

            if (jsonContent.EndsWith("```"))
            {
                jsonContent = jsonContent[..^3];
            }

            jsonContent = jsonContent.Trim();

            // Find JSON object boundaries
            var jsonStart = jsonContent.IndexOf('{');
            var jsonEnd = jsonContent.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                jsonContent = jsonContent[jsonStart..(jsonEnd + 1)];
                return JsonSerializer.Deserialize<T>(jsonContent, JsonOptions);
            }

            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    #endregion
}
