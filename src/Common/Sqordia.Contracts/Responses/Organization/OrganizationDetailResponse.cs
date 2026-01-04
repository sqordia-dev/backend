namespace Sqordia.Contracts.Responses.Organization;

public class OrganizationDetailResponse
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
    public DateTime Created { get; set; }
    public string? CreatedBy { get; set; }
    
    public required IEnumerable<OrganizationMemberResponse> Members { get; set; }
}

