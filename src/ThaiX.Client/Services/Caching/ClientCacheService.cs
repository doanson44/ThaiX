using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace ThaiX.Client.Services.Caching;

public sealed class ClientCacheService : IClientCacheService
{
    public static readonly TimeSpan DefaultAbsoluteExpiration = TimeSpan.FromMinutes(5);

    private readonly bool _enabled;
    private readonly ConcurrentDictionary<string, CacheEntry> _entries = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _groupKeys = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.Ordinal);

    public ClientCacheService(IOptions<ClientCacheOptions> options)
    {
        _enabled = options.Value.Enabled;
    }

    public Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpiration = null,
        string? group = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        if (!_enabled)
            return factory(cancellationToken);

        if (TryGetEntry(key, out var existing) && existing.TryGetValue(out T cached))
            return Task.FromResult(cached);

        return GetOrCreateCoreAsync(key, factory, absoluteExpiration, group, cancellationToken);
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpiration = null,
        string? group = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_enabled)
            return Task.CompletedTask;

        var entry = CacheEntry.FromValue(value, absoluteExpiration ?? DefaultAbsoluteExpiration);
        _entries[key] = entry;
        RegisterGroupKey(group, key);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_enabled)
            return Task.CompletedTask;

        _entries.TryRemove(key, out _);
        RemoveKeyFromAllGroups(key);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_enabled)
            return Task.CompletedTask;

        foreach (var key in _entries.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)))
            _entries.TryRemove(key, out _);

        foreach (var group in _groupKeys.Values)
        {
            foreach (var key in group.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList())
                group.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    public Task InvalidateGroupAsync(string group, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(group);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_enabled)
            return Task.CompletedTask;

        if (!_groupKeys.TryRemove(group, out var keys))
            return Task.CompletedTask;

        foreach (var key in keys.Keys)
            _entries.TryRemove(key, out _);

        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_enabled)
            return Task.CompletedTask;

        _entries.Clear();
        _groupKeys.Clear();
        return Task.CompletedTask;
    }

    private async Task<T> GetOrCreateCoreAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpiration,
        string? group,
        CancellationToken cancellationToken)
    {
        var gate = _locks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (TryGetEntry(key, out var existing) && existing.TryGetValue(out T cached))
                return cached;

            var value = await factory(cancellationToken).ConfigureAwait(false);
            if (value is null)
                throw new InvalidOperationException($"Cache factory for key '{key}' returned null.");

            _entries[key] = CacheEntry.FromValue(value, absoluteExpiration ?? DefaultAbsoluteExpiration);
            RegisterGroupKey(group, key);
            return value;
        }
        finally
        {
            gate.Release();
        }
    }

    private bool TryGetEntry(string key, out CacheEntry entry)
    {
        if (_entries.TryGetValue(key, out entry!) && !entry.IsExpired)
            return true;

        if (entry is not null)
            _entries.TryRemove(key, out _);

        entry = null!;
        return false;
    }

    private void RegisterGroupKey(string? group, string key)
    {
        if (string.IsNullOrWhiteSpace(group))
            return;

        var keys = _groupKeys.GetOrAdd(group, static _ => new ConcurrentDictionary<string, byte>(StringComparer.Ordinal));
        keys[key] = 0;
    }

    private void RemoveKeyFromAllGroups(string key)
    {
        foreach (var group in _groupKeys.Values)
            group.TryRemove(key, out _);
    }

    private sealed class CacheEntry
    {
        private readonly object _value;
        private readonly DateTimeOffset _expiresAt;

        private CacheEntry(object value, DateTimeOffset expiresAt)
        {
            _value = value;
            _expiresAt = expiresAt;
        }

        public bool IsExpired => DateTimeOffset.UtcNow >= _expiresAt;

        public static CacheEntry FromValue<T>(T value, TimeSpan absoluteExpiration) =>
            new(value!, DateTimeOffset.UtcNow.Add(absoluteExpiration));

        public bool TryGetValue<T>(out T? value)
        {
            if (_value is T typed)
            {
                value = typed;
                return true;
            }

            value = default;
            return false;
        }
    }
}
