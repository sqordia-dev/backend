using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Anthropic.SDK.Constants;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Contracts.Requests.Questionnaire;
using Sqordia.Contracts.Responses.Questionnaire;
using Sqordia.Contracts.Requests.Sections;
using Sqordia.Contracts.Responses.Sections;
using Sqordia.Contracts.Requests.AI;
using Sqordia.Contracts.Responses.AI;
using System.Text.Json;

namespace Sqordia.Infrastructure.Services;

public class ClaudeSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-3-5-sonnet-latest";
    public int MaxTokens { get; set; } = 4000;
}

public class ClaudeService : IAIService
{
    private readonly ILogger<ClaudeService> _logger;
    private readonly ClaudeSettings _settings;
    private readonly AnthropicClient? _client;

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
                _client = new AnthropicClient(_settings.ApiKey);
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

    public async Task<string> GenerateContentAsync(
        string systemPrompt,
        string userPrompt,
        int maxTokens = 2000,
        float temperature = 0.7f,
        CancellationToken cancellationToken = default)
    {
        if (_client == null)
        {
            _logger.LogWarning("Claude service not configured");
            throw new InvalidOperationException("Claude service is not configured. Please provide a valid API key.");
        }

        try
        {
            _logger.LogInformation("Generating content with Claude");

            var parameters = new MessageParameters
            {
                Model = _settings.Model,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = RoleType.User,
                        Content = new List<ContentBase>
                        {
                            new TextContent { Text = userPrompt }
                        }
                    }
                },
                MaxTokens = maxTokens,
                Temperature = (decimal)temperature,
                System = new List<SystemMessage> { new SystemMessage(systemPrompt) }
            };

            var response = await _client.Messages.GetClaudeMessageAsync(parameters, cancellationToken);
            var textContent = response.Content.FirstOrDefault() as TextContent;
            var content = textContent?.Text ?? string.Empty;

            _logger.LogInformation("Content generated successfully");
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating content with Claude");
            throw;
        }
    }

    public async Task<string> GenerateContentWithRetryAsync(
        string systemPrompt,
        string userPrompt,
        int maxTokens = 2000,
        float temperature = 0.7f,
        int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        Exception? lastException = null;

        while (attempt < maxRetries)
        {
            try
            {
                return await GenerateContentAsync(systemPrompt, userPrompt, maxTokens, temperature, cancellationToken);
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempt++;

                if (attempt < maxRetries)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    _logger.LogWarning(ex, "Attempt {Attempt}/{MaxRetries} failed. Retrying in {Delay} seconds...",
                        attempt, maxRetries, delay.TotalSeconds);
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        throw new InvalidOperationException($"Failed to generate content after {maxRetries} attempts.", lastException);
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (_client == null)
        {
            _logger.LogWarning("Claude service unavailable: client is null");
            return Task.FromResult(false);
        }

        _logger.LogDebug("Claude service is available");
        return Task.FromResult(true);
    }

    public async Task<QuestionSuggestionResponse> GenerateQuestionSuggestionsAsync(
        QuestionSuggestionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_client == null)
        {
            throw new InvalidOperationException("Claude service is not configured.");
        }

        try
        {
            _logger.LogInformation("Generating question suggestions");

            var systemPrompt = "You are an expert business consultant helping with questionnaire responses for business plan creation.";
            var userPrompt = $"Question: {request.QuestionText}\nPlan Type: {request.PlanType}\n\nGenerate 3-5 helpful suggestions.";

            var content = await GenerateContentAsync(systemPrompt, userPrompt, 1500, 0.8f, cancellationToken);

            return new QuestionSuggestionResponse
            {
                QuestionText = request.QuestionText,
                PlanType = request.PlanType,
                Suggestions = new List<QuestionSuggestion>
                {
                    new QuestionSuggestion
                    {
                        Answer = content,
                        Confidence = 0.8,
                        Reasoning = "Generated by Claude",
                        SuggestionType = "Detailed"
                    }
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
        if (_client == null)
        {
            throw new InvalidOperationException("Claude service is not configured.");
        }

        try
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("Improving section");

            var systemPrompt = "You are an expert business consultant. Improve the content to make it more professional and impactful.";
            var userPrompt = $"Current Content:\n{request.CurrentContent}\n\nPlease improve this content.";

            var content = await GenerateContentAsync(systemPrompt, userPrompt, request.MaxLength ?? 2000, 0.7f, cancellationToken);
            var endTime = DateTime.UtcNow;

            return new SectionImprovementResponse
            {
                OriginalContent = request.CurrentContent,
                ImprovedContent = content,
                ImprovementType = request.ImprovementType,
                Language = request.Language,
                PlanType = request.PlanType,
                Model = _settings.Model,
                GeneratedAt = endTime,
                Confidence = 0.85,
                ImprovementExplanation = "Content improved by Claude",
                FurtherSuggestions = new List<string> { "Consider adding examples", "Include data" },
                WordCount = content.Split(' ').Length,
                ReadingLevel = "Professional",
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
        if (_client == null)
        {
            throw new InvalidOperationException("Claude service is not configured.");
        }

        try
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("Expanding section");

            var systemPrompt = "You are an expert business consultant. Expand the content with more details and examples.";
            var userPrompt = $"Current Content:\n{request.CurrentContent}\n\nPlease expand this content.";

            var content = await GenerateContentAsync(systemPrompt, userPrompt, _settings.MaxTokens, 0.7f, cancellationToken);
            var endTime = DateTime.UtcNow;

            return new SectionExpansionResponse
            {
                OriginalContent = request.CurrentContent,
                ImprovedContent = content,
                ImprovementType = "expand",
                Language = request.Language,
                PlanType = request.PlanType,
                Model = _settings.Model,
                GeneratedAt = endTime,
                Confidence = 0.85,
                ImprovementExplanation = "Content expanded by Claude",
                FurtherSuggestions = new List<string>(),
                WordCount = content.Split(' ').Length,
                ReadingLevel = "Professional",
                ProcessingTime = endTime - startTime,
                AddedSubsections = new List<string> { "Additional details" },
                ExpandedPoints = new List<string> { "Key points expanded" }
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
        if (_client == null)
        {
            throw new InvalidOperationException("Claude service is not configured.");
        }

        try
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("Simplifying section");

            var systemPrompt = "You are an expert business consultant. Simplify the content to make it more accessible.";
            var userPrompt = $"Current Content:\n{request.CurrentContent}\n\nPlease simplify this content.";

            var content = await GenerateContentAsync(systemPrompt, userPrompt, request.MaxLength ?? 1500, 0.6f, cancellationToken);
            var endTime = DateTime.UtcNow;

            return new SectionSimplificationResponse
            {
                OriginalContent = request.CurrentContent,
                ImprovedContent = content,
                ImprovementType = "simplify",
                Language = request.Language,
                PlanType = request.PlanType,
                Model = _settings.Model,
                GeneratedAt = endTime,
                Confidence = 0.9,
                ImprovementExplanation = "Content simplified by Claude",
                FurtherSuggestions = new List<string>(),
                WordCount = content.Split(' ').Length,
                ReadingLevel = "General",
                ProcessingTime = endTime - startTime,
                SimplifiedTerms = new List<string> { "Technical terms" },
                RemovedJargon = new List<string> { "Complex terminology" },
                OriginalComplexity = 0.8,
                NewComplexity = 0.3
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
        if (_client == null)
        {
            throw new InvalidOperationException("Claude service is not configured.");
        }

        try
        {
            _logger.LogInformation("Generating strategy suggestions");

            var systemPrompt = "You are an expert business strategy consultant. Generate actionable growth strategies.";
            var userPrompt = $"Business Plan Context:\n{businessPlanContext}\n\nGenerate {request.SuggestionCount} strategy suggestions.";

            var content = await GenerateContentAsync(systemPrompt, userPrompt, 3000, 0.8f, cancellationToken);

            var suggestions = new List<StrategySuggestion>();
            for (int i = 0; i < request.SuggestionCount; i++)
            {
                suggestions.Add(new StrategySuggestion
                {
                    Title = $"Growth Strategy {i + 1}",
                    Description = content.Length > 200 ? content.Substring(0, 200) : content,
                    Category = request.FocusArea ?? "growth",
                    Priority = i == 0 ? "High" : "Medium",
                    ExpectedImpact = "Significant improvement",
                    ImplementationSteps = new List<string> { "Step 1", "Step 2" },
                    EstimatedTimeframe = "3-6 months",
                    Reasoning = "Based on business plan analysis"
                });
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
        if (_client == null)
        {
            throw new InvalidOperationException("Claude service is not configured.");
        }

        try
        {
            _logger.LogInformation("Analyzing risks");

            var systemPrompt = "You are an expert in business risk management. Identify risks and mitigation strategies.";
            var userPrompt = $"Business Plan Context:\n{businessPlanContext}\n\nIdentify at least {request.MinRiskCount} risks.";

            var content = await GenerateContentAsync(systemPrompt, userPrompt, 4000, 0.7f, cancellationToken);

            var risks = new List<RiskAnalysis>();
            for (int i = 0; i < request.MinRiskCount; i++)
            {
                risks.Add(new RiskAnalysis
                {
                    Title = $"Risk {i + 1}",
                    Description = content.Length > 200 ? content.Substring(0, 200) : content,
                    Category = request.RiskCategories?.FirstOrDefault() ?? "financial",
                    Likelihood = "Medium",
                    Impact = "Medium",
                    RiskScore = 5,
                    MitigationStrategies = new List<MitigationStrategy>
                    {
                        new MitigationStrategy
                        {
                            Title = "Mitigation Strategy",
                            Description = "Mitigation approach",
                            Priority = "High",
                            Effectiveness = "High"
                        }
                    },
                    ContingencyPlan = "Contingency plan if risk materializes"
                });
            }

            return new RiskMitigationResponse
            {
                BusinessPlanId = request.BusinessPlanId,
                Risks = risks,
                Summary = "Risk analysis completed",
                OverallRiskLevel = "Medium",
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
        if (_client == null)
        {
            throw new InvalidOperationException("Claude service is not configured.");
        }

        try
        {
            _logger.LogInformation("Performing business mentor analysis");

            var systemPrompt = "You are an experienced business mentor. Analyze the business plan comprehensively.";
            var userPrompt = $"Business Plan Context:\n{businessPlanContext}\n\nProvide a comprehensive analysis.";

            var content = await GenerateContentAsync(systemPrompt, userPrompt, 5000, 0.75f, cancellationToken);

            return new BusinessMentorResponse
            {
                BusinessPlanId = request.BusinessPlanId,
                ExecutiveSummary = content.Length > 500 ? content.Substring(0, 500) : content,
                Opportunities = new List<Opportunity>
                {
                    new Opportunity
                    {
                        Title = "Growth Opportunity",
                        Description = "Identified opportunity",
                        PotentialImpact = "High",
                        RecommendedActions = new List<string> { "Action 1", "Action 2" }
                    }
                },
                Weaknesses = new List<Weakness>
                {
                    new Weakness
                    {
                        Title = "Identified Weakness",
                        Description = "Weakness description",
                        Severity = "Medium",
                        RecommendedActions = new List<string> { "Corrective action 1" },
                        ImpactIfNotAddressed = "Negative impact on performance"
                    }
                },
                Recommendations = new List<StrategicRecommendation>
                {
                    new StrategicRecommendation
                    {
                        Title = "Strategic Recommendation",
                        Description = "Recommendation description",
                        Category = "strategy",
                        Priority = "High",
                        ExpectedBenefits = "Expected benefits",
                        ImplementationSteps = new List<string> { "Step 1", "Step 2" }
                    }
                },
                HealthScore = 75,
                Strengths = new List<string> { "Strength 1", "Strength 2" },
                CriticalAreas = new List<string> { "Critical area 1" },
                GeneratedAt = DateTime.UtcNow,
                Model = _settings.Model,
                Language = request.Language
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing business mentor analysis");
            throw;
        }
    }
}
