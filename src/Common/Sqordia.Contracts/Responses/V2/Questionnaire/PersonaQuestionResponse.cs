namespace Sqordia.Contracts.Responses.V2.Questionnaire;

/// <summary>
/// V2 questionnaire question with persona support
/// </summary>
public class PersonaQuestionResponse
{
    public Guid Id { get; set; }
    public string? PersonaType { get; set; }
    public int StepNumber { get; set; }
    public required string QuestionText { get; set; }
    public string? QuestionTextEN { get; set; }
    public string? HelpText { get; set; }
    public string? HelpTextEN { get; set; }
    public required string QuestionType { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public string? Section { get; set; }
    public List<string>? Options { get; set; }
    public List<string>? OptionsEN { get; set; }
    public string? Icon { get; set; }
}

/// <summary>
/// Response containing all questions for a step
/// </summary>
public class QuestionnaireStepResponse
{
    public int StepNumber { get; set; }
    public required string StepTitle { get; set; }
    public string? StepDescription { get; set; }
    public required List<PersonaQuestionResponse> Questions { get; set; }
    public int TotalQuestions { get; set; }
}

/// <summary>
/// Complete questionnaire template response
/// </summary>
public class QuestionnaireTemplateV2Response
{
    public string? PersonaType { get; set; }
    public required List<QuestionnaireStepResponse> Steps { get; set; }
    public int TotalSteps { get; set; }
    public int TotalQuestions { get; set; }
}
