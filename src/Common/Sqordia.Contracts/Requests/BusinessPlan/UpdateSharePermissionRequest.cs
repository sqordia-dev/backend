using System.ComponentModel.DataAnnotations;
using Sqordia.Contracts.Enums;

namespace Sqordia.Contracts.Requests.BusinessPlan;

public class UpdateSharePermissionRequest
{
    [Required]
    public required SharePermission Permission { get; set; }
}

