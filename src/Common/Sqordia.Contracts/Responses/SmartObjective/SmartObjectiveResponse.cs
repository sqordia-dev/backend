namespace Sqordia.Contracts.Responses.SmartObjective;

/// <summary>
/// SMART objective response
/// </summary>
public class SmartObjectiveResponse
{
    public required Guid Id { get; set; }
    public required Guid BusinessPlanId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Specific { get; set; }
    public required string Measurable { get; set; }
    public required string Achievable { get; set; }
    public required string Relevant { get; set; }
    public required string TimeBound { get; set; }
    public required DateTime TargetDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public required decimal ProgressPercentage { get; set; }
    public required string Status { get; set; }
    public required string Category { get; set; }
    public required int Priority { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Response for generating SMART objectives
/// </summary>
public class GenerateSmartObjectivesResponse
{
    public required Guid BusinessPlanId { get; set; }
    public required List<SmartObjectiveResponse> Objectives { get; set; }
    public required DateTime GeneratedAt { get; set; }
    public required string Language { get; set; }
}

