using Sqordia.Contracts.Enums;

namespace Sqordia.Contracts.Responses.BusinessPlan;

public class BusinessPlanShareResponse
{
    public Guid Id { get; set; }
    public Guid BusinessPlanId { get; set; }
    public Guid? SharedWithUserId { get; set; }
    public string? SharedWithEmail { get; set; }
    public string? SharedWithUserName { get; set; }
    public SharePermission Permission { get; set; }
    public string PermissionName { get; set; } = string.Empty; // "ReadOnly", "Edit", "FullAccess"
    public bool IsPublic { get; set; }
    public string? PublicToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public int AccessCount { get; set; }
    public DateTime Created { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

