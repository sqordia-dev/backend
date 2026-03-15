namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Resolves AI provider API keys and models at runtime.
/// DB Settings table is authoritative; falls back to environment variables / IOptions.
/// </summary>
public interface IAIKeyResolver
{
    Task<AIProviderConfig> ResolveOpenAIAsync(CancellationToken ct = default);
    Task<AIProviderConfig> ResolveClaudeAsync(CancellationToken ct = default);
    Task<AIProviderConfig> ResolveGeminiAsync(CancellationToken ct = default);
    void InvalidateCache();
}

/// <summary>
/// Resolved API key and model for an AI provider
/// </summary>
public record AIProviderConfig(string ApiKey, string Model);

/// <summary>
/// Implemented by AI services that support runtime reconfiguration of API key / model
/// </summary>
public interface IReconfigurableAIService
{
    void Reconfigure(string apiKey, string model);
}
