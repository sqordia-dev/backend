using System.ComponentModel.DataAnnotations;
using Sqordia.Domain.Enums;

namespace Sqordia.Contracts.Requests.Admin.PromptRegistry;

/// <summary>
/// Filter parameters for querying prompt templates
/// </summary>
public class PromptRegistryFilter
{
    /// <summary>
    /// Filter by section type
    /// </summary>
    public SectionType? SectionType { get; set; }

    /// <summary>
    /// Filter by business plan type
    /// </summary>
    public BusinessPlanType? PlanType { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Filter by deployment alias
    /// </summary>
    public PromptAlias? Alias { get; set; }

    /// <summary>
    /// Filter by industry category
    /// </summary>
    [StringLength(50)]
    public string? IndustryCategory { get; set; }

    /// <summary>
    /// Search term for name or description
    /// </summary>
    [StringLength(200)]
    public string? Search { get; set; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort by field (name, sectionType, planType, version, createdAt, updatedAt)
    /// </summary>
    [StringLength(50)]
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (asc or desc)
    /// </summary>
    [StringLength(4)]
    public string? SortDirection { get; set; } = "desc";
}
