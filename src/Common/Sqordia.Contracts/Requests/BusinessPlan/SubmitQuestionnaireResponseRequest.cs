using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.BusinessPlan;

public class SubmitQuestionnaireResponseRequest
{
    [Required]
    public required Guid QuestionTemplateId { get; set; }
    
    [Required]
    public required string ResponseText { get; set; }
    
    public decimal? NumericValue { get; set; }
    public DateTime? DateValue { get; set; }
    public bool? BooleanValue { get; set; }
    public List<string>? SelectedOptions { get; set; }
}

