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

    // Business context fields (optional)
    [MaxLength(100)]
    public string? Industry { get; set; }

    [MaxLength(100)]
    public string? Sector { get; set; }

    [MaxLength(100)]
    public string? LegalForm { get; set; }

    [MaxLength(50)]
    public string? TeamSize { get; set; }

    [MaxLength(50)]
    public string? FundingStatus { get; set; }

    [MaxLength(100)]
    public string? TargetMarket { get; set; }

    [MaxLength(50)]
    public string? BusinessStage { get; set; }

    public string? GoalsJson { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Province { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }
}

