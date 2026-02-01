namespace Sqordia.Contracts.Responses.Onboarding;

/// <summary>
/// DTO for onboarding progress information
/// </summary>
public class OnboardingProgressDto
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Whether onboarding has been completed
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Current step in the onboarding process (null if not started)
    /// </summary>
    public int? CurrentStep { get; set; }

    /// <summary>
    /// Total number of onboarding steps
    /// </summary>
    public int TotalSteps { get; set; }

    /// <summary>
    /// Completion percentage (0-100)
    /// </summary>
    public double CompletionPercentage { get; set; }

    /// <summary>
    /// Selected persona (Entrepreneur, Consultant, OBNL)
    /// </summary>
    public string? Persona { get; set; }

    /// <summary>
    /// Stored onboarding data (JSON)
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// When the onboarding was last updated
    /// </summary>
    public DateTime? LastUpdated { get; set; }
}
