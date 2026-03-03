namespace Sqordia.Contracts.Responses.Admin.PromptRegistry;

/// <summary>
/// Result of AI-powered prompt improvement
/// </summary>
public class PromptImprovementResultDto
{
    /// <summary>
    /// The improved system prompt
    /// </summary>
    public string ImprovedSystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// The improved user prompt template
    /// </summary>
    public string ImprovedUserPromptTemplate { get; set; } = string.Empty;

    /// <summary>
    /// List of improvements made with explanations
    /// </summary>
    public List<PromptImprovementExplanation> Improvements { get; set; } = new();

    /// <summary>
    /// Summary of the overall improvement
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// The model used for improvement
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Tokens used for the improvement request
    /// </summary>
    public int TokensUsed { get; set; }
}

/// <summary>
/// Explanation of a specific improvement made to the prompt
/// </summary>
public class PromptImprovementExplanation
{
    /// <summary>
    /// The area that was improved (e.g., clarity, specificity, structure)
    /// </summary>
    public string Area { get; set; } = string.Empty;

    /// <summary>
    /// Description of what was changed
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The original text (snippet)
    /// </summary>
    public string? Before { get; set; }

    /// <summary>
    /// The improved text (snippet)
    /// </summary>
    public string? After { get; set; }

    /// <summary>
    /// Reason for the improvement
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
