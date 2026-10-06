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

public sealed class TelegramNotificationService : ITelegramNotificationService
{
    private const int ChunkLength = 4000;
    private const int MaxRetries = 3;

    private readonly HttpClient _httpClient;
    private readonly TelegramSettings _settings;
    private readonly ILogger<TelegramNotificationService> _logger;

    public TelegramNotificationService(
        HttpClient httpClient,
        IOptions<TelegramSettings> options,
        ILogger<TelegramNotificationService> logger)
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
            throw new ArgumentException("Telegram message text is required.", nameof(message));
        }

        var eventKey = message.Kind.ToString();
        var routePath = ResolveRoutingPath(eventKey);
        var chatId = ResolveChatId(routePath);
        var fullText = BuildText(message);
        var chunks = SplitIntoChunks(fullText, ChunkLength);

        using var _ = await AsyncLock.GetLockByKey($"telegram:chat:{chatId}").LockAsync(ct);
        {
            for (var i = 0; i < chunks.Count; i++)
            {
                var text = chunks.Count > 1 ? $"{chunks[i]} ({i + 1}/{chunks.Count})" : chunks[i];
                await SendWithRetryAsync(chatId, text, ct);

                if (i < chunks.Count - 1)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), ct);
                }
            }
        }
    }

    public async Task SendToChatAsync(string chatId, string text, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chatId);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        await SendWithRetryAsync(chatId, EncodeHtml(text), ct);
    }

    public async Task SendRenderedToChatAsync(string chatId, string htmlText, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chatId);
        ArgumentException.ThrowIfNullOrWhiteSpace(htmlText);

        await SendWithRetryAsync(chatId, htmlText, ct);
    }

    private async Task SendWithRetryAsync(string chatId, string text, CancellationToken ct)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            var (success, shouldRetry, retryAfter) = await TrySendAsync(chatId, text, ct);
            if (success)
            {
                return;
            }

            if (!shouldRetry || attempt == MaxRetries)
            {
                throw new InvalidOperationException(
                    $"Telegram send failed after {attempt} attempt(s) for chat '{chatId}'.");
            }

            var delay = retryAfter ?? GetJitteredDelay(attempt);
            _logger.LogWarning(
                "Telegram send attempt {Attempt}/{Max} failed for chat {ChatId}, retrying in {DelayMs}ms",
                attempt, MaxRetries, chatId, delay.TotalMilliseconds);
            await Task.Delay(delay, ct);
        }
    }

    private async Task<(bool Success, bool ShouldRetry, TimeSpan? RetryAfter)> TrySendAsync(
        string chatId, string text, CancellationToken ct)
    {
        var endpoint = $"https://api.telegram.org/bot{_settings.BotToken}/sendMessage";

        var payload = new { chat_id = chatId, text, parse_mode = "HTML" };

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
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
            _logger.LogWarning("Telegram request timed out for chat {ChatId}", chatId);
            return (false, true, null);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Telegram network error for chat {ChatId}", chatId);
            return (false, true, null);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(3);
                _logger.LogWarning(
                    "Telegram 429 for chat {ChatId}, Retry-After={RetryAfter}", chatId, retryAfter);
                return (false, true, retryAfter);
            }

            if (response.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout)
            {
                _logger.LogWarning(
                    "Telegram {Status} for chat {ChatId}", (int)response.StatusCode, chatId);
                return (false, true, null);
            }

            var body = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Telegram HTTP {Status} for chat {ChatId}: {Body}",
                    (int)response.StatusCode, chatId, body);
                return (false, false, null);
            }

            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;
            var ok = root.TryGetProperty("ok", out var okProp) && okProp.ValueKind == JsonValueKind.True;
            if (!ok)
            {
                var errorCode = root.TryGetProperty("error_code", out var errCodeProp)
                    ? errCodeProp.GetInt32().ToString()
                    : "unknown";
                var description = root.TryGetProperty("description", out var descProp)
                    ? descProp.GetString()
                    : "unknown_error";
                var isRateLimited = errorCode == "429";
                _logger.LogWarning(
                    "Telegram ok:false for chat {ChatId}: error_code={ErrorCode} description={Description}",
                    chatId, errorCode, description);
                return (false, isRateLimited, isRateLimited ? TimeSpan.FromSeconds(3) : null);
            }

            var messageId = root.TryGetProperty("result", out var result)
                && result.TryGetProperty("message_id", out var mid)
                ? mid.GetInt32().ToString()
                : null;
            _logger.LogInformation(
                "Telegram message sent to {ChatId} message_id={MessageId}",
                chatId, messageId);
            return (true, false, null);
        }
    }

    private static string EncodeHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
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
            throw new InvalidOperationException("Telegram configuration is invalid: Routing is missing.");
        }

        if (!_settings.Routing.TryGetValue(eventKey, out var routePath) || string.IsNullOrWhiteSpace(routePath))
        {
            throw new InvalidOperationException(
                $"Telegram configuration is invalid: routing for event '{eventKey}' is missing.");
        }

        return routePath;
    }

    private string ResolveChatId(string routePath)
    {
        if (_settings.Channels is null || _settings.Channels.Count == 0)
        {
            throw new InvalidOperationException("Telegram configuration is invalid: Channels is missing.");
        }

        var segments = routePath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length != 2)
        {
            throw new InvalidOperationException(
                $"Telegram configuration is invalid: routing path '{routePath}' is not valid.");
        }

        var channelGroup = segments[0];
        var channelKey = segments[1];

        if (!_settings.Channels.TryGetValue(channelGroup, out var group) || group is null)
        {
            throw new InvalidOperationException(
                $"Telegram configuration is invalid: channel group '{channelGroup}' was not found for route '{routePath}'.");
        }

        if (!group.TryGetValue(channelKey, out var chatId) || string.IsNullOrWhiteSpace(chatId))
        {
            throw new InvalidOperationException(
                $"Telegram configuration is invalid: channel segment '{channelKey}' was not found for route '{routePath}'.");
        }

        return chatId;
    }

    private static string BuildText(NotificationMessage message)
    {
        var builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(message.Title))
        {
            builder.Append($"<b>{EscapeHtml(message.Title.Trim())}</b>\n");
        }

        builder.Append($"Severity: {EscapeHtml(message.Severity.ToString())}\n");

        if (builder.Length > 0)
        {
            builder.Append('\n');
        }

        builder.Append(EscapeHtml(message.Text.Trim()));
        return builder.ToString();
    }

    private static string EscapeHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}
