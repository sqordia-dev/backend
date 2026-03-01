namespace Sqordia.Contracts.Responses.Admin.QuestionMapping;

/// <summary>
/// Response DTO for a question-section mapping
/// </summary>
public record QuestionMappingResponse
{
    public Guid Id { get; init; }
    public Guid QuestionTemplateV3Id { get; init; }
    public Guid SubSectionId { get; init; }
    public string? MappingContext { get; init; }
    public decimal? Weight { get; init; }
    public string? TransformationHint { get; init; }
    public bool IsActive { get; init; }

    // Navigation info
    public int QuestionNumber { get; init; }
    public string QuestionTextFR { get; init; } = null!;
    public string QuestionTextEN { get; init; } = null!;
    public string SubSectionCode { get; init; } = null!;
    public string SubSectionTitleFR { get; init; } = null!;
    public string SubSectionTitleEN { get; init; } = null!;
    public string MainSectionCode { get; init; } = null!;
    public string MainSectionTitleFR { get; init; } = null!;
}

/// <summary>
/// Matrix representation for admin UI
/// Questions (rows) x SubSections (columns)
/// </summary>
public record MappingMatrixResponse
{
    public List<MatrixQuestionRow> Questions { get; init; } = new();
    public List<MatrixSubSectionColumn> SubSections { get; init; } = new();
}

/// <summary>
/// Row in the mapping matrix (question)
/// </summary>
public record MatrixQuestionRow
{
    public Guid QuestionId { get; init; }
    public int QuestionNumber { get; init; }
    public string QuestionTextFR { get; init; } = null!;
    public string QuestionTextEN { get; init; } = null!;
    public int StepNumber { get; init; }
    public List<MatrixCell> Mappings { get; init; } = new();
}

/// <summary>
/// Column in the mapping matrix (sub-section)
/// </summary>
public record MatrixSubSectionColumn
{
    public Guid SubSectionId { get; init; }
    public string Code { get; init; } = null!;
    public string TitleFR { get; init; } = null!;
    public string TitleEN { get; init; } = null!;
    public Guid MainSectionId { get; init; }
    public string MainSectionCode { get; init; } = null!;
    public int MainSectionNumber { get; init; }
}

/// <summary>
/// Cell in the mapping matrix (intersection)
/// </summary>
public record MatrixCell
{
    public Guid? MappingId { get; init; }
    public Guid SubSectionId { get; init; }
    public bool IsMapped { get; init; }
    public string? Context { get; init; }
    public decimal? Weight { get; init; }
}

/// <summary>
/// Summary statistics for mappings
/// </summary>
public record MappingStatsResponse
{
    public int TotalQuestions { get; init; }
    public int TotalSubSections { get; init; }
    public int TotalMappings { get; init; }
    public int UnmappedQuestions { get; init; }
    public int UnmappedSubSections { get; init; }
    public double AverageMappingsPerQuestion { get; init; }
}
