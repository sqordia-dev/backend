using Microsoft.EntityFrameworkCore;
using Sqordia.Contracts.Common;

namespace Sqordia.Persistence.Extensions;

/// <summary>
/// Extension methods for IQueryable to support pagination and other common query operations.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies pagination to a queryable and returns a paginated response.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="query">The queryable to paginate</param>
    /// <param name="request">Pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated response containing the items and metadata</returns>
    public static async Task<PaginatedResponse<T>> ToPaginatedResponseAsync<T>(
        this IQueryable<T> query,
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PaginatedResponse<T>.Create(items, totalCount, request);
    }

    /// <summary>
    /// Applies pagination to a queryable and returns a paginated response with projection.
    /// </summary>
    /// <typeparam name="TSource">The source entity type</typeparam>
    /// <typeparam name="TResult">The projected result type</typeparam>
    /// <param name="query">The queryable to paginate</param>
    /// <param name="request">Pagination parameters</param>
    /// <param name="selector">Projection function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated response containing the projected items and metadata</returns>
    public static async Task<PaginatedResponse<TResult>> ToPaginatedResponseAsync<TSource, TResult>(
        this IQueryable<TSource> query,
        PaginationRequest request,
        Func<TSource, TResult> selector,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PaginatedResponse<TResult>.Create(items.Select(selector), totalCount, request);
    }

    /// <summary>
    /// Marks a query as read-only by disabling change tracking.
    /// Use this for queries that won't modify entities.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="query">The queryable</param>
    /// <returns>The queryable with AsNoTracking applied</returns>
    public static IQueryable<T> AsReadOnly<T>(this IQueryable<T> query) where T : class
        => query.AsNoTracking();
}
