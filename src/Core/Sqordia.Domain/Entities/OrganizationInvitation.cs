using Sqordia.Domain.Common;
using Sqordia.Domain.Entities.Identity;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities;

public class OrganizationInvitation : BaseAuditableEntity
{
    public Guid OrganizationId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public OrganizationRole Role { get; private set; }
    public Guid Token { get; private set; }
    public Guid InvitedByUserId { get; private set; }
    public InvitationStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public Guid? AcceptedByUserId { get; private set; }

    // Navigation properties
    public Organization Organization { get; private set; } = null!;
    public User InvitedByUser { get; private set; } = null!;
    public User? AcceptedByUser { get; private set; }

    private OrganizationInvitation() { } // EF Core constructor

    public OrganizationInvitation(
        Guid organizationId,
        string email,
        OrganizationRole role,
        Guid invitedByUserId,
        int expirationDays = 7)
    {
        OrganizationId = organizationId;
        Email = email.ToLowerInvariant();
        Role = role;
        Token = Guid.NewGuid();
        InvitedByUserId = invitedByUserId;
        Status = InvitationStatus.Pending;
        ExpiresAt = DateTime.UtcNow.AddDays(expirationDays);
        Created = DateTime.UtcNow;
    }

    public void Accept(Guid userId)
    {
        if (Status != InvitationStatus.Pending)
            throw new InvalidOperationException("Only pending invitations can be accepted.");

        if (DateTime.UtcNow > ExpiresAt)
            throw new InvalidOperationException("This invitation has expired.");

        Status = InvitationStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
        AcceptedByUserId = userId;
    }

    public void Cancel()
    {
        if (Status != InvitationStatus.Pending)
            throw new InvalidOperationException("Only pending invitations can be cancelled.");

        Status = InvitationStatus.Cancelled;
    }

    public void MarkExpired()
    {
        if (Status == InvitationStatus.Pending)
        {
            Status = InvitationStatus.Expired;
        }
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt && Status == InvitationStatus.Pending;
}
