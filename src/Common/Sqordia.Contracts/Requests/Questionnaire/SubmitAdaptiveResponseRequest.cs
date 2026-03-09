using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Questionnaire;

public class SubmitAdaptiveResponseRequest
{
    [Required]
    public Guid QuestionId { get; set; }

    public int? QuestionNumber { get; set; }

    [Required]
    [MaxLength(10000)]
    public required string ResponseText { get; set; }

    public bool WriteBackToProfile { get; set; }
}
