namespace Sqordia.Functions.AIGenerationHandler.Configuration;

/// <summary>
/// AI Generation function configuration settings
/// </summary>
public class AIGenerationConfiguration
{
    public string OpenAISecretName { get; set; } = "openai-api-key";
    public string ClaudeSecretName { get; set; } = "claude-api-key";
    public string GeminiSecretName { get; set; } = "gemini-api-key";
    public string DefaultAiProvider { get; set; } = "openai";
    public string KeyVaultUrl { get; set; } = string.Empty;
    public string WebApiBaseUrl { get; set; } = string.Empty;
    public string SystemApiKeySecretName { get; set; } = "system-api-key";
    public int HttpTimeoutSeconds { get; set; } = 300; // 5 minutes for long-running generation
}

