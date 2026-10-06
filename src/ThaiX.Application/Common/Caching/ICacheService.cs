namespace ThaiX.Application.Common.Caching;

/// <summary>
/// Application-level cache abstraction. Implement with IMemoryCache today; replace with Redis later without changing Application logic.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by key. Returns default if not found or expired.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in cache with optional expiration and group for invalidation.
    /// </summary>
    /// <param name="group">Optional group name; when invalidated, this entry is removed.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, string? group = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a single entry by key.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all entries in the given group and increments list version key when configured for the group.
    /// </summary>
    Task InvalidateGroupAsync(string group, CancellationToken cancellationToken = default);
}
