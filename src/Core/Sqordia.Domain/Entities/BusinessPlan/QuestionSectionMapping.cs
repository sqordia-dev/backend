using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// Maps questions to sub-sections for content generation
/// Enables many-to-many relationship where one question feeds multiple sections
/// </summary>
public class QuestionSectionMapping : BaseEntity
{
    /// <summary>
    /// Reference to the V3 question template
    /// </summary>
    public Guid QuestionTemplateV3Id { get; private set; }

    /// <summary>
    /// Reference to the target sub-section
    /// </summary>
    public Guid SubSectionId { get; private set; }

    /// <summary>
    /// Context type for this mapping: "primary", "secondary", "supporting"
    /// Primary = main source of content for this section
    /// Secondary = supplementary information
    /// Supporting = background context only
    /// </summary>
    public string MappingContext { get; private set; } = "primary";

    /// <summary>
    /// Importance weight (0.0 - 1.0) for prioritizing multiple questions
    /// </summary>
    public decimal Weight { get; private set; } = 1.0m;

    /// <summary>
    /// Hint for how to transform/use the answer in this section
    /// e.g., "Use as market size data", "Extract competitive advantages"
    /// </summary>
    public string? TransformationHint { get; private set; }

    /// <summary>
    /// Whether this mapping is active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Display order when multiple questions map to same section
    /// </summary>
    public int DisplayOrder { get; private set; }

    // Navigation properties
    public virtual QuestionTemplateV3 QuestionTemplate { get; private set; } = null!;
    public virtual SubSection SubSection { get; private set; } = null!;

    private QuestionSectionMapping() { } // EF Core constructor

    public QuestionSectionMapping(
        Guid questionTemplateV3Id,
        Guid subSectionId,
        string mappingContext = "primary",
        decimal weight = 1.0m,
        string? transformationHint = null,
        int displayOrder = 0)
    {
        if (weight < 0 || weight > 1)
            throw new ArgumentException("Weight must be between 0 and 1", nameof(weight));

        QuestionTemplateV3Id = questionTemplateV3Id;
        SubSectionId = subSectionId;
        MappingContext = mappingContext ?? "primary";
        Weight = weight;
        TransformationHint = transformationHint;
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    public void UpdateMappingContext(string mappingContext)
    {
        MappingContext = mappingContext ?? "primary";
    }

    public void UpdateWeight(decimal weight)
    {
        if (weight < 0 || weight > 1)
            throw new ArgumentException("Weight must be between 0 and 1", nameof(weight));
        Weight = weight;
    }

    public void SetTransformationHint(string? transformationHint)
    {
        TransformationHint = transformationHint;
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Factory method to create a new mapping
    /// </summary>
    public static QuestionSectionMapping Create(
        Guid questionTemplateV3Id,
        Guid subSectionId,
        string? mappingContext,
        decimal? weight,
        string? transformationHint)
    {
        return new QuestionSectionMapping(
            questionTemplateV3Id,
            subSectionId,
            mappingContext ?? "primary",
            weight ?? 1.0m,
            transformationHint);
    }

    /// <summary>
    /// Update all editable properties
    /// </summary>
    public void Update(string? mappingContext, decimal? weight, string? transformationHint)
    {
        if (mappingContext != null)
        {
            UpdateMappingContext(mappingContext);
        }
        if (weight.HasValue)
        {
            UpdateWeight(weight.Value);
        }
        SetTransformationHint(transformationHint);
    }
}
