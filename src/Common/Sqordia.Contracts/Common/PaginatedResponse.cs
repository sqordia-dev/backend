namespace Sqordia.Contracts.Common;

/// <summary>
/// Wrapper for paginated API responses.
/// Provides metadata about the current page, total counts, and navigation helpers.
/// </summary>
/// <typeparam name="T">The type of items in the response</typeparam>
public record PaginatedResponse<T>
{
    /// <summary>
    /// The items for the current page.
    /// </summary>
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>
    /// Whether there is a next page available.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Whether there is a previous page available.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Creates an empty paginated response.
    /// </summary>
    public static PaginatedResponse<T> Empty(int pageNumber = 1, int pageSize = 20) => new()
    {
        Items = Enumerable.Empty<T>(),
        TotalCount = 0,
        PageNumber = pageNumber,
        PageSize = pageSize
    };

    /// <summary>
    /// Creates a paginated response from a list of items and pagination parameters.
    /// </summary>
    public static PaginatedResponse<T> Create(
        IEnumerable<T> items,
        int totalCount,
        int pageNumber,
        int pageSize) => new()
    {
        Items = items,
        TotalCount = totalCount,
        PageNumber = pageNumber,
        PageSize = pageSize
    };

    /// <summary>
    /// Creates a paginated response from a PaginationRequest.
    /// </summary>
    public static PaginatedResponse<T> Create(
        IEnumerable<T> items,
        int totalCount,
        PaginationRequest request) => new()
    {
        Items = items,
        TotalCount = totalCount,
        PageNumber = request.PageNumber,
        PageSize = request.PageSize
    };
}
