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

    /// <summary>
    /// Optional persona type (Entrepreneur, Consultant, or OBNL)
    /// If not provided, will be taken from user's profile
    /// </summary>
    [RegularExpression("^(Entrepreneur|Consultant|OBNL)?$", ErrorMessage = "Persona must be one of: Entrepreneur, Consultant, OBNL")]
    public string? Persona { get; set; }
}

