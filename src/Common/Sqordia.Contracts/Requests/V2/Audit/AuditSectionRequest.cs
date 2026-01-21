namespace Sqordia.Contracts.Requests.V2.Audit;

/// <summary>
/// Request for Socratic Coach audit on a section
/// </summary>
public class AuditSectionRequest
{
    /// <summary>
    /// The section name to audit (e.g., "executive-summary", "market-analysis")
    /// </summary>
    public string? SectionName { get; set; }

    /// <summary>
    /// Language for the audit response (fr or en)
    /// </summary>
    public string Language { get; set; } = "fr";

    /// <summary>
    /// Specific audit categories to focus on
    /// </summary>
    public List<string>? Categories { get; set; }
}
