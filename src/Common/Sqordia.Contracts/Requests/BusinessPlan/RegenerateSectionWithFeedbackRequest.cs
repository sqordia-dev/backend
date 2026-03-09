namespace Sqordia.Contracts.Requests.BusinessPlan;

/// <summary>
/// Request to regenerate a business plan section with user feedback.
/// </summary>
public class RegenerateSectionWithFeedbackRequest
{
    /// <summary>User's feedback on what to change</summary>
    public string? Feedback { get; set; }

    /// <summary>Elements from the current version to preserve</summary>
    public List<string>? KeepElements { get; set; }

    /// <summary>Desired tone adjustment (e.g., "more formal", "more concise", "data-driven")</summary>
    public string? Tone { get; set; }

    /// <summary>Language for generation (fr or en)</summary>
    public string Language { get; set; } = "fr";
}
