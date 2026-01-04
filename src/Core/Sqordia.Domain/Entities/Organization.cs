using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities;

public class Organization : BaseAuditableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Website { get; private set; }
    public string? LogoUrl { get; private set; }
    public OrganizationType OrganizationType { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }
    
    // Settings
    public int MaxMembers { get; private set; }
    public bool AllowMemberInvites { get; private set; }
    public bool RequireEmailVerification { get; private set; }
    
    // Navigation properties
    public ICollection<OrganizationMember> Members { get; private set; } = new List<OrganizationMember>();
    
    private Organization() { } // EF Core constructor
    
    public Organization(string name, OrganizationType organizationType, string? description = null, string? website = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        OrganizationType = organizationType;
        Description = description;
        Website = website;
        IsActive = true;
        MaxMembers = 10; // Default limit
        AllowMemberInvites = true;
        RequireEmailVerification = true;
        Created = DateTime.UtcNow;
    }
    
    public void UpdateDetails(string name, string? description, string? website)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        Website = website;
    }
    
    public void UpdateLogo(string logoUrl)
    {
        LogoUrl = logoUrl;
    }

    public void UpdateOrganizationType(OrganizationType organizationType)
    {
        OrganizationType = organizationType;
    }
    
    public void UpdateSettings(int maxMembers, bool allowMemberInvites, bool requireEmailVerification)
    {
        if (maxMembers < 1)
            throw new ArgumentException("Max members must be at least 1", nameof(maxMembers));
            
        MaxMembers = maxMembers;
        AllowMemberInvites = allowMemberInvites;
        RequireEmailVerification = requireEmailVerification;
    }
    
    public void Deactivate()
    {
        IsActive = false;
        DeactivatedAt = DateTime.UtcNow;
    }
    
    public void Reactivate()
    {
        IsActive = true;
        DeactivatedAt = null;
    }
    
    public bool CanAddMoreMembers()
    {
        return Members.Count(m => m.IsActive) < MaxMembers;
    }
}

