using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.Configuration;
using ThaiX.Infrastructure.Services;

namespace ThaiX.Presentation.IntegrationTests.Services;

public sealed class TelegramNotificationServiceTests
{
    [Fact]
    public async Task SendRenderedToChatAsync_WhenMessageExceedsLimit_SendsBalancedHtmlChunks()
    {
        var handler = new TelegramMessageHandler();
        using var httpClient = new HttpClient(handler);
        var service = new TelegramNotificationService(
            httpClient,
            Options.Create(new TelegramSettings { BotToken = "test-token" }),
            NullLogger<TelegramNotificationService>.Instance);
        var html = $"<b>{new string('a', 3400)} &amp; {new string('b', 3400)}</b>";

        await service.SendRenderedToChatAsync("-100123", html, CancellationToken.None);

        handler.Messages.Count.Should().BeGreaterThan(1);
        handler.Messages.Should().OnlyContain(message =>
            message.StartsWith("<b>", StringComparison.Ordinal)
            && message.EndsWith("</b>", StringComparison.Ordinal));

        var visibleChunks = handler.Messages
            .Select(message => WebUtility.HtmlDecode(Regex.Replace(message, "<[^>]+>", string.Empty)))
            .ToArray();
        var expectedVisibleText = WebUtility.HtmlDecode(Regex.Replace(html, "<[^>]+>", string.Empty));

        visibleChunks.Should().OnlyContain(chunk => chunk.Length <= 3500);
        string.Concat(visibleChunks).Should().Be(expectedVisibleText);
    }

    private sealed class TelegramMessageHandler : HttpMessageHandler
    {
        public List<string> Messages { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            using var json = JsonDocument.Parse(body);
            Messages.Add(json.RootElement.GetProperty("text").GetString()!);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true,\"result\":{\"message_id\":1}}")
            };
        }
    }
}
