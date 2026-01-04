using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// Template for individual questions in a questionnaire
/// </summary>
public class QuestionTemplate : BaseEntity
{
    public Guid QuestionnaireTemplateId { get; private set; }
    public string QuestionText { get; private set; } = null!;
    public string? HelpText { get; private set; }
    public QuestionType QuestionType { get; private set; }
    public int Order { get; private set; }
    public bool IsRequired { get; private set; }
    public string? Section { get; private set; } // e.g., "Market", "Financials", "Team"
    
    // Bilingual support - English translations
    public string? QuestionTextEN { get; private set; }
    public string? HelpTextEN { get; private set; }
    public string? OptionsEN { get; private set; } // ["Option1EN", "Option2EN", "Option3EN"]
    
    // For choice-based questions (JSON array)
    public string? Options { get; private set; } // ["Option1", "Option2", "Option3"]
    
    // Validation rules (JSON)
    public string? ValidationRules { get; private set; } // { "minLength": 10, "maxLength": 500 }
    
    // Conditional logic (JSON)
    public string? ConditionalLogic { get; private set; } // { "showIf": "question5", "equals": "Yes" }
    
    // Navigation properties
    public QuestionnaireTemplate QuestionnaireTemplate { get; private set; } = null!;
    public ICollection<QuestionnaireResponse> Responses { get; private set; } = new List<QuestionnaireResponse>();
    
    private QuestionTemplate() { } // EF Core constructor
    
    public QuestionTemplate(
        Guid questionnaireTemplateId,
        string questionText,
        QuestionType questionType,
        int order,
        bool isRequired = true,
        string? section = null,
        string? helpText = null,
        string? options = null)
    {
        QuestionnaireTemplateId = questionnaireTemplateId;
        QuestionText = questionText ?? throw new ArgumentNullException(nameof(questionText));
        QuestionType = questionType;
        Order = order;
        IsRequired = isRequired;
        Section = section;
        HelpText = helpText;
        Options = options;
    }
    
    public void UpdateQuestionText(string questionText)
    {
        QuestionText = questionText ?? throw new ArgumentNullException(nameof(questionText));
    }
    
    public void UpdateHelpText(string? helpText) => HelpText = helpText;
    public void UpdateOptions(string? options) => Options = options;
    public void UpdateValidationRules(string? rules) => ValidationRules = rules;
    public void UpdateConditionalLogic(string? logic) => ConditionalLogic = logic;
    public void SetRequired(bool isRequired) => IsRequired = isRequired;
    
    // Bilingual support methods
    public void UpdateQuestionTextEN(string? questionTextEN) => QuestionTextEN = questionTextEN;
    public void UpdateHelpTextEN(string? helpTextEN) => HelpTextEN = helpTextEN;
    public void UpdateOptionsEN(string? optionsEN) => OptionsEN = optionsEN;
}

