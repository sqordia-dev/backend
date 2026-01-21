using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// V2 Question template with persona support for the Growth Architect wizard
/// Supports the 5-step questionnaire flow with 20 questions
/// </summary>
public class QuestionTemplateV2 : BaseAuditableEntity
{
    /// <summary>
    /// The persona type this question is targeted for (null means applicable to all)
    /// </summary>
    public PersonaType? PersonaType { get; private set; }

    /// <summary>
    /// The step number in the wizard (1-5)
    /// </summary>
    public int StepNumber { get; private set; }

    /// <summary>
    /// Question text in French (primary)
    /// </summary>
    public string QuestionText { get; private set; } = null!;

    /// <summary>
    /// Question text in English
    /// </summary>
    public string? QuestionTextEN { get; private set; }

    /// <summary>
    /// Help text in French
    /// </summary>
    public string? HelpText { get; private set; }

    /// <summary>
    /// Help text in English
    /// </summary>
    public string? HelpTextEN { get; private set; }

    /// <summary>
    /// Type of question (ShortText, LongText, SingleChoice, etc.)
    /// </summary>
    public QuestionType QuestionType { get; private set; }

    /// <summary>
    /// Display order within the step
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// Whether this question is required
    /// </summary>
    public bool IsRequired { get; private set; }

    /// <summary>
    /// Section grouping within the step (e.g., "Identity & Vision", "The Offering")
    /// </summary>
    public string? Section { get; private set; }

    /// <summary>
    /// JSON array of options for choice-type questions (French)
    /// </summary>
    public string? Options { get; private set; }

    /// <summary>
    /// JSON array of options for choice-type questions (English)
    /// </summary>
    public string? OptionsEN { get; private set; }

    /// <summary>
    /// JSON validation rules (min/max length, regex patterns, etc.)
    /// </summary>
    public string? ValidationRules { get; private set; }

    /// <summary>
    /// JSON conditional logic for showing/hiding based on other answers
    /// </summary>
    public string? ConditionalLogic { get; private set; }

    /// <summary>
    /// Whether this question is active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Emoji icon for visual display
    /// </summary>
    public string? Icon { get; private set; }

    private QuestionTemplateV2() { } // EF Core constructor

    public QuestionTemplateV2(
        int stepNumber,
        string questionText,
        QuestionType questionType,
        int order,
        bool isRequired = true,
        string? section = null,
        PersonaType? personaType = null)
    {
        if (stepNumber < 1 || stepNumber > 5)
            throw new ArgumentException("Step number must be between 1 and 5");

        PersonaType = personaType;
        StepNumber = stepNumber;
        QuestionText = questionText ?? throw new ArgumentNullException(nameof(questionText));
        QuestionType = questionType;
        Order = order;
        IsRequired = isRequired;
        Section = section;
        IsActive = true;
    }

    public void SetEnglishText(string? questionTextEN, string? helpTextEN = null, string? optionsEN = null)
    {
        QuestionTextEN = questionTextEN;
        HelpTextEN = helpTextEN;
        OptionsEN = optionsEN;
    }

    public void SetHelpText(string? helpText, string? helpTextEN = null)
    {
        HelpText = helpText;
        if (helpTextEN != null)
            HelpTextEN = helpTextEN;
    }

    public void SetOptions(string? options, string? optionsEN = null)
    {
        Options = options;
        if (optionsEN != null)
            OptionsEN = optionsEN;
    }

    public void SetValidationRules(string? validationRules)
    {
        ValidationRules = validationRules;
    }

    public void SetConditionalLogic(string? conditionalLogic)
    {
        ConditionalLogic = conditionalLogic;
    }

    public void SetIcon(string? icon)
    {
        Icon = icon;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void UpdateOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("Order must be non-negative");
        Order = order;
    }

    public void UpdateSection(string? section)
    {
        Section = section;
    }
}
