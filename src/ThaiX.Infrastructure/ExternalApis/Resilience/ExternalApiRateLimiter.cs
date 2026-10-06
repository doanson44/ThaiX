using System.Collections.Concurrent;
using ThaiX.Infrastructure.ExternalApis.Core;

namespace ThaiX.Infrastructure.ExternalApis.Resilience;

/// <summary>
/// In-memory rate limiter: limits concurrent requests per host to avoid hitting external API rate limits.
/// </summary>
public sealed class ExternalApiRateLimiter
{
    private readonly int _maxConcurrentPerHost;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _perHostSemaphores = new();

    public ExternalApiRateLimiter(int maxConcurrentPerHost)
    {
        _maxConcurrentPerHost = Math.Max(1, maxConcurrentPerHost);
    }

    /// <summary>
    /// Executes the delegate with a per-host concurrency limit.
    /// </summary>
    public async Task<T?> ExecuteAsync<T>(
        ApiRequest request,
        Func<ApiRequest, CancellationToken, Task<T?>> execute,
        CancellationToken cancellationToken = default)
    {
        var host = GetHost(request.Url);
        var semaphore = _perHostSemaphores.GetOrAdd(host, _ => new SemaphoreSlim(_maxConcurrentPerHost, _maxConcurrentPerHost));

        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await execute(request, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static string GetHost(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "unknown";
        }

        try
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri && !string.IsNullOrEmpty(uri.Host))
            {
                return uri.Host;
            }
        }
        catch
        {
            // ignore
        }

        return "unknown";
    }
}
