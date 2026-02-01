namespace Sqordia.Domain.Enums;

/// <summary>
/// Supported AI provider types for content generation
/// </summary>
public enum AIProviderType
{
    /// <summary>
    /// OpenAI (GPT models)
    /// </summary>
    OpenAI = 0,

    /// <summary>
    /// Anthropic Claude models
    /// </summary>
    Claude = 1,

    /// <summary>
    /// Google Gemini models
    /// </summary>
    Gemini = 2
}
