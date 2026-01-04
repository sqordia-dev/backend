namespace Sqordia.Application.Contracts.Requests;

/// <summary>
/// Request to subscribe to a plan
/// </summary>
public class SubscribeRequest
{
    public Guid PlanId { get; set; }
    public Guid OrganizationId { get; set; }
    public bool IsYearly { get; set; }
}

