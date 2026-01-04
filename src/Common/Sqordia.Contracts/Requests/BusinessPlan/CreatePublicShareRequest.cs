using Sqordia.Contracts.Enums;

namespace Sqordia.Contracts.Requests.BusinessPlan;

public class CreatePublicShareRequest
{
    public SharePermission Permission { get; set; } = SharePermission.ReadOnly;
    
    public DateTime? ExpiresAt { get; set; }
}

