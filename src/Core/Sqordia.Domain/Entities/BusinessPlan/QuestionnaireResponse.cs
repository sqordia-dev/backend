using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// User's answer to a specific question in a business plan questionnaire
/// </summary>
public class QuestionnaireResponse : BaseAuditableEntity
{
    public Guid BusinessPlanId { get; private set; }
    public Guid QuestionTemplateId { get; private set; }
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
    public QuestionTemplate QuestionTemplate { get; private set; } = null!;
    
    private QuestionnaireResponse() { } // EF Core constructor
    
    public QuestionnaireResponse(
        Guid businessPlanId,
        Guid questionTemplateId,
        string responseText)
    {
        BusinessPlanId = businessPlanId;
        QuestionTemplateId = questionTemplateId;
        ResponseText = responseText ?? string.Empty;
    }
    
    public void UpdateResponse(string responseText)
    {
        ResponseText = responseText ?? string.Empty;
    }
    
    public void SetNumericValue(decimal? value) => NumericValue = value;
    public void SetDateValue(DateTime? value) => DateValue = value;
    public void SetBooleanValue(bool? value) => BooleanValue = value;
    public void SetSelectedOptions(string? options) => SelectedOptions = options;
    public void SetAiInsights(string? insights) => AiInsights = insights;
}

