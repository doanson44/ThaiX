namespace ThaiX.Client.Services.Caching;

public static class ClientCacheExtensions
{
    public static string CacheKey(string group, string suffix) => $"{group}:{suffix}";

    public static Task<T> GetOrCreateGroupedAsync<T>(
        this IClientCacheService cache,
        string group,
        string keySuffix,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default)
    {
        return cache.GetOrCreateAsync(
            CacheKey(group, keySuffix),
            factory,
            absoluteExpiration,
            group,
            cancellationToken);
    }
}
