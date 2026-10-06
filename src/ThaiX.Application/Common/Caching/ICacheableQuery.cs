namespace ThaiX.Application.Common.Caching;

/// <summary>
/// Marker and configuration for queries that support response caching via QueryCachingBehavior.
/// CacheKey may contain placeholder "{version}" for versioned list caches; behavior replaces it with current list version.
/// </summary>
public interface ICacheableQuery
{
    /// <summary>
    /// Cache key for this query. Use CacheKeys helpers. Use "{version}" for versioned list keys.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Optional expiration. If null, a default per-query-type should be used by the behavior.
    /// </summary>
    TimeSpan? Expiration { get; }

    /// <summary>
    /// Cache group for invalidation (e.g. CacheGroups.Contacts). When a command invalidates this group, this entry is cleared.
    /// </summary>
    string CacheGroup { get; }

    /// <summary>
    /// When true, this is a versioned list cache: key contains "{version}", entry is not registered in group set; invalidation only increments version.
    /// </summary>
    bool IsVersionedList => false;
}
