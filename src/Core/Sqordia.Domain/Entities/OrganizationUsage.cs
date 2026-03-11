using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

/// <summary>
/// Tracks monthly resource consumption per organization.
/// Resets at the start of each billing month.
/// </summary>
public class OrganizationUsage : BaseAuditableEntity
{
    public Guid OrganizationId { get; private set; }

    /// <summary>
    /// Billing period in YYYYMM format (e.g. 202603 for March 2026)
    /// </summary>
    public int Period { get; private set; }

    public int PlansGenerated { get; private set; }
    public int AiCoachMessages { get; private set; }
    public int ExportsGenerated { get; private set; }
    public long AiTokensUsed { get; private set; }
    public long StorageUsedBytes { get; private set; }

    // Navigation
    public Organization Organization { get; private set; } = null!;

    private OrganizationUsage() { } // EF Core

    public OrganizationUsage(Guid organizationId, int period)
    {
        OrganizationId = organizationId;
        Period = period;
    }

    public static OrganizationUsage CreateForCurrentMonth(Guid organizationId)
    {
        var period = int.Parse(DateTime.UtcNow.ToString("yyyyMM"));
        return new OrganizationUsage(organizationId, period);
    }

    // ── Increment methods ────────────────────────────────

    public void IncrementPlansGenerated(int count = 1)
    {
        PlansGenerated += count;
    }

    public void IncrementAiCoachMessages(int count = 1)
    {
        AiCoachMessages += count;
    }

    public void IncrementExports(int count = 1)
    {
        ExportsGenerated += count;
    }

    public void IncrementAiTokens(long tokens)
    {
        AiTokensUsed += tokens;
    }

    public void SetStorageUsed(long bytes)
    {
        StorageUsedBytes = bytes;
    }

    public void AddStorageUsed(long bytes)
    {
        StorageUsedBytes += bytes;
    }

    // ── Limit checks ─────────────────────────────────────

    public bool ExceedsLimit(string metricName, int limit)
    {
        if (limit < 0) return false; // unlimited

        return metricName switch
        {
            "max_ai_generations_monthly" => PlansGenerated >= limit,
            "max_ai_coach_messages_monthly" => AiCoachMessages >= limit,
            _ => false
        };
    }

    public double GetUsagePercent(string metricName, int limit)
    {
        if (limit <= 0) return 0;

        var current = metricName switch
        {
            "max_ai_generations_monthly" => PlansGenerated,
            "max_ai_coach_messages_monthly" => AiCoachMessages,
            _ => 0
        };

        return (double)current / limit * 100;
    }
}
