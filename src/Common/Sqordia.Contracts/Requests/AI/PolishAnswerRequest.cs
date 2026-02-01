using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.AI;

/// <summary>
/// Request to polish/enhance an answer using AI
/// </summary>
public class PolishAnswerRequest
{
    /// <summary>
    /// The question ID this answer relates to
    /// </summary>
    [Required]
    public required string QuestionId { get; set; }

    /// <summary>
    /// The answer text to polish
    /// </summary>
    [Required]
    [MinLength(10, ErrorMessage = "Answer must be at least 10 characters")]
    public required string Answer { get; set; }

    /// <summary>
    /// Optional context about the question
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// User persona: Entrepreneur, Consultant, OBNL
    /// </summary>
    public string Persona { get; set; } = "Entrepreneur";

    /// <summary>
    /// User location information
    /// </summary>
    public LocationInfo? Location { get; set; }

    /// <summary>
    /// Language for the polished output: en, fr
    /// </summary>
    public string Language { get; set; } = "en";
}
