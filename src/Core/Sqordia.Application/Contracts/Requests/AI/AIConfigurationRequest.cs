namespace Sqordia.Contracts.Requests.AI;

/// <summary>
/// Request to update AI provider configuration
/// </summary>
public class AIConfigurationRequest
{
    /// <summary>
    /// Active AI provider name (OpenAI, Claude, or Gemini)
    /// </summary>
    public required string ActiveProvider { get; set; }

    /// <summary>
    /// Fallback providers in order of preference
    /// </summary>
    public required List<string> FallbackProviders { get; set; }

    /// <summary>
    /// Provider-specific settings
    /// </summary>
    public required Dictionary<string, ProviderSettingsRequest> Providers { get; set; }
}

/// <summary>
/// Settings for a specific AI provider
/// </summary>
public class ProviderSettingsRequest
{
    /// <summary>
    /// API key for the provider (null = don't update)
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Model name to use for this provider
    /// </summary>
    public required string Model { get; set; }
}
