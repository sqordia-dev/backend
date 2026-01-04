using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Organization;

public class UpdateOrganizationRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(200)]
    public required string Name { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(500)]
    [Url]
    public string? Website { get; set; }
}

