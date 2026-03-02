using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Common;

/// <summary>
/// Base pagination request parameters for list endpoints.
/// Use this as a base class or query parameters for paginated endpoints.
/// </summary>
public record PaginationRequest
{
    /// <summary>
    /// Page number (1-based). Default is 1.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of items per page. Default is 20, max is 100.
    /// </summary>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Field to sort by. Supported fields depend on the endpoint.
    /// </summary>
    [StringLength(50)]
    public string? SortBy { get; init; }

    /// <summary>
    /// Sort in descending order. Default is false (ascending).
    /// </summary>
    public bool SortDescending { get; init; }

    /// <summary>
    /// Calculate the number of items to skip based on page number and size.
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;
}
