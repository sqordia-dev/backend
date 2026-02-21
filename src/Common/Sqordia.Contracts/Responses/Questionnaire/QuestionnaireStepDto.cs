namespace Sqordia.Contracts.Responses.Questionnaire;

/// <summary>
/// Represents a questionnaire step configuration
/// </summary>
public class QuestionnaireStepDto
{
    /// <summary>
    /// The step ID
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// The step number (1-5)
    /// </summary>
    public required int StepNumber { get; set; }

    /// <summary>
    /// Step title in French
    /// </summary>
    public required string TitleFR { get; set; }

    /// <summary>
    /// Step title in English
    /// </summary>
    public string? TitleEN { get; set; }

    /// <summary>
    /// Step description in French
    /// </summary>
    public string? DescriptionFR { get; set; }

    /// <summary>
    /// Step description in English
    /// </summary>
    public string? DescriptionEN { get; set; }

    /// <summary>
    /// Emoji icon for visual display
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Whether this step is active
    /// </summary>
    public required bool IsActive { get; set; }

    /// <summary>
    /// Number of questions in this step (computed)
    /// </summary>
    public int QuestionCount { get; set; }
}
