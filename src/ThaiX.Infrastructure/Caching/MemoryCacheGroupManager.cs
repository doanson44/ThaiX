using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace ThaiX.Infrastructure.Caching;

/// <summary>
/// Manages per-group change tokens for cache invalidation. When a group is invalidated,
/// all entries that registered with that group's token are evicted by IMemoryCache.
/// Thread-safe.
/// </summary>
public sealed class MemoryCacheGroupManager
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _sources = new();

    /// <summary>
    /// Returns the current change token for the group. Entries that add this token will be evicted when InvalidateGroup is called.
    /// Creates a new token if the group has not been registered or the previous token was cancelled.
    /// </summary>
    public IChangeToken GetGroupToken(string group)
    {
        while (true)
        {
            if (_sources.TryGetValue(group, out var existing) && !existing.Token.IsCancellationRequested)
            {
                return new CancellationChangeToken(existing.Token);
            }

            var cts = new CancellationTokenSource();
            if (_sources.TryAdd(group, cts) || (_sources.TryGetValue(group, out var current) && current.Token.IsCancellationRequested && _sources.TryUpdate(group, cts, current)))
            {
                return new CancellationChangeToken(cts.Token);
            }
        }
    }

    /// <summary>
    /// Invalidates the group: cancels the current token so all entries using it are evicted, then removes it so the next GetGroupToken creates a fresh token.
    /// </summary>
    public void InvalidateGroup(string group)
    {
        if (_sources.TryRemove(group, out var cts))
        {
            try
            {
                cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Ignore; already cancelled/disposed
            }
        }
    }
}
