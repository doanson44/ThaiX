using MediatR;
using Microsoft.Extensions.Logging;
using System.Globalization;
using ThaiX.Application.Common.Caching;

namespace ThaiX.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that caches responses for requests implementing ICacheableQuery.
/// Cache key may contain "{version}" for versioned list caches; replaced at runtime from group list version.
/// On cache failure the request still succeeds (no throw).
/// </summary>
public sealed class QueryCachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cache;
    private readonly ILogger<QueryCachingBehavior<TRequest, TResponse>> _logger;

    public QueryCachingBehavior(ICacheService cache, ILogger<QueryCachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheable)
        {
            return await next();
        }

        var cacheKey = cacheable.CacheKey;

        // Resolve version placeholder for versioned list caches
        if (cacheKey.Contains("{version}", StringComparison.Ordinal))
        {
            var versionKey = CacheKeys.GetListVersionKeyForGroup(cacheable.CacheGroup);
            if (versionKey != null)
            {
                var version = await _cache.GetAsync<int>(versionKey, cancellationToken).ConfigureAwait(false);
                var versionStr = version is { } v ? v.ToString(CultureInfo.InvariantCulture) : "0";
                cacheKey = cacheKey.Replace("{version}", versionStr, StringComparison.Ordinal);
            }
        }

        try
        {
            var cached = await _cache.GetAsync<TResponse>(cacheKey, cancellationToken).ConfigureAwait(false);
            if (cached != null)
            {
                return cached;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache get failed for key {CacheKey}, executing handler", cacheKey);
        }

        var response = await next().ConfigureAwait(false);

        try
        {
            if (response is not null)
            {
                var expiration = cacheable.Expiration;
                var group = cacheable.IsVersionedList ? null : cacheable.CacheGroup;
                await _cache.SetAsync(cacheKey, response, expiration, group, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache set failed for key {CacheKey}", cacheKey);
        }

        return response;
    }
}
