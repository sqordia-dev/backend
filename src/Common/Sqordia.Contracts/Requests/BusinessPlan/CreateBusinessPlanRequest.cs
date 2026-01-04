using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.BusinessPlan;

public class CreateBusinessPlanRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public required string Title { get; set; }
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [RegularExpression("^(BusinessPlan|StrategicPlan|LeanCanvas)$", ErrorMessage = "PlanType must be one of: BusinessPlan, StrategicPlan, LeanCanvas")]
    public required string PlanType { get; set; } // "BusinessPlan", "StrategicPlan", "LeanCanvas"
    
    [Required]
    public required Guid OrganizationId { get; set; }
}

