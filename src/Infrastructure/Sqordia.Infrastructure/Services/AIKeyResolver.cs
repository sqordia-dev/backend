using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Resolves AI provider keys from DB Settings table first, falls back to IOptions (env vars / appsettings).
/// Singleton-safe: creates scopes to access the scoped ISettingsService.
/// Caches resolved values for 5 minutes; call InvalidateCache() after admin saves.
/// </summary>
public class AIKeyResolver : IAIKeyResolver
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<OpenAISettings> _openAIOptions;
    private readonly IOptions<ClaudeSettings> _claudeOptions;
    private readonly IOptions<GeminiSettings> _geminiOptions;
    private readonly ILogger<AIKeyResolver> _logger;

    private readonly object _cacheLock = new();
    private Dictionary<string, AIProviderConfig>? _cache;
    private DateTime _lastCacheTime = DateTime.MinValue;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    public AIKeyResolver(
        IServiceScopeFactory scopeFactory,
        IOptions<OpenAISettings> openAIOptions,
        IOptions<ClaudeSettings> claudeOptions,
        IOptions<GeminiSettings> geminiOptions,
        ILogger<AIKeyResolver> logger)
    {
        _scopeFactory = scopeFactory;
        _openAIOptions = openAIOptions;
        _claudeOptions = claudeOptions;
        _geminiOptions = geminiOptions;
        _logger = logger;
    }

    public async Task<AIProviderConfig> ResolveOpenAIAsync(CancellationToken ct = default)
    {
        var cache = await GetOrRefreshCacheAsync(ct);
        return cache.TryGetValue("OpenAI", out var config) ? config : new AIProviderConfig(string.Empty, "gpt-4.1");
    }

    public async Task<AIProviderConfig> ResolveClaudeAsync(CancellationToken ct = default)
    {
        var cache = await GetOrRefreshCacheAsync(ct);
        return cache.TryGetValue("Claude", out var config) ? config : new AIProviderConfig(string.Empty, "claude-sonnet-4-6");
    }

    public async Task<AIProviderConfig> ResolveGeminiAsync(CancellationToken ct = default)
    {
        var cache = await GetOrRefreshCacheAsync(ct);
        return cache.TryGetValue("Gemini", out var config) ? config : new AIProviderConfig(string.Empty, "gemini-2.5-flash");
    }

    public void InvalidateCache()
    {
        lock (_cacheLock)
        {
            _cache = null;
            _lastCacheTime = DateTime.MinValue;
        }
        _logger.LogInformation("AI key resolver cache invalidated");
    }

    private async Task<Dictionary<string, AIProviderConfig>> GetOrRefreshCacheAsync(CancellationToken ct)
    {
        // Fast path: cache still valid
        lock (_cacheLock)
        {
            if (_cache != null && DateTime.UtcNow - _lastCacheTime < CacheExpiration)
                return _cache;
        }

        // Slow path: refresh from DB
        var newCache = new Dictionary<string, AIProviderConfig>();

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var settingsService = scope.ServiceProvider.GetService<ISettingsService>();

            if (settingsService != null)
            {
                newCache["OpenAI"] = await ResolveFromDbAsync(
                    settingsService, "OpenAI",
                    _openAIOptions.Value.ApiKey, _openAIOptions.Value.Model, ct);

                newCache["Claude"] = await ResolveFromDbAsync(
                    settingsService, "Claude",
                    _claudeOptions.Value.ApiKey, _claudeOptions.Value.Model, ct);

                newCache["Gemini"] = await ResolveFromDbAsync(
                    settingsService, "Gemini",
                    _geminiOptions.Value.ApiKey, _geminiOptions.Value.Model, ct);
            }
            else
            {
                // No settings service available, use IOptions directly
                newCache["OpenAI"] = new AIProviderConfig(_openAIOptions.Value.ApiKey, _openAIOptions.Value.Model);
                newCache["Claude"] = new AIProviderConfig(_claudeOptions.Value.ApiKey, _claudeOptions.Value.Model);
                newCache["Gemini"] = new AIProviderConfig(_geminiOptions.Value.ApiKey, _geminiOptions.Value.Model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve AI keys from DB, falling back to IOptions");
            newCache["OpenAI"] = new AIProviderConfig(_openAIOptions.Value.ApiKey, _openAIOptions.Value.Model);
            newCache["Claude"] = new AIProviderConfig(_claudeOptions.Value.ApiKey, _claudeOptions.Value.Model);
            newCache["Gemini"] = new AIProviderConfig(_geminiOptions.Value.ApiKey, _geminiOptions.Value.Model);
        }

        lock (_cacheLock)
        {
            _cache = newCache;
            _lastCacheTime = DateTime.UtcNow;
        }

        return newCache;
    }

    private async Task<AIProviderConfig> ResolveFromDbAsync(
        ISettingsService settingsService,
        string providerName,
        string fallbackKey,
        string fallbackModel,
        CancellationToken ct)
    {
        // DB is authoritative — try it first
        var dbKeyResult = await settingsService.GetSettingAsync($"AI.{providerName}.ApiKey");
        var dbModelResult = await settingsService.GetSettingAsync($"AI.{providerName}.Model");

        var apiKey = (dbKeyResult.IsSuccess && !string.IsNullOrEmpty(dbKeyResult.Value))
            ? dbKeyResult.Value
            : fallbackKey;

        var model = (dbModelResult.IsSuccess && !string.IsNullOrEmpty(dbModelResult.Value))
            ? dbModelResult.Value
            : fallbackModel;

        var source = (dbKeyResult.IsSuccess && !string.IsNullOrEmpty(dbKeyResult.Value)) ? "DB" : "Env";
        _logger.LogDebug("Resolved {Provider} key from {Source}, model: {Model}", providerName, source, model);

        return new AIProviderConfig(apiKey ?? string.Empty, model ?? string.Empty);
    }
}
