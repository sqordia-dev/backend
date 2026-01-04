namespace Sqordia.Application.Contracts.Responses;

/// <summary>
/// Subscription plan DTO
/// </summary>
public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string PlanType { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public string Currency { get; set; } = "CAD";
    public int MaxUsers { get; set; }
    public int MaxBusinessPlans { get; set; }
    public int MaxStorageGB { get; set; }
    public List<string> Features { get; set; } = new();
    public bool IsActive { get; set; }
    
    // Frontend-expected properties
    public int MaxOrganizations { get; set; }
    public int MaxTeamMembers { get; set; }
    public bool HasAdvancedAI { get; set; }
    public bool HasExportPDF { get; set; }
    public bool HasExportWord { get; set; }
    public bool HasExportExcel { get; set; }
    public bool HasPrioritySupport { get; set; }
    public bool HasCustomBranding { get; set; }
    public bool HasAPIAccess { get; set; }
    public int? DisplayOrder { get; set; }
}

