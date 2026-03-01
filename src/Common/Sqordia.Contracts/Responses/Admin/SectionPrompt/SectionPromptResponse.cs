namespace Sqordia.Contracts.Responses.Admin.SectionPrompt;

/// <summary>
/// Response DTO for a section prompt
/// </summary>
public record SectionPromptResponse
{
    public Guid Id { get; init; }
    public Guid? MainSectionId { get; init; }
    public Guid? SubSectionId { get; init; }
    public string Level { get; init; } = null!; // "Master" or "Override"
    public string PlanType { get; init; } = null!;
    public string Language { get; init; } = null!;
    public string? IndustryCategory { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string SystemPrompt { get; init; } = null!;
    public string UserPromptTemplate { get; init; } = null!;
    public string? VariablesJson { get; init; }
    public string OutputFormat { get; init; } = null!;
    public string? VisualElementsJson { get; init; }
    public string? ExampleOutput { get; init; }
    public int Version { get; init; }
    public bool IsActive { get; init; }
    public DateTime Created { get; init; }
    public DateTime? LastModified { get; init; }
    public string? CreatedBy { get; init; }

    // Navigation info
    public string? MainSectionCode { get; init; }
    public string? MainSectionTitle { get; init; }
    public string? SubSectionCode { get; init; }
    public string? SubSectionTitle { get; init; }
}

/// <summary>
/// Lightweight list response for section prompts
/// </summary>
public record SectionPromptListResponse
{
    public Guid Id { get; init; }
    public string Level { get; init; } = null!;
    public string PlanType { get; init; } = null!;
    public string Language { get; init; } = null!;
    public string Name { get; init; } = null!;
    public int Version { get; init; }
    public bool IsActive { get; init; }
    public string? MainSectionCode { get; init; }
    public string? SubSectionCode { get; init; }
    public DateTime? LastModified { get; init; }
}

/// <summary>
/// Response for prompt test execution
/// </summary>
public record SectionPromptTestResponse
{
    public bool Success { get; init; }
    public string? GeneratedContent { get; init; }
    public string? ErrorMessage { get; init; }
    public int TokensUsed { get; init; }
    public double ResponseTimeMs { get; init; }
    public string Provider { get; init; } = null!;
    public string Model { get; init; } = null!;
}
