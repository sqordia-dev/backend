namespace Sqordia.Contracts.Responses.AI;

/// <summary>
/// Response containing current AI provider configuration
/// </summary>
public class AIConfigurationResponse
{
    /// <summary>
    /// Currently active AI provider
    /// </summary>
    public required string ActiveProvider { get; set; }

    /// <summary>
    /// Fallback providers in order
    /// </summary>
    public required List<string> FallbackProviders { get; set; }

    /// <summary>
    /// Provider information
    /// </summary>
    public required Dictionary<string, ProviderInfo> Providers { get; set; }
}

/// <summary>
/// Information about a configured AI provider
/// </summary>
public class ProviderInfo
{
    /// <summary>
    /// Whether this provider is configured with an API key
    /// </summary>
    public required bool IsConfigured { get; set; }

    /// <summary>
    /// Model being used
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// Masked preview of the API key (e.g., "sk-...xyz")
    /// </summary>
    public required string ApiKeyPreview { get; set; }

    /// <summary>
    /// Last time the provider was tested
    /// </summary>
    public DateTime? LastTested { get; set; }

    /// <summary>
    /// Result of the last test (true = success, false = failure, null = never tested)
    /// </summary>
    public bool? LastTestSuccess { get; set; }
}
