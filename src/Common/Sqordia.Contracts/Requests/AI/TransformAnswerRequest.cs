using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.AI;

/// <summary>
/// Request to transform/enhance an answer using AI with various actions
/// </summary>
public class TransformAnswerRequest
{
    /// <summary>
    /// The question ID this answer relates to
    /// </summary>
    [Required]
    public required string QuestionId { get; set; }

    /// <summary>
    /// The question number (1-22) for context mapping
    /// </summary>
    public int? QuestionNumber { get; set; }

    /// <summary>
    /// The answer text to transform (can be empty for 'generate' action)
    /// </summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// The question text (used for context, especially for 'generate' action)
    /// </summary>
    public string? QuestionText { get; set; }

    /// <summary>
    /// The transformation action: polish, shorten, expand, professional, examples, simplify
    /// </summary>
    [Required]
    public required string Action { get; set; }

    /// <summary>
    /// Optional context about the question
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// User persona: Entrepreneur, Consultant, OBNL
    /// </summary>
    public string Persona { get; set; } = "Entrepreneur";

    /// <summary>
    /// Language for the transformed output: en, fr
    /// </summary>
    public string Language { get; set; } = "en";

    /// <summary>
    /// Accumulated answers from previous questions for context.
    /// Key: Question number (1-22), Value: User's answer text
    /// </summary>
    public Dictionary<int, string>? PreviousAnswers { get; set; }

    /// <summary>
    /// Business name extracted from Q1 for personalization
    /// </summary>
    public string? BusinessName { get; set; }

    /// <summary>
    /// Business sector/industry from Q5 for context
    /// </summary>
    public string? BusinessSector { get; set; }
}

/// <summary>
/// Valid transformation action types
/// </summary>
public static class TransformActionTypes
{
    public const string Generate = "generate";
    public const string Polish = "polish";
    public const string Shorten = "shorten";
    public const string Expand = "expand";
    public const string Professional = "professional";
    public const string Examples = "examples";
    public const string Simplify = "simplify";

    public static readonly string[] AllActions = { Generate, Polish, Shorten, Expand, Professional, Examples, Simplify };

    public static bool IsValid(string action) =>
        AllActions.Contains(action, StringComparer.OrdinalIgnoreCase);
}
