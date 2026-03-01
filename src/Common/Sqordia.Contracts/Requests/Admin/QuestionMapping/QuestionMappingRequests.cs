namespace Sqordia.Contracts.Requests.Admin.QuestionMapping;

/// <summary>
/// Request to create a new question-section mapping
/// </summary>
public record CreateQuestionMappingRequest
{
    public Guid QuestionTemplateV3Id { get; init; }
    public Guid SubSectionId { get; init; }
    public string? MappingContext { get; init; } // "primary", "secondary"
    public decimal? Weight { get; init; } // 0-1
    public string? TransformationHint { get; init; }
}

/// <summary>
/// Request to update a question-section mapping
/// </summary>
public record UpdateQuestionMappingRequest
{
    public string? MappingContext { get; init; }
    public decimal? Weight { get; init; }
    public string? TransformationHint { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>
/// Request for bulk updating mappings (from matrix UI)
/// </summary>
public record BulkUpdateMappingsRequest
{
    public List<MappingUpdate> Updates { get; init; } = new();
}

/// <summary>
/// Single mapping update in bulk operation
/// </summary>
public record MappingUpdate
{
    public Guid QuestionTemplateV3Id { get; init; }
    public Guid SubSectionId { get; init; }
    public MappingAction Action { get; init; }
    public string? MappingContext { get; init; }
    public decimal? Weight { get; init; }
}

/// <summary>
/// Action for bulk mapping update
/// </summary>
public enum MappingAction
{
    Create = 1,
    Update = 2,
    Delete = 3
}

/// <summary>
/// Filter for listing mappings
/// </summary>
public record QuestionMappingFilterRequest
{
    public Guid? QuestionTemplateV3Id { get; init; }
    public Guid? SubSectionId { get; init; }
    public Guid? MainSectionId { get; init; }
    public string? MappingContext { get; init; }
    public bool? IsActive { get; init; }
}

/// <summary>
/// Request for matrix view with optional filters
/// </summary>
public record MappingMatrixRequest
{
    public Guid? MainSectionId { get; init; } // Filter columns by main section
    public int? StepNumber { get; init; } // Filter rows by question step
    public bool IncludeInactive { get; init; } = false;
}
