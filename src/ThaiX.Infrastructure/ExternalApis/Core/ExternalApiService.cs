using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Caching;

namespace ThaiX.Infrastructure.ExternalApis.Core;

/// <summary>
/// Central service for executing external API requests with optional caching.
/// Uses ExternalApiCacheKeyBuilder for deterministic cache keys.
/// </summary>
public sealed class ExternalApiService
{
    private readonly IApiTransport _transport;
    private readonly ICacheService _cache;
    private readonly ExternalApiCacheKeyBuilder _cacheKeyBuilder;
    private readonly ILogger<ExternalApiService> _logger;

    public ExternalApiService(
        IApiTransport transport,
        ICacheService cache,
        ExternalApiCacheKeyBuilder cacheKeyBuilder,
        ILogger<ExternalApiService> logger)
    {
        _transport = transport;
        _cache = cache;
        _cacheKeyBuilder = cacheKeyBuilder;
        _logger = logger;
    }

    public async Task<T?> ExecuteAsync<T>(ApiRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.UseCache)
        {
            return await _transport.SendAsync<T>(request, cancellationToken).ConfigureAwait(false);
        }

        var cacheKey = _cacheKeyBuilder.Build(request);

        try
        {
            var cached = await _cache.GetAsync<T>(cacheKey, cancellationToken).ConfigureAwait(false);
            if (cached != null)
            {
                _logger.LogDebug("External API cache hit. Key={CacheKey}", cacheKey);
                return cached;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "External API cache get failed. Key={CacheKey}", cacheKey);
        }

        var response = await _transport.SendAsync<T>(request, cancellationToken).ConfigureAwait(false);

        try
        {
            if (response is not null)
            {
                await _cache.SetAsync(cacheKey, response, request.CacheDuration, request.CacheGroup, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "External API cache set failed. Key={CacheKey}", cacheKey);
        }

        return response;
    }
}
