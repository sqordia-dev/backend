using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.AI;

/// <summary>
/// Request to analyze a questionnaire step for gaps
/// </summary>
public class AnalyzeStepRequest
{
    /// <summary>
    /// The step number being analyzed
    /// </summary>
    [Required]
    [Range(1, 10)]
    public int StepNumber { get; set; }

    /// <summary>
    /// Answers for this step
    /// </summary>
    [Required]
    public required List<StepAnswer> Answers { get; set; }

    /// <summary>
    /// User persona: Entrepreneur, Consultant, OBNL
    /// </summary>
    public string Persona { get; set; } = "Entrepreneur";

    /// <summary>
    /// User location information
    /// </summary>
    public LocationInfo? Location { get; set; }

    /// <summary>
    /// Language for the analysis output: en, fr
    /// </summary>
    public string Language { get; set; } = "en";
}

/// <summary>
/// An answer within a step
/// </summary>
public class StepAnswer
{
    /// <summary>
    /// The question ID
    /// </summary>
    [Required]
    public required string QuestionId { get; set; }

    /// <summary>
    /// The answer text
    /// </summary>
    [Required]
    public required string Answer { get; set; }
}
