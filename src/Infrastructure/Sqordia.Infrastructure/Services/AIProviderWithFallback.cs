using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Contracts.Requests.Questionnaire;
using Sqordia.Contracts.Responses.Questionnaire;
using Sqordia.Contracts.Requests.Sections;
using Sqordia.Contracts.Responses.Sections;
using Sqordia.Contracts.Requests.AI;
using Sqordia.Contracts.Responses.AI;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Decorator for IAIService that implements automatic fallback logic
/// Tries the primary provider first, then falls back to secondary providers if the primary fails
/// </summary>
public class AIProviderWithFallback : IAIService
{
    private readonly IAIProviderFactory _factory;
    private readonly ILogger<AIProviderWithFallback> _logger;

    public AIProviderWithFallback(
        IAIProviderFactory factory,
        ILogger<AIProviderWithFallback> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<string> GenerateContentAsync(
        string systemPrompt,
        string userPrompt,
        int maxTokens = 2000,
        float temperature = 0.7f,
        CancellationToken cancellationToken = default)
    {
        var providers = await GetProviderChainAsync();

        Exception? lastException = null;

        foreach (var provider in providers)
        {
            try
            {
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogInformation("Attempting to generate content with provider: {Provider}", providerName);

                var result = await provider.GenerateContentAsync(
                    systemPrompt,
                    userPrompt,
                    maxTokens,
                    temperature,
                    cancellationToken);

                _logger.LogInformation("Successfully generated content with provider: {Provider}", providerName);

                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogWarning(ex, "Provider {Provider} failed to generate content. Trying next provider...", providerName);
            }
        }

        _logger.LogError(lastException, "All AI providers failed to generate content");
        throw new InvalidOperationException(
            "All configured AI providers failed to generate content. Please check provider configurations and API keys.",
            lastException);
    }

    public async Task<string> GenerateContentWithRetryAsync(
        string systemPrompt,
        string userPrompt,
        int maxTokens = 2000,
        float temperature = 0.7f,
        int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        var providers = await GetProviderChainAsync();

        Exception? lastException = null;

        foreach (var provider in providers)
        {
            try
            {
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogInformation("Attempting to generate content with retry using provider: {Provider}", providerName);

                var result = await provider.GenerateContentWithRetryAsync(
                    systemPrompt,
                    userPrompt,
                    maxTokens,
                    temperature,
                    maxRetries,
                    cancellationToken);

                _logger.LogInformation("Successfully generated content with retry using provider: {Provider}", providerName);

                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogWarning(ex, "Provider {Provider} failed after retries. Trying next provider...", providerName);
            }
        }

        _logger.LogError(lastException, "All AI providers failed to generate content after retries");
        throw new InvalidOperationException(
            "All configured AI providers failed to generate content after retries. Please check provider configurations and API keys.",
            lastException);
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var activeProvider = await _factory.GetActiveProviderAsync();
            if (activeProvider != null)
            {
                return await activeProvider.IsAvailableAsync(cancellationToken);
            }

            _logger.LogWarning("No active AI provider configured");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking AI provider availability");
            return false;
        }
    }

    public async Task<QuestionSuggestionResponse> GenerateQuestionSuggestionsAsync(
        QuestionSuggestionRequest request,
        CancellationToken cancellationToken = default)
    {
        var providers = await GetProviderChainAsync();

        Exception? lastException = null;

        foreach (var provider in providers)
        {
            try
            {
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogInformation("Generating question suggestions with provider: {Provider}", providerName);

                var result = await provider.GenerateQuestionSuggestionsAsync(request, cancellationToken);

                _logger.LogInformation("Successfully generated question suggestions with provider: {Provider}", providerName);

                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogWarning(ex, "Provider {Provider} failed. Trying next provider...", providerName);
            }
        }

        _logger.LogError(lastException, "All AI providers failed to generate question suggestions");
        throw new InvalidOperationException(
            "All configured AI providers failed to generate question suggestions.",
            lastException);
    }

    public async Task<SectionImprovementResponse> ImproveSectionAsync(
        SectionImprovementRequest request,
        CancellationToken cancellationToken = default)
    {
        var providers = await GetProviderChainAsync();

        Exception? lastException = null;

        foreach (var provider in providers)
        {
            try
            {
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogInformation("Improving section with provider: {Provider}", providerName);

                var result = await provider.ImproveSectionAsync(request, cancellationToken);

                _logger.LogInformation("Successfully improved section with provider: {Provider}", providerName);

                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogWarning(ex, "Provider {Provider} failed. Trying next provider...", providerName);
            }
        }

        _logger.LogError(lastException, "All AI providers failed to improve section");
        throw new InvalidOperationException(
            "All configured AI providers failed to improve section.",
            lastException);
    }

    public async Task<SectionExpansionResponse> ExpandSectionAsync(
        SectionImprovementRequest request,
        CancellationToken cancellationToken = default)
    {
        var providers = await GetProviderChainAsync();

        Exception? lastException = null;

        foreach (var provider in providers)
        {
            try
            {
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogInformation("Expanding section with provider: {Provider}", providerName);

                var result = await provider.ExpandSectionAsync(request, cancellationToken);

                _logger.LogInformation("Successfully expanded section with provider: {Provider}", providerName);

                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogWarning(ex, "Provider {Provider} failed. Trying next provider...", providerName);
            }
        }

        _logger.LogError(lastException, "All AI providers failed to expand section");
        throw new InvalidOperationException(
            "All configured AI providers failed to expand section.",
            lastException);
    }

    public async Task<SectionSimplificationResponse> SimplifySectionAsync(
        SectionImprovementRequest request,
        CancellationToken cancellationToken = default)
    {
        var providers = await GetProviderChainAsync();

        Exception? lastException = null;

        foreach (var provider in providers)
        {
            try
            {
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogInformation("Simplifying section with provider: {Provider}", providerName);

                var result = await provider.SimplifySectionAsync(request, cancellationToken);

                _logger.LogInformation("Successfully simplified section with provider: {Provider}", providerName);

                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogWarning(ex, "Provider {Provider} failed. Trying next provider...", providerName);
            }
        }

        _logger.LogError(lastException, "All AI providers failed to simplify section");
        throw new InvalidOperationException(
            "All configured AI providers failed to simplify section.",
            lastException);
    }

    public async Task<StrategySuggestionResponse> GenerateStrategySuggestionsAsync(
        StrategySuggestionRequest request,
        string businessPlanContext,
        CancellationToken cancellationToken = default)
    {
        var providers = await GetProviderChainAsync();

        Exception? lastException = null;

        foreach (var provider in providers)
        {
            try
            {
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogInformation("Generating strategy suggestions with provider: {Provider}", providerName);

                var result = await provider.GenerateStrategySuggestionsAsync(request, businessPlanContext, cancellationToken);

                _logger.LogInformation("Successfully generated strategy suggestions with provider: {Provider}", providerName);

                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogWarning(ex, "Provider {Provider} failed. Trying next provider...", providerName);
            }
        }

        _logger.LogError(lastException, "All AI providers failed to generate strategy suggestions");
        throw new InvalidOperationException(
            "All configured AI providers failed to generate strategy suggestions.",
            lastException);
    }

    public async Task<RiskMitigationResponse> AnalyzeRisksAsync(
        RiskMitigationRequest request,
        string businessPlanContext,
        CancellationToken cancellationToken = default)
    {
        var providers = await GetProviderChainAsync();

        Exception? lastException = null;

        foreach (var provider in providers)
        {
            try
            {
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogInformation("Analyzing risks with provider: {Provider}", providerName);

                var result = await provider.AnalyzeRisksAsync(request, businessPlanContext, cancellationToken);

                _logger.LogInformation("Successfully analyzed risks with provider: {Provider}", providerName);

                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogWarning(ex, "Provider {Provider} failed. Trying next provider...", providerName);
            }
        }

        _logger.LogError(lastException, "All AI providers failed to analyze risks");
        throw new InvalidOperationException(
            "All configured AI providers failed to analyze risks.",
            lastException);
    }

    public async Task<BusinessMentorResponse> PerformBusinessMentorAnalysisAsync(
        BusinessMentorRequest request,
        string businessPlanContext,
        CancellationToken cancellationToken = default)
    {
        var providers = await GetProviderChainAsync();

        Exception? lastException = null;

        foreach (var provider in providers)
        {
            try
            {
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogInformation("Performing business mentor analysis with provider: {Provider}", providerName);

                var result = await provider.PerformBusinessMentorAnalysisAsync(request, businessPlanContext, cancellationToken);

                _logger.LogInformation("Successfully performed business mentor analysis with provider: {Provider}", providerName);

                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
                var providerName = provider.GetType().Name.Replace("Service", "");
                _logger.LogWarning(ex, "Provider {Provider} failed. Trying next provider...", providerName);
            }
        }

        _logger.LogError(lastException, "All AI providers failed to perform business mentor analysis");
        throw new InvalidOperationException(
            "All configured AI providers failed to perform business mentor analysis.",
            lastException);
    }

    /// <summary>
    /// Gets the chain of providers to try (primary + fallbacks)
    /// </summary>
    private async Task<List<IAIService>> GetProviderChainAsync()
    {
        var providers = new List<IAIService>();

        try
        {
            // Get primary provider
            var primaryProvider = await _factory.GetActiveProviderAsync();
            if (primaryProvider != null)
            {
                providers.Add(primaryProvider);
            }

            // Get fallback providers
            var fallbackProviders = await _factory.GetFallbackProvidersAsync();
            providers.AddRange(fallbackProviders);

            if (providers.Count == 0)
            {
                _logger.LogWarning("No AI providers configured. Cannot perform AI operations.");
                throw new InvalidOperationException("No AI providers are configured. Please configure at least one AI provider.");
            }

            _logger.LogDebug("Provider chain: {Providers}",
                string.Join(" -> ", providers.Select(p => p.GetType().Name.Replace("Service", ""))));

            return providers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building provider chain");
            throw;
        }
    }
}
