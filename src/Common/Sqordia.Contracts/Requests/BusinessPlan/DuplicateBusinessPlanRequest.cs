using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.BusinessPlan;

public class DuplicateBusinessPlanRequest
{
    [StringLength(200, MinimumLength = 3)]
    public string? NewTitle { get; set; }
}

