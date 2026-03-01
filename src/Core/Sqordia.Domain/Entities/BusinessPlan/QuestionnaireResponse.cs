using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// User's answer to a specific question in a business plan questionnaire
/// </summary>
public class QuestionnaireResponse : BaseAuditableEntity
{
    public Guid BusinessPlanId { get; private set; }

    /// <summary>
    /// FK to QuestionTemplate (V1) - nullable to support V2 questions
    /// </summary>
    public Guid? QuestionTemplateId { get; private set; }

    /// <summary>
    /// FK to QuestionTemplateV2 - nullable to support V1 questions
    /// </summary>
    public Guid? QuestionTemplateV2Id { get; private set; }

    /// <summary>
    /// FK to QuestionTemplateV3 (STRUCTURE FINALE) - nullable to support V1/V2 questions
    /// </summary>
    public Guid? QuestionTemplateV3Id { get; private set; }

    public string ResponseText { get; private set; } = null!;

    // For numeric responses
    public decimal? NumericValue { get; private set; }

    // For date responses
    public DateTime? DateValue { get; private set; }

    // For boolean responses
    public bool? BooleanValue { get; private set; }

    // For multiple choice (JSON array)
    public string? SelectedOptions { get; private set; } // ["Option1", "Option2"]

    // AI analysis and insights on this response
    public string? AiInsights { get; private set; }

    // Navigation properties
    public BusinessPlan BusinessPlan { get; private set; } = null!;
    public QuestionTemplate? QuestionTemplate { get; private set; }
    public QuestionTemplateV2? QuestionTemplateV2 { get; private set; }
    public QuestionTemplateV3? QuestionTemplateV3 { get; private set; }
    
    private QuestionnaireResponse() { } // EF Core constructor

    /// <summary>
    /// Create response for V1 question template
    /// </summary>
    public QuestionnaireResponse(
        Guid businessPlanId,
        Guid questionTemplateId,
        string responseText)
    {
        BusinessPlanId = businessPlanId;
        QuestionTemplateId = questionTemplateId;
        ResponseText = responseText ?? string.Empty;
    }

    /// <summary>
    /// Create response for V2 question template
    /// </summary>
    public static QuestionnaireResponse CreateForV2(
        Guid businessPlanId,
        Guid questionTemplateV2Id,
        string responseText)
    {
        return new QuestionnaireResponse
        {
            BusinessPlanId = businessPlanId,
            QuestionTemplateV2Id = questionTemplateV2Id,
            ResponseText = responseText ?? string.Empty
        };
    }

    /// <summary>
    /// Create response for V3 question template (STRUCTURE FINALE)
    /// </summary>
    public static QuestionnaireResponse CreateForV3(
        Guid businessPlanId,
        Guid questionTemplateV3Id,
        string responseText)
    {
        return new QuestionnaireResponse
        {
            BusinessPlanId = businessPlanId,
            QuestionTemplateV3Id = questionTemplateV3Id,
            ResponseText = responseText ?? string.Empty
        };
    }

    public void SetQuestionTemplateV2Id(Guid? id) => QuestionTemplateV2Id = id;
    public void SetQuestionTemplateV3Id(Guid? id) => QuestionTemplateV3Id = id;
    
    public void UpdateResponse(string responseText)
    {
        ResponseText = responseText ?? string.Empty;
    }
    
    public void SetNumericValue(decimal? value) => NumericValue = value;
    public void SetDateValue(DateTime? value) => DateValue = value;
    public void SetBooleanValue(bool? value) => BooleanValue = value;
    public void SetSelectedOptions(string? options) => SelectedOptions = options;
    public void SetAiInsights(string? insights) => AiInsights = insights;

    /// <summary>
    /// Returns true if at least one response field is populated
    /// </summary>
    public bool HasResponse()
    {
        return !string.IsNullOrWhiteSpace(ResponseText)
            || NumericValue.HasValue
            || DateValue.HasValue
            || BooleanValue.HasValue
            || !string.IsNullOrWhiteSpace(SelectedOptions);
    }

    /// <summary>
    /// Validates that the populated response field matches the expected question type.
    /// Returns true if valid, false if mismatched.
    /// </summary>
    public bool ValidateResponseType(string questionType)
    {
        return questionType?.ToLowerInvariant() switch
        {
            "text" or "textarea" or "open" => !string.IsNullOrWhiteSpace(ResponseText),
            "number" or "numeric" or "currency" or "percentage" => NumericValue.HasValue,
            "date" => DateValue.HasValue,
            "boolean" or "yes_no" or "yesno" => BooleanValue.HasValue,
            "multiple_choice" or "multiplechoice" or "select" or "multiselect" or "checkbox" => !string.IsNullOrWhiteSpace(SelectedOptions),
            _ => true // Unknown types pass validation (forward compatibility)
        };
    }
}

