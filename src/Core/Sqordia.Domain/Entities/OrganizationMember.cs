using Sqordia.Domain.Common;
using Sqordia.Domain.Entities.Identity;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities;

public class OrganizationMember : BaseAuditableEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid UserId { get; private set; }
    public OrganizationRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }
    public Guid? InvitedBy { get; private set; }
    
    // Navigation properties
    public Organization Organization { get; private set; } = null!;
    public User User { get; private set; } = null!;
    
    private OrganizationMember() { } // EF Core constructor
    
    public OrganizationMember(Guid organizationId, Guid userId, OrganizationRole role, Guid? invitedBy = null)
    {
        OrganizationId = organizationId;
        UserId = userId;
        Role = role;
        IsActive = true;
        JoinedAt = DateTime.UtcNow;
        InvitedBy = invitedBy;
        Created = DateTime.UtcNow;
    }
    
    public void UpdateRole(OrganizationRole newRole)
    {
        Role = newRole;
    }
    
    public void Deactivate()
    {
        IsActive = false;
        LeftAt = DateTime.UtcNow;
    }
    
    public void Reactivate()
    {
        IsActive = true;
        LeftAt = null;
    }
}

