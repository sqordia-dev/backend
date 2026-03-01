namespace Sqordia.Contracts.Requests.Admin.QuestionnaireV3;

/// <summary>
/// Request to create a new V3 question template
/// </summary>
public record CreateQuestionTemplateV3Request
{
    public int QuestionNumber { get; init; }
    public string? PersonaType { get; init; }
    public int StepNumber { get; init; }
    public string QuestionTextFR { get; init; } = null!;
    public string QuestionTextEN { get; init; } = null!;
    public string? HelpTextFR { get; init; }
    public string? HelpTextEN { get; init; }
    public string QuestionType { get; init; } = null!;
    public string? OptionsFR { get; init; }
    public string? OptionsEN { get; init; }
    public string? ValidationRules { get; init; }
    public string? ConditionalLogic { get; init; }
    public string? CoachPromptFR { get; init; }
    public string? CoachPromptEN { get; init; }
    public string? ExpertAdviceFR { get; init; }
    public string? ExpertAdviceEN { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsRequired { get; init; } = true;
    public string? Icon { get; init; }
}

/// <summary>
/// Request to update a V3 question template
/// </summary>
public record UpdateQuestionTemplateV3Request
{
    public string QuestionTextFR { get; init; } = null!;
    public string QuestionTextEN { get; init; } = null!;
    public string? HelpTextFR { get; init; }
    public string? HelpTextEN { get; init; }
    public string QuestionType { get; init; } = null!;
    public string? OptionsFR { get; init; }
    public string? OptionsEN { get; init; }
    public string? ValidationRules { get; init; }
    public string? ConditionalLogic { get; init; }
    public string? ExpertAdviceFR { get; init; }
    public string? ExpertAdviceEN { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsRequired { get; init; }
    public bool IsActive { get; init; }
    public string? Icon { get; init; }
}

/// <summary>
/// Request to update coach prompts for a question
/// </summary>
public record UpdateCoachPromptRequest
{
    public string? CoachPromptFR { get; init; }
    public string? CoachPromptEN { get; init; }
}

/// <summary>
/// Request to get AI coach suggestion
/// </summary>
public record GetCoachSuggestionRequest
{
    public string CurrentAnswer { get; init; } = null!;
    public string Language { get; init; } = "fr";
}

/// <summary>
/// Filter for listing V3 questions
/// </summary>
public record QuestionTemplateV3FilterRequest
{
    public string? PersonaType { get; init; }
    public int? StepNumber { get; init; }
    public string? QuestionType { get; init; }
    public bool? IsActive { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
