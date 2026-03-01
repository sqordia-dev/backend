namespace Sqordia.Contracts.Requests.Admin.SectionPrompt;

/// <summary>
/// Request to create a new section prompt
/// </summary>
public record CreateSectionPromptRequest
{
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
}

/// <summary>
/// Request to update a section prompt
/// </summary>
public record UpdateSectionPromptRequest
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string SystemPrompt { get; init; } = null!;
    public string UserPromptTemplate { get; init; } = null!;
    public string? VariablesJson { get; init; }
    public string OutputFormat { get; init; } = null!;
    public string? VisualElementsJson { get; init; }
    public string? ExampleOutput { get; init; }
    public string? IndustryCategory { get; init; }
}

/// <summary>
/// Request to test a section prompt
/// </summary>
public record TestSectionPromptRequest
{
    public Dictionary<string, string> Variables { get; init; } = new();
    public double Temperature { get; init; } = 0.7;
    public int MaxTokens { get; init; } = 2000;
}

/// <summary>
/// Filter for listing section prompts
/// </summary>
public record SectionPromptFilterRequest
{
    public string? Level { get; init; }
    public string? PlanType { get; init; }
    public string? Language { get; init; }
    public Guid? MainSectionId { get; init; }
    public Guid? SubSectionId { get; init; }
    public bool? IsActive { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
