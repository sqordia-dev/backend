namespace Sqordia.Contracts.Responses.BusinessPlan;

/// <summary>
/// Represents a business plan section with its content and metadata
/// </summary>
public class BusinessPlanSectionDto
{
    /// <summary>
    /// The business plan ID
    /// </summary>
    public required Guid BusinessPlanId { get; set; }
    
    /// <summary>
    /// The section name (e.g., "executive-summary", "market-analysis")
    /// </summary>
    public required string SectionName { get; set; }
    
    /// <summary>
    /// The display title of the section
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// The section content
    /// </summary>
    public string? Content { get; set; }
    
    /// <summary>
    /// Whether the section has content
    /// </summary>
    public required bool HasContent { get; set; }
    
    /// <summary>
    /// Word count of the content
    /// </summary>
    public required int WordCount { get; set; }
    
    /// <summary>
    /// Character count of the content
    /// </summary>
    public required int CharacterCount { get; set; }
    
    /// <summary>
    /// When the section was last updated
    /// </summary>
    public DateTime? LastUpdated { get; set; }
    
    /// <summary>
    /// Who last updated the section
    /// </summary>
    public string? LastUpdatedBy { get; set; }
    
    /// <summary>
    /// Whether the section is required for this plan type
    /// </summary>
    public required bool IsRequired { get; set; }
    
    /// <summary>
    /// The order/position of the section in the plan
    /// </summary>
    public required int Order { get; set; }
    
    /// <summary>
    /// Section description/help text
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Whether the section is AI-generated
    /// </summary>
    public required bool IsAIGenerated { get; set; }
    
    /// <summary>
    /// Whether the section has been manually edited
    /// </summary>
    public required bool IsManuallyEdited { get; set; }
    
    /// <summary>
    /// Section status (draft, review, final)
    /// </summary>
    public required string Status { get; set; }
    
    /// <summary>
    /// Tags associated with the section
    /// </summary>
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Represents a list of business plan sections
/// </summary>
public class BusinessPlanSectionsDto
{
    /// <summary>
    /// The business plan ID
    /// </summary>
    public required Guid BusinessPlanId { get; set; }
    
    /// <summary>
    /// The business plan title
    /// </summary>
    public required string BusinessPlanTitle { get; set; }
    
    /// <summary>
    /// The plan type
    /// </summary>
    public required string PlanType { get; set; }
    
    /// <summary>
    /// List of sections
    /// </summary>
    public required List<BusinessPlanSectionDto> Sections { get; set; }
    
    /// <summary>
    /// Total number of sections
    /// </summary>
    public required int TotalSections { get; set; }
    
    /// <summary>
    /// Number of sections with content
    /// </summary>
    public required int SectionsWithContent { get; set; }
    
    /// <summary>
    /// Overall completion percentage
    /// </summary>
    public required decimal CompletionPercentage { get; set; }
    
    /// <summary>
    /// When the sections were last updated
    /// </summary>
    public DateTime? LastUpdated { get; set; }
}
