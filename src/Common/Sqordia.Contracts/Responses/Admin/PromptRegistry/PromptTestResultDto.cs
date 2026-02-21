namespace Sqordia.Contracts.Responses.Admin.PromptRegistry;

/// <summary>
/// Result of testing a prompt with sample data
/// </summary>
public class PromptTestResultDto
{
    /// <summary>
    /// The system prompt that was used
    /// </summary>
    public string RenderedSystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// The user prompt after template variable substitution
    /// </summary>
    public string RenderedUserPrompt { get; set; } = string.Empty;

    /// <summary>
    /// The AI-generated output
    /// </summary>
    public string Output { get; set; } = string.Empty;

    /// <summary>
    /// Number of tokens used (prompt + response)
    /// </summary>
    public int TokensUsed { get; set; }

    /// <summary>
    /// Input tokens (prompt only)
    /// </summary>
    public int InputTokens { get; set; }

    /// <summary>
    /// Output tokens (response only)
    /// </summary>
    public int OutputTokens { get; set; }

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public int ResponseTimeMs { get; set; }

    /// <summary>
    /// AI provider used for the test (OpenAI, Claude, Gemini)
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Model name used for the test
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Whether the test was successful
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Error message if the test failed
    /// </summary>
    public string? Error { get; set; }
}
