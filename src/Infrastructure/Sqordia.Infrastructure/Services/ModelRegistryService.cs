using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// DB-backed model registry that allows runtime model switching without code changes.
/// Reads/writes AI.* settings via ISettingsService. Provides a structured API over
/// the raw key-value settings, with validation and caching.
/// </summary>
public class ModelRegistryService : IModelRegistryService
{
    private readonly ISettingsService _settings;
    private readonly IAIProviderFactory _providerFactory;
    private readonly ILogger<ModelRegistryService> _logger;

    // Default models per provider (used when no DB override exists)
    private static readonly Dictionary<string, string> DefaultModels = new()
    {
        ["openai"] = "gpt-4.1",
        ["claude"] = "claude-sonnet-4-6",
        ["gemini"] = "gemini-2.5-flash",
    };

    // Known available models per provider
    private static readonly Dictionary<string, List<ModelInfo>> KnownModels = new()
    {
        ["openai"] = new List<ModelInfo>
        {
            new() { Id = "gpt-4.1", DisplayName = "GPT-4.1", Provider = "openai", Tier = "standard", MaxOutputTokens = 32768, SupportsVision = true, SupportsToolUse = true, SupportsStructuredOutput = true },
            new() { Id = "gpt-4.1-mini", DisplayName = "GPT-4.1 Mini", Provider = "openai", Tier = "fast", MaxOutputTokens = 16384, SupportsVision = true, SupportsToolUse = true, SupportsStructuredOutput = true },
            new() { Id = "gpt-4.1-nano", DisplayName = "GPT-4.1 Nano", Provider = "openai", Tier = "fast", MaxOutputTokens = 16384, SupportsVision = false, SupportsToolUse = true, SupportsStructuredOutput = true },
            new() { Id = "o3", DisplayName = "o3 (Reasoning)", Provider = "openai", Tier = "heavy", MaxOutputTokens = 100000, SupportsVision = true, SupportsToolUse = true, SupportsStructuredOutput = true },
            new() { Id = "o4-mini", DisplayName = "o4-mini (Reasoning)", Provider = "openai", Tier = "standard", MaxOutputTokens = 100000, SupportsVision = true, SupportsToolUse = true, SupportsStructuredOutput = true },
        },
        ["claude"] = new List<ModelInfo>
        {
            new() { Id = "claude-sonnet-4-6", DisplayName = "Claude Sonnet 4.6", Provider = "claude", Tier = "standard", MaxOutputTokens = 16000, SupportsVision = true, SupportsToolUse = true, SupportsStructuredOutput = true },
            new() { Id = "claude-opus-4-6", DisplayName = "Claude Opus 4.6", Provider = "claude", Tier = "heavy", MaxOutputTokens = 32000, SupportsVision = true, SupportsToolUse = true, SupportsStructuredOutput = true },
            new() { Id = "claude-haiku-4-5-20251001", DisplayName = "Claude Haiku 4.5", Provider = "claude", Tier = "fast", MaxOutputTokens = 8192, SupportsVision = true, SupportsToolUse = true, SupportsStructuredOutput = true },
        },
        ["gemini"] = new List<ModelInfo>
        {
            new() { Id = "gemini-2.5-flash", DisplayName = "Gemini 2.5 Flash", Provider = "gemini", Tier = "fast", MaxOutputTokens = 8192, SupportsVision = true, SupportsToolUse = true, SupportsStructuredOutput = true },
            new() { Id = "gemini-2.5-pro", DisplayName = "Gemini 2.5 Pro", Provider = "gemini", Tier = "standard", MaxOutputTokens = 8192, SupportsVision = true, SupportsToolUse = true, SupportsStructuredOutput = true },
        },
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ModelRegistryService(
        ISettingsService settings,
        IAIProviderFactory providerFactory,
        ILogger<ModelRegistryService> logger)
    {
        _settings = settings;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    public async Task<Result<ModelRegistrySnapshot>> GetCurrentConfigAsync(CancellationToken ct = default)
    {
        try
        {
            var snapshot = new ModelRegistrySnapshot();

            // Active provider
            var activeResult = await _settings.GetSettingAsync("AI.ActiveProvider", ct);
            snapshot.ActiveProvider = activeResult.IsSuccess && !string.IsNullOrEmpty(activeResult.Value)
                ? activeResult.Value
                : "openai";

            // Fallback providers
            var fallbackResult = await _settings.GetSettingAsync("AI.FallbackProviders", ct);
            if (fallbackResult.IsSuccess && !string.IsNullOrEmpty(fallbackResult.Value))
            {
                snapshot.FallbackProviders = JsonSerializer.Deserialize<List<string>>(fallbackResult.Value, JsonOptions) ?? new();
            }

            // Per-provider model configs
            foreach (var (provider, models) in KnownModels)
            {
                var modelResult = await _settings.GetSettingAsync($"AI.Models.{provider}", ct);
                var currentModel = modelResult.IsSuccess && !string.IsNullOrEmpty(modelResult.Value)
                    ? modelResult.Value
                    : DefaultModels.GetValueOrDefault(provider, models[0].Id);

                var isAvailable = _providerFactory.GetProvider(Enum.TryParse<Sqordia.Domain.Enums.AIProviderType>(provider, true, out var pt) ? pt : 0) != null;

                snapshot.ProviderModels[provider] = new ProviderModelConfig
                {
                    Provider = provider,
                    CurrentModel = currentModel,
                    DefaultModel = DefaultModels.GetValueOrDefault(provider, models[0].Id),
                    IsAvailable = isAvailable,
                    AvailableModels = models
                };
            }

            // Section overrides
            var overridesResult = await _settings.GetSettingAsync("AI.SectionOverrides", ct);
            if (overridesResult.IsSuccess && !string.IsNullOrEmpty(overridesResult.Value))
            {
                var overrides = JsonSerializer.Deserialize<Dictionary<string, SectionOverrideConfig>>(overridesResult.Value, JsonOptions);
                if (overrides != null)
                {
                    foreach (var (section, config) in overrides)
                    {
                        snapshot.SectionOverrides[section] = new SectionModelOverride
                        {
                            SectionType = section,
                            Provider = config.Provider,
                            Model = config.Model
                        };
                    }
                }
            }

            return Result.Success(snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get model registry config");
            return Result.Failure<ModelRegistrySnapshot>(Error.Failure("ModelRegistry.Error", ex.Message));
        }
    }

    public async Task<Result<string>> GetModelForProviderAsync(string provider, CancellationToken ct = default)
    {
        var key = $"AI.Models.{provider.ToLower()}";
        var result = await _settings.GetSettingAsync(key, ct);

        if (result.IsSuccess && !string.IsNullOrEmpty(result.Value))
            return Result.Success(result.Value);

        // Return default
        var defaultModel = DefaultModels.GetValueOrDefault(provider.ToLower(), "");
        return string.IsNullOrEmpty(defaultModel)
            ? Result.Failure<string>(Error.NotFound("ModelRegistry.NotFound", $"No model configured for provider: {provider}"))
            : Result.Success(defaultModel);
    }

    public async Task<Result> SetModelForProviderAsync(string provider, string model, CancellationToken ct = default)
    {
        var providerLower = provider.ToLower();

        // Validate provider
        if (!KnownModels.ContainsKey(providerLower))
            return Result.Failure(Error.Validation("ModelRegistry.InvalidProvider", $"Unknown provider: {provider}. Valid: {string.Join(", ", KnownModels.Keys)}"));

        // Validate model belongs to provider
        var validModels = KnownModels[providerLower];
        if (!validModels.Any(m => m.Id == model))
        {
            _logger.LogWarning("Model {Model} not in known list for {Provider}, allowing anyway (may be a new model)", model, provider);
        }

        var key = $"AI.Models.{providerLower}";
        var result = await _settings.UpsertSettingAsync(key, model, "AI", $"Active model for {provider}", cancellationToken: ct);

        if (result.IsSuccess)
        {
            _providerFactory.InvalidateCache();
            _logger.LogInformation("Model for {Provider} updated to {Model}", provider, model);
        }

        return result;
    }

    public async Task<Result> SetActiveProviderAsync(string provider, CancellationToken ct = default)
    {
        var providerLower = provider.ToLower();
        if (!KnownModels.ContainsKey(providerLower) && providerLower != "ollama")
            return Result.Failure(Error.Validation("ModelRegistry.InvalidProvider", $"Unknown provider: {provider}"));

        var result = await _settings.UpsertSettingAsync("AI.ActiveProvider", provider, "AI", "Active AI provider", cancellationToken: ct);

        if (result.IsSuccess)
        {
            _providerFactory.InvalidateCache();
            _logger.LogInformation("Active provider changed to {Provider}", provider);
        }

        return result;
    }

    public async Task<Result> SetFallbackProvidersAsync(List<string> providers, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(providers, JsonOptions);
        var result = await _settings.UpsertSettingAsync("AI.FallbackProviders", json, "AI", "Fallback AI providers", cancellationToken: ct);

        if (result.IsSuccess)
        {
            _providerFactory.InvalidateCache();
            _logger.LogInformation("Fallback providers updated to: {Providers}", string.Join(", ", providers));
        }

        return result;
    }

    public async Task<Result> SetSectionOverrideAsync(string sectionType, string provider, string? model, CancellationToken ct = default)
    {
        // Load current overrides
        var overridesResult = await _settings.GetSettingAsync("AI.SectionOverrides", ct);
        var overrides = new Dictionary<string, SectionOverrideConfig>();

        if (overridesResult.IsSuccess && !string.IsNullOrEmpty(overridesResult.Value))
        {
            overrides = JsonSerializer.Deserialize<Dictionary<string, SectionOverrideConfig>>(overridesResult.Value, JsonOptions)
                ?? new();
        }

        overrides[sectionType] = new SectionOverrideConfig { Provider = provider, Model = model };

        var json = JsonSerializer.Serialize(overrides, JsonOptions);
        var result = await _settings.UpsertSettingAsync("AI.SectionOverrides", json, "AI", "Section-specific AI provider overrides", cancellationToken: ct);

        if (result.IsSuccess)
        {
            _providerFactory.InvalidateCache();
            _logger.LogInformation("Section override set: {Section} -> {Provider}/{Model}", sectionType, provider, model);
        }

        return result;
    }

    public async Task<Result> RemoveSectionOverrideAsync(string sectionType, CancellationToken ct = default)
    {
        var overridesResult = await _settings.GetSettingAsync("AI.SectionOverrides", ct);
        var overrides = new Dictionary<string, SectionOverrideConfig>();

        if (overridesResult.IsSuccess && !string.IsNullOrEmpty(overridesResult.Value))
        {
            overrides = JsonSerializer.Deserialize<Dictionary<string, SectionOverrideConfig>>(overridesResult.Value, JsonOptions)
                ?? new();
        }

        if (!overrides.Remove(sectionType))
            return Result.Failure(Error.NotFound("ModelRegistry.NotFound", $"No override exists for section: {sectionType}"));

        var json = JsonSerializer.Serialize(overrides, JsonOptions);
        var result = await _settings.UpsertSettingAsync("AI.SectionOverrides", json, "AI", "Section-specific AI provider overrides", cancellationToken: ct);

        if (result.IsSuccess)
        {
            _providerFactory.InvalidateCache();
            _logger.LogInformation("Section override removed: {Section}", sectionType);
        }

        return result;
    }

    public List<ModelInfo> GetAvailableModels(string provider)
    {
        return KnownModels.GetValueOrDefault(provider.ToLower(), new List<ModelInfo>());
    }

    public void InvalidateCache()
    {
        _providerFactory.InvalidateCache();
    }
}
