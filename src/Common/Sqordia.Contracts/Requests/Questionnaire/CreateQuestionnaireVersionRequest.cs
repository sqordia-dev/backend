using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Questionnaire;

/// <summary>
/// Request to create a new questionnaire version (draft)
/// </summary>
public class CreateQuestionnaireVersionRequest
{
    /// <summary>
    /// Optional notes describing this version
    /// </summary>
    [StringLength(1000, ErrorMessage = "Notes must not exceed 1000 characters")]
    public string? Notes { get; set; }
}
