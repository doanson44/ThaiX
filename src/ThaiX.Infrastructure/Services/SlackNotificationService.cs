using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Models;
using ThaiX.Infrastructure.Configuration;
using ThaiX.Infrastructure.Services.Notifications;

namespace ThaiX.Infrastructure.Services;

public sealed class SlackNotificationService : ISlackNotificationService
{
    private const int ChunkLength = 3800;
    private const int MaxRetries = 3;

    private static readonly Uri PostMessageEndpoint = new("https://slack.com/api/chat.postMessage");

    private readonly HttpClient _httpClient;
    private readonly SlackSettings _settings;
    private readonly ILogger<SlackNotificationService> _logger;

    public SlackNotificationService(
        HttpClient httpClient,
        IOptions<SlackSettings> options,
        ILogger<SlackNotificationService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendAsync(NotificationMessage message, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(message.Text))
        {
            throw new ArgumentException("Slack message text is required.", nameof(message));
        }

        var eventKey = message.Kind.ToString();
        var routePath = ResolveRoutingPath(eventKey);
        var channel = ResolveChannel(routePath);
        var fullText = BuildText(message);
        var chunks = SplitIntoChunks(fullText, ChunkLength);

        // One message at a time per channel to respect Slack ~1 msg/sec/channel limit.
        using var _ = await AsyncLock.GetLockByKey($"slack:channel:{channel}").LockAsync(ct);
        {
            for (var i = 0; i < chunks.Count; i++)
            {
                var text = chunks.Count > 1 ? $"{chunks[i]} ({i + 1}/{chunks.Count})" : chunks[i];
                await SendWithRetryAsync(channel, text, null, ct);

                // Pace between chunks to stay under per-channel rate limit.
                if (i < chunks.Count - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1.1), ct);
                }
            }
        }
    }

    public async Task SendToChannelAsync(string channelId, string text, string? threadTs, CancellationToken ct, string? attachmentsJson = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelId);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var payload = new Dictionary<string, object>
        {
            ["channel"] = channelId,
            ["text"] = text
        };
        if (!string.IsNullOrWhiteSpace(threadTs))
        {
            payload["thread_ts"] = threadTs;
        }
        if (!string.IsNullOrWhiteSpace(attachmentsJson))
        {
            payload["attachments"] = JsonSerializer.Deserialize<JsonElement>(attachmentsJson);
        }

        await SendWithRetryAsync(channelId, text, null, ct, payload);
    }

    public async Task SendToResponseUrlAsync(string responseUrl, string text, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(responseUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var payload = new { response_type = "ephemeral", text, replace_original = false };

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, responseUrl)
            {
                Content = JsonContent.Create(payload)
            };
            using var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Slack response_url POST returned {Status}", (int)response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Slack response_url POST failed");
        }
    }

    private async Task SendWithRetryAsync(string channel, string text, object? blocks, CancellationToken ct,
        Dictionary<string, object>? overridePayload = null)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            var (success, shouldRetry, retryAfter) = await TrySendAsync(channel, text, blocks, ct, overridePayload);
            if (success)
            {
                return;
            }

            if (!shouldRetry || attempt == MaxRetries)
            {
                throw new InvalidOperationException(
                    $"Slack send failed after {attempt} attempt(s) for channel '{channel}'.");
            }

            var delay = retryAfter ?? GetJitteredDelay(attempt);
            _logger.LogWarning(
                "Slack send attempt {Attempt}/{Max} failed for channel {Channel}, retrying in {DelayMs}ms",
                attempt, MaxRetries, channel, delay.TotalMilliseconds);
            await Task.Delay(delay, ct);
        }
    }

    private async Task<(bool Success, bool ShouldRetry, TimeSpan? RetryAfter)> TrySendAsync(
        string channel, string text, object? blocks, CancellationToken ct,
        Dictionary<string, object>? overridePayload = null)
    {
        object payload;
        if (overridePayload is not null)
        {
            payload = overridePayload;
        }
        else if (blocks is null)
        {
            payload = new { channel, text };
        }
        else
        {
            payload = new { channel, text, blocks };
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, PostMessageEndpoint)
        {
            Content = JsonContent.Create(payload)
        };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, ct);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("Slack request timed out for channel {Channel}", channel);
            return (false, true, null);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Slack network error for channel {Channel}", channel);
            return (false, true, null);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var retryAfter = response.Headers.RetryAfter?.Delta;
                _logger.LogWarning(
                    "Slack 429 for channel {Channel}, Retry-After={RetryAfter}", channel, retryAfter);
                return (false, true, retryAfter);
            }

            if (response.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout)
            {
                _logger.LogWarning(
                    "Slack {Status} for channel {Channel}", (int)response.StatusCode, channel);
                return (false, true, null);
            }

            var body = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Slack HTTP {Status} for channel {Channel}: {Body}",
                    (int)response.StatusCode, channel, body);
                return (false, false, null);
            }

            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;
            var ok = root.TryGetProperty("ok", out var okProp) && okProp.ValueKind == JsonValueKind.True;
            if (!ok)
            {
                var error = root.TryGetProperty("error", out var errProp)
                    ? errProp.GetString()
                    : "unknown_error";
                var isRateLimited = string.Equals(error, "ratelimited", StringComparison.OrdinalIgnoreCase);
                _logger.LogWarning(
                    "Slack ok:false for channel {Channel}: error={Error}", channel, error);
                return (false, isRateLimited, isRateLimited ? TimeSpan.FromSeconds(1) : null);
            }

            var ts = root.TryGetProperty("ts", out var tsProp) ? tsProp.GetString() : null;
            _logger.LogInformation(
                "Slack message sent to {Channel} ts={Ts}",
                channel, ts);
            return (true, false, null);
        }
    }

    private static TimeSpan GetJitteredDelay(int attempt)
    {
        var baseMs = (int)(1000 * Math.Pow(2, attempt - 1));
        var jitter = Random.Shared.Next(0, baseMs / 2);
        return TimeSpan.FromMilliseconds(baseMs + jitter);
    }

    private static IReadOnlyList<string> SplitIntoChunks(string text, int maxLength)
    {
        if (text.Length <= maxLength)
        {
            return [text];
        }

        var chunks = new List<string>();
        var start = 0;
        while (start < text.Length)
        {
            var length = Math.Min(maxLength, text.Length - start);
            if (start + length < text.Length)
            {
                // Break at last newline or space to avoid cutting mid-word.
                var breakAt = text.LastIndexOfAny(['\n', ' '], start + length - 1, length);
                if (breakAt > start)
                {
                    length = breakAt - start;
                }
            }

            chunks.Add(text.Substring(start, length).Trim());
            start += length;
            while (start < text.Length && char.IsWhiteSpace(text[start]))
            {
                start++;
            }
        }

        return chunks;
    }

    private string ResolveRoutingPath(string eventKey)
    {
        if (_settings.Routing is null || _settings.Routing.Count == 0)
        {
            throw new InvalidOperationException("Slack configuration is invalid: Routing is missing.");
        }

        if (!_settings.Routing.TryGetValue(eventKey, out var routePath) || string.IsNullOrWhiteSpace(routePath))
        {
            throw new InvalidOperationException(
                $"Slack configuration is invalid: routing for event '{eventKey}' is missing.");
        }

        return routePath;
    }

    private string ResolveChannel(string routePath)
    {
        if (_settings.Channels is null || _settings.Channels.Count == 0)
        {
            throw new InvalidOperationException("Slack configuration is invalid: Channels is missing.");
        }

        var segments = routePath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length != 2)
        {
            throw new InvalidOperationException(
                $"Slack configuration is invalid: routing path '{routePath}' is not valid.");
        }

        var channelGroup = segments[0];
        var channelKey = segments[1];

        if (!_settings.Channels.TryGetValue(channelGroup, out var group) || group is null)
        {
            throw new InvalidOperationException(
                $"Slack configuration is invalid: channel group '{channelGroup}' was not found for route '{routePath}'.");
        }

        if (!group.TryGetValue(channelKey, out var channel) || string.IsNullOrWhiteSpace(channel))
        {
            throw new InvalidOperationException(
                $"Slack configuration is invalid: channel segment '{channelKey}' was not found for route '{routePath}'.");
        }

        return channel;
    }

    private static string BuildText(NotificationMessage message)
    {
        var builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(message.Title))
        {
            builder.Append(message.Title.Trim()).Append('\n');
        }

        builder.Append($"Severity: {message.Severity}\n");

        if (builder.Length > 0)
        {
            builder.Append('\n');
        }

        builder.Append(message.Text.Trim());
        return builder.ToString();
    }
}
