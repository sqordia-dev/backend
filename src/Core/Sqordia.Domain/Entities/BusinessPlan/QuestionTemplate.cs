using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// V3 Question template based on STRUCTURE FINALE specification
/// Supports 22 core questions with coach prompts and multi-section mappings
/// </summary>
public class QuestionTemplate : BaseAuditableEntity
{
    /// <summary>
    /// Question number from STRUCTURE FINALE (1-22)
    /// </summary>
    public int QuestionNumber { get; private set; }

    /// <summary>
    /// Persona type this question is targeted for (null means applicable to all)
    /// </summary>
    public PersonaType? PersonaType { get; private set; }

    /// <summary>
    /// Wizard step number (1-7) for grouping questions
    /// </summary>
    public int StepNumber { get; private set; }

    /// <summary>
    /// Question text in French (primary language)
    /// </summary>
    public string QuestionTextFR { get; private set; } = null!;

    /// <summary>
    /// Question text in English
    /// </summary>
    public string QuestionTextEN { get; private set; } = null!;

    /// <summary>
    /// Help text / explanation in French
    /// </summary>
    public string? HelpTextFR { get; private set; }

    /// <summary>
    /// Help text / explanation in English
    /// </summary>
    public string? HelpTextEN { get; private set; }

    /// <summary>
    /// Type of question input
    /// </summary>
    public QuestionType QuestionType { get; private set; }

    /// <summary>
    /// JSON array of options for choice-type questions (French)
    /// </summary>
    public string? OptionsFR { get; private set; }

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
    /// AI coach prompt in French - used to generate suggestions for users
    /// </summary>
    public string? CoachPromptFR { get; private set; }

    /// <summary>
    /// AI coach prompt in English
    /// </summary>
    public string? CoachPromptEN { get; private set; }

    /// <summary>
    /// Expert advice/tip in French to help users answer
    /// </summary>
    public string? ExpertAdviceFR { get; private set; }

    /// <summary>
    /// Expert advice/tip in English
    /// </summary>
    public string? ExpertAdviceEN { get; private set; }

    /// <summary>
    /// Display order within the step
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Whether this question is required
    /// </summary>
    public bool IsRequired { get; private set; }

    /// <summary>
    /// Whether this question is active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Icon for visual display (emoji or icon name)
    /// </summary>
    public string? Icon { get; private set; }

    /// <summary>
    /// Section grouping label within the step
    /// </summary>
    public string? SectionGroup { get; private set; }

    /// <summary>
    /// Maps this question to an Organization profile field key for adaptive interview filtering.
    /// When set, answers can be pre-filled from the org profile and vice versa.
    /// </summary>
    public string? ProfileFieldKey { get; private set; }

    // Navigation properties
    public virtual ICollection<QuestionSectionMapping> SectionMappings { get; private set; } = new List<QuestionSectionMapping>();

    private QuestionTemplate() { } // EF Core constructor

    public QuestionTemplate(
        int questionNumber,
        int stepNumber,
        string questionTextFR,
        string questionTextEN,
        QuestionType questionType,
        int displayOrder,
        bool isRequired = true,
        PersonaType? personaType = null,
        string? sectionGroup = null)
    {
        if (questionNumber < 1 || questionNumber > 50) // Allow some buffer above 22
            throw new ArgumentException("Question number must be between 1 and 50", nameof(questionNumber));
        if (stepNumber < 1 || stepNumber > 10) // Allow flexibility
            throw new ArgumentException("Step number must be between 1 and 10", nameof(stepNumber));

        QuestionNumber = questionNumber;
        StepNumber = stepNumber;
        QuestionTextFR = questionTextFR ?? throw new ArgumentNullException(nameof(questionTextFR));
        QuestionTextEN = questionTextEN ?? throw new ArgumentNullException(nameof(questionTextEN));
        QuestionType = questionType;
        DisplayOrder = displayOrder;
        IsRequired = isRequired;
        PersonaType = personaType;
        SectionGroup = sectionGroup;
        IsActive = true;
    }

    public void UpdateQuestionText(string questionTextFR, string questionTextEN)
    {
        QuestionTextFR = questionTextFR ?? throw new ArgumentNullException(nameof(questionTextFR));
        QuestionTextEN = questionTextEN ?? throw new ArgumentNullException(nameof(questionTextEN));
    }

    public void SetHelpText(string? helpTextFR, string? helpTextEN)
    {
        HelpTextFR = helpTextFR;
        HelpTextEN = helpTextEN;
    }

    public void SetOptions(string? optionsFR, string? optionsEN)
    {
        OptionsFR = optionsFR;
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

    /// <summary>
    /// Sets the AI coach prompts used to generate suggestions for users
    /// </summary>
    public void SetCoachPrompts(string? coachPromptFR, string? coachPromptEN)
    {
        CoachPromptFR = coachPromptFR;
        CoachPromptEN = coachPromptEN;
    }

    /// <summary>
    /// Sets expert advice/tips shown to users before they answer
    /// </summary>
    public void SetExpertAdvice(string? expertAdviceFR, string? expertAdviceEN)
    {
        ExpertAdviceFR = expertAdviceFR;
        ExpertAdviceEN = expertAdviceEN;
    }

    public void SetIcon(string? icon)
    {
        Icon = icon;
    }

    public void SetSectionGroup(string? sectionGroup)
    {
        SectionGroup = sectionGroup;
    }

    public void SetProfileFieldKey(string? profileFieldKey)
    {
        ProfileFieldKey = profileFieldKey;
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new ArgumentException("Display order must be non-negative", nameof(displayOrder));
        DisplayOrder = displayOrder;
    }

    public void UpdateStepNumber(int stepNumber)
    {
        if (stepNumber < 1 || stepNumber > 10)
            throw new ArgumentException("Step number must be between 1 and 10", nameof(stepNumber));
        StepNumber = stepNumber;
    }

    public void UpdateQuestionType(QuestionType questionType)
    {
        QuestionType = questionType;
    }

    public void SetPersonaType(PersonaType? personaType)
    {
        PersonaType = personaType;
    }

    public void SetRequired(bool isRequired)
    {
        IsRequired = isRequired;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void SetDisplayOrder(int order) => UpdateDisplayOrder(order);

    /// <summary>
    /// Factory method to create a new QuestionTemplate
    /// </summary>
    public static QuestionTemplate Create(
        int questionNumber,
        PersonaType? personaType,
        int stepNumber,
        string questionTextFR,
        string questionTextEN,
        string? helpTextFR,
        string? helpTextEN,
        QuestionType questionType,
        string? optionsFR,
        string? optionsEN,
        string? validationRules,
        string? conditionalLogic,
        string? coachPromptFR,
        string? coachPromptEN,
        string? expertAdviceFR,
        string? expertAdviceEN,
        int displayOrder,
        bool isRequired,
        string? icon)
    {
        var question = new QuestionTemplate(
            questionNumber,
            stepNumber,
            questionTextFR,
            questionTextEN,
            questionType,
            displayOrder,
            isRequired,
            personaType);

        question.SetHelpText(helpTextFR, helpTextEN);
        question.SetOptions(optionsFR, optionsEN);
        question.SetValidationRules(validationRules);
        question.SetConditionalLogic(conditionalLogic);
        question.SetCoachPrompts(coachPromptFR, coachPromptEN);
        question.SetExpertAdvice(expertAdviceFR, expertAdviceEN);
        question.SetIcon(icon);

        return question;
    }

    /// <summary>
    /// Update all editable properties
    /// </summary>
    public void Update(
        string questionTextFR,
        string questionTextEN,
        string? helpTextFR,
        string? helpTextEN,
        QuestionType questionType,
        string? optionsFR,
        string? optionsEN,
        string? validationRules,
        string? conditionalLogic,
        string? expertAdviceFR,
        string? expertAdviceEN,
        int displayOrder,
        bool isRequired,
        string? icon)
    {
        UpdateQuestionText(questionTextFR, questionTextEN);
        SetHelpText(helpTextFR, helpTextEN);
        UpdateQuestionType(questionType);
        SetOptions(optionsFR, optionsEN);
        SetValidationRules(validationRules);
        SetConditionalLogic(conditionalLogic);
        SetExpertAdvice(expertAdviceFR, expertAdviceEN);
        UpdateDisplayOrder(displayOrder);
        SetRequired(isRequired);
        SetIcon(icon);
    }

    /// <summary>
    /// Update just the coach prompts
    /// </summary>
    public void UpdateCoachPrompt(string? coachPromptFR, string? coachPromptEN)
    {
        SetCoachPrompts(coachPromptFR, coachPromptEN);
    }
}
