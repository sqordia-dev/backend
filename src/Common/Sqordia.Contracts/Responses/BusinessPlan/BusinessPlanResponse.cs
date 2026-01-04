namespace Sqordia.Contracts.Responses.BusinessPlan;

public class BusinessPlanResponse
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string PlanType { get; set; }
    public required string Status { get; set; }
    public Guid OrganizationId { get; set; }
    public required string OrganizationName { get; set; }
    public int Version { get; set; }
    public int TotalQuestions { get; set; }
    public int CompletedQuestions { get; set; }
    public decimal CompletionPercentage { get; set; }
    public DateTime? QuestionnaireCompletedAt { get; set; }
    public DateTime? GenerationStartedAt { get; set; }
    public DateTime? GenerationCompletedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public required string CreatedBy { get; set; }
}

