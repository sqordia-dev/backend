namespace Sqordia.Contracts.Responses.AICoach;

/// <summary>
/// Response containing AI Coach token usage information
/// </summary>
public class AICoachTokenUsageResponse
{
    /// <summary>
    /// Total tokens used this month
    /// </summary>
    public int TokensUsed { get; set; }

    /// <summary>
    /// Monthly token limit based on subscription tier
    /// </summary>
    public int TokenLimit { get; set; }

    /// <summary>
    /// Usage percentage (0-100)
    /// </summary>
    public double UsagePercent { get; set; }

    /// <summary>
    /// Whether the user is near the token limit (>= warning threshold)
    /// </summary>
    public bool IsNearLimit { get; set; }

    /// <summary>
    /// Warning message to display (if near limit)
    /// </summary>
    public string? WarningMessage { get; set; }

    /// <summary>
    /// Remaining tokens available
    /// </summary>
    public int TokensRemaining { get; set; }

    /// <summary>
    /// Current billing month (YYYYMM format)
    /// </summary>
    public int CurrentMonth { get; set; }
}
