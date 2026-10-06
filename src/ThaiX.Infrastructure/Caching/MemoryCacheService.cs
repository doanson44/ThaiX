using Microsoft.Extensions.Caching.Memory;
using ThaiX.Application.Common.Caching;

namespace ThaiX.Infrastructure.Caching;

/// <summary>
/// Implements ICacheService using IMemoryCache. Group invalidation uses ChangeToken from MemoryCacheGroupManager.
/// List version keys are incremented on group invalidation when configured in CacheKeys.
/// </summary>
public sealed class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheGroupManager _groupManager;

    public MemoryCacheService(IMemoryCache cache, MemoryCacheGroupManager groupManager)
    {
        _cache = cache;
        _groupManager = groupManager;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_cache.TryGetValue(key, out var value) && value is T typed)
        {
            return Task.FromResult(typed);
        }
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, string? group = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        }

        if (!string.IsNullOrEmpty(group))
        {
            var token = _groupManager.GetGroupToken(group);
            options.AddExpirationToken(token);
        }

        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public async Task InvalidateGroupAsync(string group, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _groupManager.InvalidateGroup(group);

        var versionKey = CacheKeys.GetListVersionKeyForGroup(group);
        if (versionKey != null)
        {
            var current = _cache.TryGetValue(versionKey, out var boxed) && boxed is int v ? v : 0;
            _cache.Set(versionKey, current + 1, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365) });
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
