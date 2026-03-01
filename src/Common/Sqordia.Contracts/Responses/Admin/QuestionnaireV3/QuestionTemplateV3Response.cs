namespace Sqordia.Contracts.Responses.Admin.QuestionnaireV3;

/// <summary>
/// Full response DTO for a V3 question template
/// </summary>
public record QuestionTemplateV3Response
{
    public Guid Id { get; init; }
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
    public bool IsRequired { get; init; }
    public bool IsActive { get; init; }
    public string? Icon { get; init; }
    public DateTime Created { get; init; }
    public DateTime? LastModified { get; init; }
    public List<QuestionMappingSummaryResponse> SectionMappings { get; init; } = new();
}

/// <summary>
/// Lightweight list response for V3 questions
/// </summary>
public record QuestionTemplateV3ListResponse
{
    public Guid Id { get; init; }
    public int QuestionNumber { get; init; }
    public string? PersonaType { get; init; }
    public int StepNumber { get; init; }
    public string QuestionTextFR { get; init; } = null!;
    public string QuestionTextEN { get; init; } = null!;
    public string QuestionType { get; init; } = null!;
    public bool IsRequired { get; init; }
    public bool IsActive { get; init; }
    public int DisplayOrder { get; init; }
    public int MappingsCount { get; init; }
}

/// <summary>
/// Summary of a question's section mapping for nested display
/// </summary>
public record QuestionMappingSummaryResponse
{
    public Guid MappingId { get; init; }
    public Guid SubSectionId { get; init; }
    public string SubSectionCode { get; init; } = null!;
    public string SubSectionTitleFR { get; init; } = null!;
    public string SubSectionTitleEN { get; init; } = null!;
    public string? MappingContext { get; init; }
    public decimal? Weight { get; init; }
}

/// <summary>
/// Response for coach suggestion request
/// </summary>
public record CoachSuggestionResponse
{
    public bool Success { get; init; }
    public string? Suggestion { get; init; }
    public string? ErrorMessage { get; init; }
    public int TokensUsed { get; init; }
}
