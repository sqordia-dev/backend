namespace Sqordia.Contracts.Responses.Sections;

/// <summary>
/// Response containing AI-improved section content
/// </summary>
public class SectionImprovementResponse
{
    /// <summary>
    /// The original content that was improved
    /// </summary>
    public required string OriginalContent { get; set; }
    
    /// <summary>
    /// The AI-improved content
    /// </summary>
    public required string ImprovedContent { get; set; }
    
    /// <summary>
    /// The type of improvement applied
    /// </summary>
    public required string ImprovementType { get; set; }
    
    /// <summary>
    /// Language of the improved content
    /// </summary>
    public required string Language { get; set; }
    
    /// <summary>
    /// Business plan type context
    /// </summary>
    public required string PlanType { get; set; }
    
    /// <summary>
    /// AI model used for the improvement
    /// </summary>
    public required string Model { get; set; }
    
    /// <summary>
    /// Timestamp when the improvement was generated
    /// </summary>
    public required DateTime GeneratedAt { get; set; }
    
    /// <summary>
    /// Confidence score for the improvement (0.0 to 1.0)
    /// </summary>
    public required double Confidence { get; set; }
    
    /// <summary>
    /// Explanation of what was improved
    /// </summary>
    public required string ImprovementExplanation { get; set; }
    
    /// <summary>
    /// Suggestions for further improvements
    /// </summary>
    public List<string> FurtherSuggestions { get; set; } = new();
    
    /// <summary>
    /// Word count of the improved content
    /// </summary>
    public required int WordCount { get; set; }
    
    /// <summary>
    /// Reading level of the improved content
    /// </summary>
    public string? ReadingLevel { get; set; }
    
    /// <summary>
    /// Time taken to generate the improvement
    /// </summary>
    public required TimeSpan ProcessingTime { get; set; }
}

/// <summary>
/// Response for section expansion
/// </summary>
public class SectionExpansionResponse : SectionImprovementResponse
{
    /// <summary>
    /// Additional subsections added during expansion
    /// </summary>
    public List<string> AddedSubsections { get; set; } = new();
    
    /// <summary>
    /// Key points that were expanded
    /// </summary>
    public List<string> ExpandedPoints { get; set; } = new();
}

/// <summary>
/// Response for section simplification
/// </summary>
public class SectionSimplificationResponse : SectionImprovementResponse
{
    /// <summary>
    /// Complex terms that were simplified
    /// </summary>
    public List<string> SimplifiedTerms { get; set; } = new();
    
    /// <summary>
    /// Removed technical jargon
    /// </summary>
    public List<string> RemovedJargon { get; set; } = new();
    
    /// <summary>
    /// Original complexity score (before simplification)
    /// </summary>
    public required double OriginalComplexity { get; set; }
    
    /// <summary>
    /// New complexity score (after simplification)
    /// </summary>
    public required double NewComplexity { get; set; }
}
