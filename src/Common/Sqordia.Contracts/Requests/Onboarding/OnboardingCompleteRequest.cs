using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Onboarding;

/// <summary>
/// Request to mark onboarding as complete, optionally creating an initial business plan
/// </summary>
public class OnboardingCompleteRequest
{
    /// <summary>
    /// Whether to create an initial business plan for the user
    /// </summary>
    public bool CreateInitialPlan { get; set; }

    /// <summary>
    /// Name for the initial business plan (required if CreateInitialPlan is true)
    /// </summary>
    [StringLength(200)]
    public string? PlanName { get; set; }

    /// <summary>
    /// Industry for the initial business plan
    /// </summary>
    [StringLength(100)]
    public string? Industry { get; set; }

    /// <summary>
    /// Template ID to use for the initial plan (optional)
    /// </summary>
    public Guid? TemplateId { get; set; }

    /// <summary>
    /// Final onboarding data (all collected data)
    /// </summary>
    public string? FinalData { get; set; }
}
