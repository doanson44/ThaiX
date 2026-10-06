using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Common.Extensions;

/// <summary>
/// Extension methods for IQueryable to support pagination in Application query handlers.
/// Apply filtering, sorting, and projection BEFORE calling ToPagedListAsync.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Converts a projected IQueryable to a paged result.
    /// Pass pageSize = -1 (PagedRequest.FetchAll) to retrieve all records without pagination.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedListAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await source.CountAsync(cancellationToken);

        if (pageSize == PagedRequest.FetchAll)
        {
            if (totalCount == 0)
                return PagedResult<T>.Empty(1, 0);

            var allItems = await source.ToListAsync(cancellationToken);
            return PagedResult<T>.Create(allItems, totalCount, 1, totalCount);
        }

        var validPage = Math.Max(1, pageNumber);
        var validSize = Math.Clamp(pageSize, PagedRequest.MinPageSize, 10_000);

        if (totalCount == 0)
            return PagedResult<T>.Empty(validPage, validSize);

        var items = await source
            .Skip((validPage - 1) * validSize)
            .Take(validSize)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.Create(items, totalCount, validPage, validSize);
    }

    /// <summary>
    /// Converts a projected IQueryable to a paged result using a PagedRequest.
    /// </summary>
    public static Task<PagedResult<T>> ToPagedListAsync<T>(
        this IQueryable<T> source,
        PagedRequest request,
        CancellationToken cancellationToken = default)
        => source.ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);
}
