namespace Sqordia.Contracts.Responses.BusinessPlan;

public class BusinessPlanVersionResponse
{
    public Guid Id { get; set; }
    public Guid BusinessPlanId { get; set; }
    public int VersionNumber { get; set; }
    public string? Comment { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PlanType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    
    // Content preview (first 200 chars of executive summary)
    public string? ContentPreview { get; set; }
}

