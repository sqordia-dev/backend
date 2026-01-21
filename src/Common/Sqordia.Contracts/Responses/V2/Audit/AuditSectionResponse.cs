namespace Sqordia.Contracts.Responses.V2.Audit;

/// <summary>
/// Socratic Coach audit response with Nudge + Triad
/// </summary>
public class AuditSectionResponse
{
    public Guid BusinessPlanId { get; set; }
    public required string SectionName { get; set; }
    public required string CategoryBadge { get; set; }
    public required NudgeResponse Nudge { get; set; }
    public required List<SmartSuggestion> Triad { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string? Model { get; set; }
}

/// <summary>
/// The Socratic "Nudge" - a probing question
/// </summary>
public class NudgeResponse
{
    /// <summary>
    /// The Socratic question to prompt user reflection
    /// </summary>
    public required string Question { get; set; }

    /// <summary>
    /// Context explaining why this question matters
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Severity level: Info, Warning, Critical
    /// </summary>
    public required string Severity { get; set; }
}

/// <summary>
/// A smart suggestion option (part of the Triad)
/// </summary>
public class SmartSuggestion
{
    /// <summary>
    /// Option label (e.g., "Option A", "Option B", "Option C")
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// The actual advice or suggestion text
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// Action identifier for the frontend (e.g., "update_budget", "add_risk_note")
    /// </summary>
    public required string Action { get; set; }

    /// <summary>
    /// Optional payload for the action
    /// </summary>
    public string? ActionPayload { get; set; }
}

/// <summary>
/// Summary of all audits for a business plan
/// </summary>
public class AuditSummaryResponse
{
    public Guid BusinessPlanId { get; set; }
    public decimal OverallScore { get; set; }
    public required List<SectionAuditSummary> Sections { get; set; }
    public required List<string> CriticalIssues { get; set; }
    public required List<string> Recommendations { get; set; }
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Audit summary for a single section
/// </summary>
public class SectionAuditSummary
{
    public required string SectionName { get; set; }
    public decimal Score { get; set; }
    public required string Status { get; set; }
    public int IssueCount { get; set; }
}
