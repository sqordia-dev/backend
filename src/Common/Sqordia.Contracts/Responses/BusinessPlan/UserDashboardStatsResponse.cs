namespace Sqordia.Contracts.Responses.BusinessPlan;

/// <summary>
/// Dashboard statistics for a user
/// </summary>
public class UserDashboardStatsResponse
{
    /// <summary>
    /// Total number of business plans owned by the user
    /// </summary>
    public int TotalPlans { get; set; }

    /// <summary>
    /// Number of plans created this week
    /// </summary>
    public int PlansCreatedThisWeek { get; set; }

    /// <summary>
    /// Number of plans created last week (for comparison)
    /// </summary>
    public int PlansCreatedLastWeek { get; set; }

    /// <summary>
    /// Percentage change compared to last week (-100 to +Infinity)
    /// </summary>
    public decimal GrowthPercentage { get; set; }

    /// <summary>
    /// Whether the trend is positive
    /// </summary>
    public bool IsPositiveTrend { get; set; }

    /// <summary>
    /// Number of draft/in-progress plans
    /// </summary>
    public int InProgressPlans { get; set; }

    /// <summary>
    /// Number of completed plans
    /// </summary>
    public int CompletedPlans { get; set; }

    /// <summary>
    /// Number of generated plans
    /// </summary>
    public int GeneratedPlans { get; set; }

    /// <summary>
    /// Daily plan counts for the last 7 days (for sparkline)
    /// </summary>
    public List<DailyPlanCount> DailyActivity { get; set; } = new();

    /// <summary>
    /// Most recent activity date
    /// </summary>
    public DateTime? LastActivityDate { get; set; }
}

/// <summary>
/// Daily plan count for sparkline chart
/// </summary>
public class DailyPlanCount
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}
