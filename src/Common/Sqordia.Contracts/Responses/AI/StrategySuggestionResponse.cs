namespace Sqordia.Contracts.Responses.AI;

/// <summary>
/// Response containing AI-generated strategy suggestions
/// </summary>
public class StrategySuggestionResponse
{
    /// <summary>
    /// The business plan ID
    /// </summary>
    public required Guid BusinessPlanId { get; set; }
    
    /// <summary>
    /// List of strategy suggestions
    /// </summary>
    public required List<StrategySuggestion> Suggestions { get; set; }
    
    /// <summary>
    /// Timestamp when suggestions were generated
    /// </summary>
    public required DateTime GeneratedAt { get; set; }
    
    /// <summary>
    /// AI model used for generation
    /// </summary>
    public required string Model { get; set; }
    
    /// <summary>
    /// Language of the generated suggestions
    /// </summary>
    public required string Language { get; set; }
}

/// <summary>
/// Individual strategy suggestion
/// </summary>
public class StrategySuggestion
{
    /// <summary>
    /// The strategy title
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Detailed description of the strategy
    /// </summary>
    public required string Description { get; set; }
    
    /// <summary>
    /// Category of the strategy (e.g., "growth", "marketing", "operations", "financial")
    /// </summary>
    public required string Category { get; set; }
    
    /// <summary>
    /// Priority level (High, Medium, Low)
    /// </summary>
    public required string Priority { get; set; }
    
    /// <summary>
    /// Expected impact of implementing this strategy
    /// </summary>
    public required string ExpectedImpact { get; set; }
    
    /// <summary>
    /// Implementation steps or recommendations
    /// </summary>
    public required List<string> ImplementationSteps { get; set; }
    
    /// <summary>
    /// Estimated time to implement (e.g., "1-3 months", "6-12 months")
    /// </summary>
    public string? EstimatedTimeframe { get; set; }
    
    /// <summary>
    /// Reasoning behind this suggestion
    /// </summary>
    public required string Reasoning { get; set; }
}

