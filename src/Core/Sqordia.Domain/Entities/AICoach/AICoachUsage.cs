using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.AICoach;

/// <summary>
/// Tracks monthly token usage for AI Coach per user or organization.
/// Used for enforcing token limits based on subscription tier.
/// </summary>
public class AICoachUsage : BaseAuditableEntity
{
    public Guid? UserId { get; private set; }
    public Guid? OrganizationId { get; private set; }

    /// <summary>
    /// Month in YYYYMM format (e.g., 202603 for March 2026)
    /// </summary>
    public int Month { get; private set; }

    public int TotalTokensUsed { get; private set; }
    public DateTime LastUpdated { get; private set; }

    private AICoachUsage() { } // EF Core constructor

    public AICoachUsage(Guid? userId, Guid? organizationId, int month)
    {
        if (userId == null && organizationId == null)
            throw new ArgumentException("Either userId or organizationId must be provided");

        UserId = userId;
        OrganizationId = organizationId;
        Month = month;
        TotalTokensUsed = 0;
        LastUpdated = DateTime.UtcNow;
        Created = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a usage record for the current month
    /// </summary>
    public static AICoachUsage CreateForCurrentMonth(Guid? userId, Guid? organizationId)
    {
        var currentMonth = int.Parse(DateTime.UtcNow.ToString("yyyyMM"));
        return new AICoachUsage(userId, organizationId, currentMonth);
    }

    /// <summary>
    /// Increments the token usage
    /// </summary>
    public void IncrementUsage(int tokens)
    {
        if (tokens < 0)
            throw new ArgumentException("Tokens must be a positive value", nameof(tokens));

        TotalTokensUsed += tokens;
        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if usage exceeds the given limit
    /// </summary>
    public bool ExceedsLimit(int limit)
    {
        return TotalTokensUsed >= limit;
    }

    /// <summary>
    /// Gets the usage percentage relative to the limit
    /// </summary>
    public double GetUsagePercent(int limit)
    {
        if (limit <= 0) return 100;
        return (double)TotalTokensUsed / limit * 100;
    }

    /// <summary>
    /// Checks if usage is near the limit (80% or more by default)
    /// </summary>
    public bool IsNearLimit(int limit, double warningThreshold = 80)
    {
        return GetUsagePercent(limit) >= warningThreshold;
    }
}
