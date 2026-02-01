using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services;
using Sqordia.Domain.Enums;

namespace Sqordia.Infrastructure.Services;

public interface IAIProviderFactory
{
    /// <summary>
    /// Gets the currently active AI provider based on settings
    /// </summary>
    Task<IAIService?> GetActiveProviderAsync();

    /// <summary>
    /// Gets the fallback providers in order based on settings
    /// </summary>
    Task<List<IAIService>> GetFallbackProvidersAsync();

    /// <summary>
    /// Invalidates the cached provider selection, forcing a refresh from settings
    /// </summary>
    void InvalidateCache();

    /// <summary>
    /// Gets a specific provider by type
    /// </summary>
    IAIService? GetProvider(AIProviderType providerType);
}

public class AIProviderFactory : IAIProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<AIProviderFactory> _logger;
    private string? _cachedActiveProvider;
    private List<string>? _cachedFallbackProviders;
    private DateTime _lastCacheTime = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public AIProviderFactory(
        IServiceProvider serviceProvider,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<AIProviderFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task<IAIService?> GetActiveProviderAsync()
    {
        try
        {
            var activeProviderName = await GetActiveProviderNameAsync();

            if (string.IsNullOrEmpty(activeProviderName))
            {
                _logger.LogWarning("No active AI provider configured. Defaulting to OpenAI.");
                activeProviderName = "OpenAI";
            }

            var provider = GetProviderByName(activeProviderName);

            if (provider == null)
            {
                _logger.LogWarning("Could not resolve provider: {ProviderName}. Falling back to OpenAI.", activeProviderName);
                provider = _serviceProvider.GetService<OpenAIService>();
            }

            return provider;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active provider. Falling back to OpenAI.");
            return _serviceProvider.GetService<OpenAIService>();
        }
    }

    public async Task<List<IAIService>> GetFallbackProvidersAsync()
    {
        try
        {
            var fallbackNames = await GetFallbackProviderNamesAsync();
            var fallbackProviders = new List<IAIService>();

            foreach (var name in fallbackNames)
            {
                var provider = GetProviderByName(name);
                if (provider != null)
                {
                    fallbackProviders.Add(provider);
                }
                else
                {
                    _logger.LogWarning("Could not resolve fallback provider: {ProviderName}", name);
                }
            }

            return fallbackProviders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fallback providers. Returning empty list.");
            return new List<IAIService>();
        }
    }

    public IAIService? GetProvider(AIProviderType providerType)
    {
        return providerType switch
        {
            AIProviderType.OpenAI => _serviceProvider.GetService<OpenAIService>(),
            AIProviderType.Claude => _serviceProvider.GetService<ClaudeService>(),
            AIProviderType.Gemini => _serviceProvider.GetService<GeminiService>(),
            _ => null
        };
    }

    public void InvalidateCache()
    {
        _logger.LogInformation("Invalidating AI provider cache");
        _cachedActiveProvider = null;
        _cachedFallbackProviders = null;
        _lastCacheTime = DateTime.MinValue;
    }

    private async Task<string> GetActiveProviderNameAsync()
    {
        // Check if cache is still valid
        if (_cachedActiveProvider != null && DateTime.UtcNow - _lastCacheTime < _cacheExpiration)
        {
            _logger.LogDebug("Returning cached active provider: {Provider}", _cachedActiveProvider);
            return _cachedActiveProvider;
        }

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var settingsService = scope.ServiceProvider.GetService<ISettingsService>();
            if (settingsService != null)
            {
                var result = await settingsService.GetSettingAsync("AI.ActiveProvider");
                if (result.IsSuccess && !string.IsNullOrEmpty(result.Value))
                {
                    _cachedActiveProvider = result.Value;
                    _lastCacheTime = DateTime.UtcNow;
                    _logger.LogInformation("Active AI provider from settings: {Provider}", _cachedActiveProvider);
                    return _cachedActiveProvider;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read active provider from settings. Using default.");
        }

        // Default to OpenAI if no setting found
        _cachedActiveProvider = "OpenAI";
        _lastCacheTime = DateTime.UtcNow;
        return _cachedActiveProvider;
    }

    private async Task<List<string>> GetFallbackProviderNamesAsync()
    {
        // Check if cache is still valid
        if (_cachedFallbackProviders != null && DateTime.UtcNow - _lastCacheTime < _cacheExpiration)
        {
            _logger.LogDebug("Returning cached fallback providers: {Providers}", string.Join(", ", _cachedFallbackProviders));
            return _cachedFallbackProviders;
        }

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var settingsService = scope.ServiceProvider.GetService<ISettingsService>();
            if (settingsService != null)
            {
                var result = await settingsService.GetSettingAsync("AI.FallbackProviders");
                if (result.IsSuccess && !string.IsNullOrEmpty(result.Value))
                {
                    // Parse JSON array
                    var fallbackProviders = System.Text.Json.JsonSerializer.Deserialize<List<string>>(result.Value);
                    if (fallbackProviders != null && fallbackProviders.Any())
                    {
                        _cachedFallbackProviders = fallbackProviders;
                        _lastCacheTime = DateTime.UtcNow;
                        _logger.LogInformation("Fallback AI providers from settings: {Providers}", string.Join(", ", _cachedFallbackProviders));
                        return _cachedFallbackProviders;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read fallback providers from settings. Using default.");
        }

        // Default fallback order: Claude, then Gemini
        _cachedFallbackProviders = new List<string> { "Claude", "Gemini" };
        _lastCacheTime = DateTime.UtcNow;
        return _cachedFallbackProviders;
    }

    private IAIService? GetProviderByName(string providerName)
    {
        return providerName.ToLower() switch
        {
            "openai" => _serviceProvider.GetService<OpenAIService>(),
            "claude" => _serviceProvider.GetService<ClaudeService>(),
            "gemini" => _serviceProvider.GetService<GeminiService>(),
            _ => null
        };
    }
}
