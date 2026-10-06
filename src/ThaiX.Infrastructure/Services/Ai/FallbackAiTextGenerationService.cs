using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Infrastructure.Configuration;

namespace ThaiX.Infrastructure.Services.Ai;

public sealed class FallbackAiTextGenerationService : IAiTextGenerationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AiSettings _settings;
    private readonly INotificationRouter _notificationRouter;
    private readonly ILogger<FallbackAiTextGenerationService> _logger;

    public FallbackAiTextGenerationService(
        IHttpClientFactory httpClientFactory,
        IOptions<AiSettings> options,
        INotificationRouter notificationRouter,
        ILogger<FallbackAiTextGenerationService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _notificationRouter = notificationRouter ?? throw new ArgumentNullException(nameof(notificationRouter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AiTextGenerationResult> GenerateAsync(
        string prompt,
        string? systemPrompt,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt is required.", nameof(prompt));
        }

        var providers = ResolveOrderedProviders();
        if (providers.Count == 0)
        {
            throw new InvalidOperationException("No enabled AI provider is configured.");
        }

        var failures = new List<string>();
        foreach (var providerName in providers)
        {
            if (!_settings.Providers.TryGetValue(providerName, out var provider))
            {
                failures.Add($"{providerName}: Missing provider configuration");
                continue;
            }

            try
            {
                var text = providerName.Equals("Gemini", StringComparison.OrdinalIgnoreCase)
                    ? await CallGeminiAsync(provider, prompt, systemPrompt, ct)
                    : await CallOpenAiCompatibleAsync(providerName, provider, prompt, systemPrompt, ct);

                _logger.LogInformation(
                    "AI generation succeeded using provider {Provider} and model {Model}",
                    providerName,
                    provider.Model);

                return new AiTextGenerationResult(providerName, provider.Model, text);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
            {
                var reason = ExtractFailureReason(ex);
                failures.Add($"{providerName}: {reason}");
                _logger.LogWarning(ex, "AI provider {Provider} failed. Falling back to next provider.", providerName);
            }
        }

        await NotifyAllProvidersFailedAsync(failures, ct);

        throw new InvalidOperationException(
            $"All AI providers failed: {string.Join(" | ", failures)}");
    }

    private List<string> ResolveOrderedProviders()
    {
        var ordered = new List<string>();
        foreach (var providerName in _settings.ProviderOrder)
        {
            if (_settings.Providers.TryGetValue(providerName, out var provider) && provider.Enabled)
            {
                ordered.Add(providerName);
            }
        }

        if (ordered.Count > 0)
        {
            return ordered;
        }

        if (_settings.Providers.TryGetValue(_settings.PreferredProvider, out var preferred) && preferred.Enabled)
        {
            ordered.Add(_settings.PreferredProvider);
        }

        ordered.AddRange(_settings.Providers
            .Where(kv => kv.Value.Enabled && !kv.Key.Equals(_settings.PreferredProvider, StringComparison.OrdinalIgnoreCase))
            .Select(kv => kv.Key));

        return ordered;
    }

    private async Task<string> CallGeminiAsync(
        AiProviderSettings provider,
        string prompt,
        string? systemPrompt,
        CancellationToken ct)
    {
        using var client = CreateClient(provider.BaseUrl);

        var url = $"v1beta/models/{provider.Model}:generateContent?key={Uri.EscapeDataString(provider.ApiKey)}";
        var userText = string.IsNullOrWhiteSpace(systemPrompt)
            ? prompt
            : $"{systemPrompt}\n\n{prompt}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = userText } }
                        }
                    }
                }),
                Encoding.UTF8,
                "application/json")
        };

        using var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        EnsureSuccess(provider.Model, response.StatusCode, body);

        using var json = JsonDocument.Parse(body);
        var text = json.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return string.IsNullOrWhiteSpace(text)
            ? throw new InvalidOperationException("Gemini returned empty response.")
            : text;
    }

    private async Task<string> CallOpenAiCompatibleAsync(
        string providerName,
        AiProviderSettings provider,
        string prompt,
        string? systemPrompt,
        CancellationToken ct)
    {
        using var client = CreateClient(provider.BaseUrl);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", provider.ApiKey);

        var messages = new List<object>();
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            messages.Add(new { role = "system", content = systemPrompt });
        }

        messages.Add(new { role = "user", content = prompt });

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    model = provider.Model,
                    messages,
                    temperature = 0.2
                }),
                Encoding.UTF8,
                "application/json")
        };

        using var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        EnsureSuccess(provider.Model, response.StatusCode, body);

        using var json = JsonDocument.Parse(body);
        var text = json.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return string.IsNullOrWhiteSpace(text)
            ? throw new InvalidOperationException($"{providerName} returned empty response.")
            : text;
    }

    private HttpClient CreateClient(string baseUrl)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/", UriKind.Absolute);
        client.Timeout = TimeSpan.FromSeconds(_settings.RequestTimeoutSeconds);
        return client;
    }

    private static void EnsureSuccess(string model, HttpStatusCode statusCode, string responseBody)
    {
        if ((int)statusCode is >= 200 and < 300)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Model {model} request failed with status {(int)statusCode}: {responseBody}");
    }

    private static string ExtractFailureReason(Exception ex)
    {
        var message = ex.Message;
        if (message.Contains("429", StringComparison.OrdinalIgnoreCase)
            || message.Contains("quota", StringComparison.OrdinalIgnoreCase)
            || message.Contains("resource exhausted", StringComparison.OrdinalIgnoreCase)
            || message.Contains("insufficient_quota", StringComparison.OrdinalIgnoreCase)
            || message.Contains("rate limit", StringComparison.OrdinalIgnoreCase))
        {
            return "Quota or rate limit exceeded";
        }

        if (ex is TaskCanceledException)
        {
            return "Request timed out";
        }

        return message;
    }

    private async Task NotifyAllProvidersFailedAsync(IReadOnlyList<string> failures, CancellationToken ct)
    {
        var message = new NotificationMessage
        {
            Kind = NotificationKind.SystemAlert,
            Severity = NotificationSeverity.Critical,
            Title = "AI provider fallback exhausted",
            Text = $"All configured AI providers failed. Failures: {string.Join(" | ", failures)}"
        };

        try
        {
            await _notificationRouter.DispatchAsync(message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create notification for AI provider exhaustion.");
        }
    }
}
