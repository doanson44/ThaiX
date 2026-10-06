using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using System.Net;

namespace ThaiX.Infrastructure.ExternalApis.Resilience;

/// <summary>
/// Applies per-request timeout and retry (5xx, network, timeout) to external API HTTP calls.
/// Timeout is read from HttpRequestMessage.Options; default 120s.
/// </summary>
public sealed class ExternalApiResilienceHandler : DelegatingHandler
{
    private const int DefaultTimeoutSeconds = 120;
    private const int MaxRetryAttempts = 3;

    internal static readonly HttpRequestOptionsKey<TimeSpan> RequestTimeoutKey = new("ExternalApiRequestTimeout");

    private readonly ILogger<ExternalApiResilienceHandler> _logger;

    public ExternalApiResilienceHandler(ILogger<ExternalApiResilienceHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var timeout = GetTimeout(request);
        var pipeline = BuildPipeline(timeout);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var method = request.Method.Method;
        var url = request.RequestUri?.ToString() ?? "unknown";

        try
        {
            var response = await pipeline.ExecuteAsync(async ct =>
            {
                var clone = await CloneRequestAsync(request, ct).ConfigureAwait(false);
                return await InnerSendAsync(clone, ct).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();
            _logger.LogInformation(
                "External API request completed. Method={Method}, Url={Url}, StatusCode={StatusCode}, DurationMs={DurationMs}",
                method, RedactSensitive(url), (int)response.StatusCode, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "External API request failed. Method={Method}, Url={Url}, DurationMs={DurationMs}",
                method, RedactSensitive(url), stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task<HttpResponseMessage> InnerSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content != null)
        {
            var ms = new MemoryStream();
            await request.Content.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);
            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Options.TryGetValue(RequestTimeoutKey, out var timeout))
        {
            clone.Options.Set(RequestTimeoutKey, timeout);
        }

        return clone;
    }

    private static TimeSpan GetTimeout(HttpRequestMessage request)
    {
        if (request.Options.TryGetValue(RequestTimeoutKey, out var timeout) && timeout > TimeSpan.Zero)
        {
            return timeout;
        }

        return TimeSpan.FromSeconds(DefaultTimeoutSeconds);
    }

    private ResiliencePipeline<HttpResponseMessage> BuildPipeline(TimeSpan timeout)
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = timeout
            })
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = MaxRetryAttempts,
                DelayGenerator = static args =>
                {
                    var delaySeconds = Math.Pow(2, args.AttemptNumber + 1);
                    return new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(delaySeconds));
                },
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
                    .HandleResult(response => IsTransientStatusCode(response.StatusCode)),
                OnRetry = args =>
                {
                    args.Outcome.Result?.Dispose();
                    _logger.LogWarning(
                        "External API retry. Attempt={Attempt}, Exception={ExceptionType}, StatusCode={StatusCode}",
                        args.AttemptNumber + 1,
                        args.Outcome.Exception?.GetType().Name ?? "None",
                        args.Outcome.Result?.StatusCode.ToString() ?? "N/A");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    private static bool IsTransientStatusCode(HttpStatusCode statusCode)
    {
        return statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or >= HttpStatusCode.InternalServerError;
    }

    private static string RedactSensitive(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return url;
        }

        try
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            {
                return url;
            }

            var builder = new UriBuilder(uri);
            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                var parts = uri.UserInfo.Split(':', 2, StringSplitOptions.None);
                builder.UserName = parts[0] ?? string.Empty;
                builder.Password = string.Empty;
            }
            return builder.Uri.ToString();
        }
        catch
        {
            return url;
        }
    }
}
