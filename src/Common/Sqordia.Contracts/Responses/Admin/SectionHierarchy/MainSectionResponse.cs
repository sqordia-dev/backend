namespace Sqordia.Contracts.Responses.Admin.SectionHierarchy;

/// <summary>
/// Response DTO for a main section with its sub-sections
/// </summary>
public record MainSectionResponse
{
    public Guid Id { get; init; }
    public int Number { get; init; }
    public string Code { get; init; } = null!;
    public string TitleFR { get; init; } = null!;
    public string TitleEN { get; init; } = null!;
    public string? DescriptionFR { get; init; }
    public string? DescriptionEN { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public bool GeneratedLast { get; init; }
    public string? Icon { get; init; }
    public DateTime Created { get; init; }
    public DateTime? LastModified { get; init; }
    public List<SubSectionResponse> SubSections { get; init; } = new();
}

/// <summary>
/// Response DTO for a sub-section
/// </summary>
public record SubSectionResponse
{
    public Guid Id { get; init; }
    public Guid MainSectionId { get; init; }
    public string Code { get; init; } = null!;
    public string TitleFR { get; init; } = null!;
    public string TitleEN { get; init; } = null!;
    public string? DescriptionFR { get; init; }
    public string? DescriptionEN { get; init; }
    public string? NoteFR { get; init; }
    public string? NoteEN { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public string? Icon { get; init; }
    public DateTime Created { get; init; }
    public DateTime? LastModified { get; init; }
    public int QuestionMappingsCount { get; init; }
    public int PromptsCount { get; init; }
}

/// <summary>
/// Lightweight list response for main sections
/// </summary>
public record MainSectionListResponse
{
    public Guid Id { get; init; }
    public int Number { get; init; }
    public string Code { get; init; } = null!;
    public string TitleFR { get; init; } = null!;
    public string TitleEN { get; init; } = null!;
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public bool GeneratedLast { get; init; }
    public string? Icon { get; init; }
    public int SubSectionsCount { get; init; }
}
