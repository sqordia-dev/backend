using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Organization;

public class UpdateOrganizationSettingsRequest
{
    [Required]
    [Range(1, 1000)]
    public int MaxMembers { get; set; }
    
    public bool AllowMemberInvites { get; set; }
    
    public bool RequireEmailVerification { get; set; }
}

