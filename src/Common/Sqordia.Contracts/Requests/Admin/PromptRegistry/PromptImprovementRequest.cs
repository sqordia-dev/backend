namespace Sqordia.Contracts.Requests.Admin.PromptRegistry;

/// <summary>
/// Request to improve a prompt using AI
/// </summary>
public class PromptImprovementRequest
{
    /// <summary>
    /// The system prompt to improve
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// The user prompt template to improve
    /// </summary>
    public string UserPromptTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Focus area for improvement: clarity, specificity, format, or all
    /// </summary>
    public string FocusArea { get; set; } = "all";

    /// <summary>
    /// Optional custom instructions for the improvement
    /// </summary>
    public string? CustomInstructions { get; set; }

    /// <summary>
    /// Target language for the improved prompt (en or fr)
    /// </summary>
    public string? TargetLanguage { get; set; }
}
