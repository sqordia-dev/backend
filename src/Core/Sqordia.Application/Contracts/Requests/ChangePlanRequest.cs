namespace Sqordia.Application.Contracts.Requests;

/// <summary>
/// Request to change subscription plan
/// </summary>
public class ChangePlanRequest
{
    public Guid NewPlanId { get; set; }
    public bool IsYearly { get; set; }
}

