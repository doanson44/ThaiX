using MediatR;
using Microsoft.Extensions.Logging;
using System.Reflection;
using ThaiX.Application.Common.Caching;

namespace ThaiX.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that invalidates cache groups after command execution based on [InvalidateCache] attributes.
/// Runs after the handler; supports multiple attributes per command.
/// </summary>
public sealed class CommandCacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cache;
    private readonly ILogger<CommandCacheInvalidationBehavior<TRequest, TResponse>> _logger;

    public CommandCacheInvalidationBehavior(ICacheService cache, ILogger<CommandCacheInvalidationBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next().ConfigureAwait(false);

        var type = request.GetType();
        var attrs = type.GetCustomAttributes<InvalidateCacheAttribute>(inherit: false).ToList();
        if (attrs.Count == 0)
        {
            return response;
        }

        foreach (var attr in attrs)
        {
            try
            {
                await _cache.InvalidateGroupAsync(attr.Group, cancellationToken).ConfigureAwait(false);
                _logger.LogDebug("Invalidated cache group {Group} for {RequestName}", attr.Group, type.Name);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache invalidation failed for group {Group}", attr.Group);
            }
        }

        return response;
    }
}
