using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Service for managing AI model configurations at runtime.
/// Model configs are stored in the DB via ISettingsService, allowing
/// admins to switch models without code changes or redeployment.
/// </summary>
public interface IModelRegistryService
{
    /// <summary>
    /// Get the current model configuration for all providers
    /// </summary>
    Task<Result<ModelRegistrySnapshot>> GetCurrentConfigAsync(CancellationToken ct = default);

    /// <summary>
    /// Get the model to use for a given provider (respects overrides)
    /// </summary>
    Task<Result<string>> GetModelForProviderAsync(string provider, CancellationToken ct = default);

    /// <summary>
    /// Update the model for a specific provider
    /// </summary>
    Task<Result> SetModelForProviderAsync(string provider, string model, CancellationToken ct = default);

    /// <summary>
    /// Update the active provider
    /// </summary>
    Task<Result> SetActiveProviderAsync(string provider, CancellationToken ct = default);

    /// <summary>
    /// Update fallback providers
    /// </summary>
    Task<Result> SetFallbackProvidersAsync(List<string> providers, CancellationToken ct = default);

    /// <summary>
    /// Set section-specific model override
    /// </summary>
    Task<Result> SetSectionOverrideAsync(string sectionType, string provider, string? model, CancellationToken ct = default);

    /// <summary>
    /// Remove a section override
    /// </summary>
    Task<Result> RemoveSectionOverrideAsync(string sectionType, CancellationToken ct = default);

    /// <summary>
    /// Get available models for a provider
    /// </summary>
    List<ModelInfo> GetAvailableModels(string provider);

    /// <summary>
    /// Force refresh cached configuration
    /// </summary>
    void InvalidateCache();
}

/// <summary>
/// Snapshot of the full model registry configuration
/// </summary>
public class ModelRegistrySnapshot
{
    public string ActiveProvider { get; set; } = string.Empty;
    public List<string> FallbackProviders { get; set; } = new();
    public Dictionary<string, ProviderModelConfig> ProviderModels { get; set; } = new();
    public Dictionary<string, SectionModelOverride> SectionOverrides { get; set; } = new();
}

/// <summary>
/// Model configuration for a single provider
/// </summary>
public class ProviderModelConfig
{
    public string Provider { get; set; } = string.Empty;
    public string CurrentModel { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public List<ModelInfo> AvailableModels { get; set; } = new();
}

/// <summary>
/// Information about an available model
/// </summary>
public class ModelInfo
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty; // "standard", "heavy", "fast"
    public int MaxOutputTokens { get; set; }
    public bool SupportsVision { get; set; }
    public bool SupportsToolUse { get; set; }
    public bool SupportsStructuredOutput { get; set; }
}

/// <summary>
/// Section-specific model override
/// </summary>
public class SectionModelOverride
{
    public string SectionType { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? Model { get; set; }
}
