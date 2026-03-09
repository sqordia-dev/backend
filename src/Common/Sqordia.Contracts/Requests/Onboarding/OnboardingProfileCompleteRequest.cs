using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Onboarding;

public class OnboardingProfileCompleteRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(200)]
    public required string CompanyName { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Persona { get; set; }

    [MaxLength(100)]
    public string? Industry { get; set; }

    [MaxLength(100)]
    public string? Sector { get; set; }

    [MaxLength(50)]
    public string? BusinessStage { get; set; }

    [MaxLength(50)]
    public string? TeamSize { get; set; }

    [MaxLength(50)]
    public string? FundingStatus { get; set; }

    [MaxLength(100)]
    public string? TargetMarket { get; set; }

    public string? GoalsJson { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Province { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    public bool CreateBusinessPlan { get; set; } = true;
}
