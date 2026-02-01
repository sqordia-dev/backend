using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Onboarding;

/// <summary>
/// Request to save onboarding step progress
/// </summary>
public class OnboardingProgressRequest
{
    /// <summary>
    /// The current onboarding step number (0-based)
    /// </summary>
    [Required]
    [Range(0, 10)]
    public int Step { get; set; }

    /// <summary>
    /// JSON data for the current step (form responses, selections, etc.)
    /// </summary>
    public string? StepData { get; set; }

    /// <summary>
    /// User persona selected during onboarding (Entrepreneur, Consultant, OBNL)
    /// </summary>
    [StringLength(50)]
    public string? Persona { get; set; }
}
