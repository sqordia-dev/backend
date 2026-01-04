using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Organization;

public class CreateOrganizationRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(200)]
    public required string Name { get; set; }
    
    [Required]
    public required string OrganizationType { get; set; } // "Startup", "OBNL", or "ConsultingFirm"
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(500)]
    [Url]
    public string? Website { get; set; }
}

