namespace Sqordia.Application.Contracts.Responses;

/// <summary>
/// Subscription DTO
/// </summary>
public class SubscriptionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionPlanDto Plan { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? CancelledEffectiveDate { get; set; }
    public bool IsYearly { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "CAD";
    public bool IsTrial { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public bool IsActive { get; set; }
}

