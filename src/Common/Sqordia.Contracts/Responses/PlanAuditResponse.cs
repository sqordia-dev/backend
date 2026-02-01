namespace Sqordia.Contracts.Responses;

public class PlanAuditResponse
{
    public decimal ReadinessScore { get; set; }
    public ReadinessComponents ReadinessComponents { get; set; } = null!;
    public int? PivotPointMonth { get; set; }
    public int? RunwayMonths { get; set; }
    public ConfidenceInterval? ConfidenceInterval { get; set; }
    public List<AuditIssue> Issues { get; set; } = new();
}

public class ReadinessComponents
{
    public decimal ConsistencyScore { get; set; }
    public decimal RiskMitigationScore { get; set; }
    public decimal CompletenessScore { get; set; }
}

public class ConfidenceInterval
{
    public decimal Ambition { get; set; }
    public decimal Evidence { get; set; }
}

public class AuditIssue
{
    public string Category { get; set; } = null!; // Financial, Strategic, Legal
    public string Severity { get; set; } = null!; // error, warning, info
    public string Message { get; set; } = null!;
    public string? Nudge { get; set; }
    public Suggestions? Suggestions { get; set; }
}

public class Suggestions
{
    public string OptionA { get; set; } = null!;
    public string OptionB { get; set; } = null!;
    public string OptionC { get; set; } = null!;
}
