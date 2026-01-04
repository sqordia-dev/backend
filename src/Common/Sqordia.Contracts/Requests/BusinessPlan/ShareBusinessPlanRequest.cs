using System.ComponentModel.DataAnnotations;
using Sqordia.Contracts.Enums;

namespace Sqordia.Contracts.Requests.BusinessPlan;

public class ShareBusinessPlanRequest
{
    public Guid? SharedWithUserId { get; set; }
    
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }
    
    [Required]
    public required SharePermission Permission { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
}

