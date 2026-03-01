namespace Sqordia.Contracts.Requests.Admin.SectionHierarchy;

/// <summary>
/// Request to create a new main section
/// </summary>
public record CreateMainSectionRequest
{
    public int Number { get; init; }
    public string Code { get; init; } = null!;
    public string TitleFR { get; init; } = null!;
    public string TitleEN { get; init; } = null!;
    public string? DescriptionFR { get; init; }
    public string? DescriptionEN { get; init; }
    public int DisplayOrder { get; init; }
    public bool GeneratedLast { get; init; }
    public string? Icon { get; init; }
}

/// <summary>
/// Request to update a main section
/// </summary>
public record UpdateMainSectionRequest
{
    public string TitleFR { get; init; } = null!;
    public string TitleEN { get; init; } = null!;
    public string? DescriptionFR { get; init; }
    public string? DescriptionEN { get; init; }
    public int DisplayOrder { get; init; }
    public bool GeneratedLast { get; init; }
    public string? Icon { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>
/// Request to create a new sub-section
/// </summary>
public record CreateSubSectionRequest
{
    public string Code { get; init; } = null!;
    public string TitleFR { get; init; } = null!;
    public string TitleEN { get; init; } = null!;
    public string? DescriptionFR { get; init; }
    public string? DescriptionEN { get; init; }
    public string? NoteFR { get; init; }
    public string? NoteEN { get; init; }
    public int DisplayOrder { get; init; }
    public string? Icon { get; init; }
}

/// <summary>
/// Request to update a sub-section
/// </summary>
public record UpdateSubSectionRequest
{
    public string TitleFR { get; init; } = null!;
    public string TitleEN { get; init; } = null!;
    public string? DescriptionFR { get; init; }
    public string? DescriptionEN { get; init; }
    public string? NoteFR { get; init; }
    public string? NoteEN { get; init; }
    public int DisplayOrder { get; init; }
    public string? Icon { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>
/// Request to reorder sections
/// </summary>
public record ReorderSectionsRequest
{
    public List<SectionOrderItem> Items { get; init; } = new();
}

public record SectionOrderItem
{
    public Guid Id { get; init; }
    public int DisplayOrder { get; init; }
}
