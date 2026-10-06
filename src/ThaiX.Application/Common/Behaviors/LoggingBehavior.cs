using MediatR;
using Microsoft.Extensions.Logging;

namespace ThaiX.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs request execution details.
/// Logs before and after handler execution with timing information.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Handling {RequestName}", requestName);

        var startTime = DateTime.UtcNow;

        try
        {
            var response = await next();

            var elapsedMilliseconds = (DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds}ms",
                requestName,
                elapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            var elapsedMilliseconds = (DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogError(
                ex,
                "Error handling {RequestName} after {ElapsedMilliseconds}ms",
                requestName,
                elapsedMilliseconds);

            throw;
        }
    }
}
