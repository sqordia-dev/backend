using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Questionnaire;

/// <summary>
/// Request to update a questionnaire step's configuration
/// </summary>
public class UpdateQuestionnaireStepRequest
{
    /// <summary>
    /// Step title in French
    /// </summary>
    [StringLength(200, ErrorMessage = "Title (FR) must not exceed 200 characters")]
    public string? TitleFR { get; set; }

    /// <summary>
    /// Step title in English
    /// </summary>
    [StringLength(200, ErrorMessage = "Title (EN) must not exceed 200 characters")]
    public string? TitleEN { get; set; }

    /// <summary>
    /// Step description in French
    /// </summary>
    [StringLength(500, ErrorMessage = "Description (FR) must not exceed 500 characters")]
    public string? DescriptionFR { get; set; }

    /// <summary>
    /// Step description in English
    /// </summary>
    [StringLength(500, ErrorMessage = "Description (EN) must not exceed 500 characters")]
    public string? DescriptionEN { get; set; }
}
