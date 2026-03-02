using Sqordia.Contracts.Common;

namespace Sqordia.Contracts.Requests.BusinessPlan;

/// <summary>
/// Request parameters for listing business plans with pagination and filtering.
/// </summary>
public record BusinessPlanListRequest : PaginationRequest
{
    /// <summary>
    /// Filter by search term (searches title and description).
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Filter by plan type (BusinessPlan, StrategicPlan, LeanCanvas).
    /// </summary>
    public string? PlanType { get; init; }

    /// <summary>
    /// Filter by status (Draft, Active, Completed, Archived).
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Filter by organization ID.
    /// </summary>
    public Guid? OrganizationId { get; init; }

    /// <summary>
    /// Include archived plans in results. Default is false.
    /// </summary>
    public bool IncludeArchived { get; init; } = false;
}
