namespace Sqordia.Contracts.Responses.Questionnaire;

public class AdaptiveQuestionnaireResponse
{
    public required IReadOnlyList<AdaptiveQuestionDto> Questions { get; set; }
    public required IReadOnlyList<SkippedQuestionDto> SkippedQuestions { get; set; }
    public int TotalQuestions { get; set; }
    public int RemainingQuestions { get; set; }
    public int ProfileCompletenessScore { get; set; }
}

public class AdaptiveQuestionDto
{
    public Guid Id { get; set; }
    public int QuestionNumber { get; set; }
    public int StepNumber { get; set; }
    public required string QuestionText { get; set; }
    public string? HelpText { get; set; }
    public required string QuestionType { get; set; }
    public string? Options { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }
    public string? Icon { get; set; }
    public string? SectionGroup { get; set; }
    public string? CoachPrompt { get; set; }
    public string? ExpertAdvice { get; set; }
    public string? ProfileFieldKey { get; set; }
    public string? PrefilledValue { get; set; }
    public bool IsGapQuestion { get; set; }
    public string? ExistingResponse { get; set; }
}

public class SkippedQuestionDto
{
    public Guid Id { get; set; }
    public int QuestionNumber { get; set; }
    public required string QuestionText { get; set; }
    public required string ProfileFieldKey { get; set; }
    public required string ProfileFieldValue { get; set; }
}
