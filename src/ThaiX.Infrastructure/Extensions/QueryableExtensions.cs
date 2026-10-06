using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Models;

namespace ThaiX.Infrastructure.Extensions;

/// <summary>
/// Extension methods for IQueryable to support pagination.
/// Lives in Infrastructure layer due to EF Core dependency.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Converts an IQueryable to a paged result.
    /// Executes count and data queries, then returns PagedResult.
    /// </summary>
    /// <typeparam name="T">The type of items in the query.</typeparam>
    /// <param name="source">The source queryable (should already be filtered and sorted).</param>
    /// <param name="request">The paging request parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A PagedResult containing items and pagination metadata.</returns>
    /// <remarks>
    /// IMPORTANT: Apply filtering, sorting, and projection BEFORE calling this method.
    /// This method only handles Skip/Take/Count.
    /// 
    /// Example:
    /// <code>
    /// var result = await _context.Users
    ///     .AsNoTracking()
    ///     .Where(u => u.IsActive)
    ///     .OrderBy(u => u.LastName)
    ///     .Select(u => new UserDto { ... })
    ///     .ToPagedListAsync(request, cancellationToken);
    /// </code>
    /// </remarks>
    public static async Task<PagedResult<T>> ToPagedListAsync<T>(
        this IQueryable<T> source,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Use validated values to ensure bounds
        var pageNumber = request.ValidatedPageNumber;
        var pageSize = request.ValidatedPageSize;

        // Execute count query
        var totalCount = await source.CountAsync(cancellationToken);

        // If no items, return empty result
        if (totalCount == 0)
        {
            return PagedResult<T>.Empty(pageNumber, pageSize);
        }

        // Execute data query with pagination
        var items = await source
            .Skip(request.CalculateSkip())
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.Create(
            items,
            totalCount,
            pageNumber,
            pageSize);
    }

    /// <summary>
    /// Converts an IQueryable to a paged result with explicit page parameters.
    /// Use this overload when you don't have a PagedRequest object.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedListAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var request = new PagedRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return await source.ToPagedListAsync(request, cancellationToken);
    }
}
