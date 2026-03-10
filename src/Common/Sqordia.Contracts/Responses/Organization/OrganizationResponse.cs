namespace Sqordia.Contracts.Responses.Organization;

public class OrganizationResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string OrganizationType { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime? DeactivatedAt { get; set; }
    public int MaxMembers { get; set; }
    public bool AllowMemberInvites { get; set; }
    public bool RequireEmailVerification { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    // Business context
    public string? Industry { get; set; }
    public string? Sector { get; set; }
    public string? TeamSize { get; set; }
    public string? FundingStatus { get; set; }
    public string? TargetMarket { get; set; }
    public string? BusinessStage { get; set; }
    public string? GoalsJson { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Country { get; set; }
    public int ProfileCompletenessScore { get; set; }
}

