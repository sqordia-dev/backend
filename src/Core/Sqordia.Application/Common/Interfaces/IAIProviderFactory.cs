using Sqordia.Application.Services;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Common.Interfaces;

public interface IAIProviderFactory
{
    /// <summary>
    /// Gets the currently active AI provider based on settings
    /// </summary>
    Task<IAIService?> GetActiveProviderAsync();

    /// <summary>
    /// Gets the AI provider for a specific section, respecting section overrides
    /// </summary>
    /// <param name="sectionType">The section type to get a provider for</param>
    /// <returns>The section-specific provider if configured, otherwise the active provider</returns>
    Task<IAIService?> GetProviderForSectionAsync(SectionType sectionType);

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
