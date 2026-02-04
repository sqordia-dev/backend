namespace Sqordia.Contracts.Responses.Admin;

public class QuestionTemplateDto
{
    public required Guid Id { get; set; }
    public string? PersonaType { get; set; }
    public required int StepNumber { get; set; }
    public required string QuestionText { get; set; }
    public string? QuestionTextEN { get; set; }
    public string? HelpText { get; set; }
    public string? HelpTextEN { get; set; }
    public required string QuestionType { get; set; }
    public required int Order { get; set; }
    public required bool IsRequired { get; set; }
    public string? Section { get; set; }
    public string? Options { get; set; }
    public string? OptionsEN { get; set; }
    public string? ValidationRules { get; set; }
    public string? ConditionalLogic { get; set; }
    public string? Icon { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
