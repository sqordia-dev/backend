namespace Sqordia.Contracts.Responses.BusinessPlan;

public class QuestionnaireQuestionResponse
{
    public Guid Id { get; set; }
    public required string QuestionText { get; set; }
    public string? HelpText { get; set; }
    public required string QuestionType { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public string? Section { get; set; }
    public List<string>? Options { get; set; }
    public string? UserResponse { get; set; }
    public decimal? NumericValue { get; set; }
    public DateTime? DateValue { get; set; }
    public bool? BooleanValue { get; set; }
    public List<string>? SelectedOptions { get; set; }
}

