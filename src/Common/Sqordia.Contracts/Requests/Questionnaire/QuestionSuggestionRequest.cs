using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Questionnaire;

/// <summary>
/// Request to generate AI suggestions for a questionnaire question
/// </summary>
public class QuestionSuggestionRequest
{
    /// <summary>
    /// The question text to generate suggestions for
    /// </summary>
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public required string QuestionText { get; set; }
    
    /// <summary>
    /// The business plan type to provide context for suggestions
    /// </summary>
    [Required]
    public required string PlanType { get; set; }
    
    /// <summary>
    /// Optional: Any existing response to the question for context
    /// </summary>
    [StringLength(2000)]
    public string? ExistingResponse { get; set; }
    
    /// <summary>
    /// Optional: Organization context for more relevant suggestions
    /// </summary>
    [StringLength(500)]
    public string? OrganizationContext { get; set; }
    
    /// <summary>
    /// Number of suggestions to generate (1-5, default: 3)
    /// </summary>
    [Range(1, 5)]
    public int SuggestionCount { get; set; } = 3;
    
    /// <summary>
    /// Language for the suggestions (fr or en)
    /// </summary>
    [Required]
    [StringLength(2, MinimumLength = 2)]
    public required string Language { get; set; }
}
