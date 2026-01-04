namespace Sqordia.Contracts.Responses.Admin;

/// <summary>
/// AI Prompt template information
/// </summary>
public class AIPromptDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Category { get; set; }
    public required string PlanType { get; set; }
    public required string Language { get; set; }
    public required string SystemPrompt { get; set; }
    public required string UserPromptTemplate { get; set; }
    public required string Variables { get; set; }
    public required bool IsActive { get; set; }
    public required int Version { get; set; }
    public string? ParentPromptId { get; set; }
    public string? Notes { get; set; }
    public required int UsageCount { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public required double AverageRating { get; set; }
    public required int RatingCount { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public required string CreatedBy { get; set; }
    public required string UpdatedBy { get; set; }
}

/// <summary>
/// AI Prompt test result
/// </summary>
public class AIPromptTestResult
{
    public required string PromptId { get; set; }
    public required string TestInput { get; set; }
    public required string GeneratedOutput { get; set; }
    public required int TokensUsed { get; set; }
    public required double Temperature { get; set; }
    public required DateTime TestedAt { get; set; }
    public required string Model { get; set; }
    public required TimeSpan ResponseTime { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// AI Prompt statistics
/// </summary>
public class AIPromptStats
{
    public required string PromptId { get; set; }
    public required string Name { get; set; }
    public required int TotalUsage { get; set; }
    public required double AverageRating { get; set; }
    public required int RatingCount { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public required int ActiveVersions { get; set; }
    public required int TotalVersions { get; set; }
    public required string MostUsedLanguage { get; set; }
    public required string MostUsedPlanType { get; set; }
}
