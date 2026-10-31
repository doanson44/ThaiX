using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;
using ThaiX.Infrastructure.ExternalApis.Transport;

namespace ThaiX.Presentation.IntegrationTests.ExternalApis;

public sealed class ProxyTransportTests
{
    [Fact]
    public async Task SendAsync_ThroughCmsxProxy_ShouldUseApiKeyAndDeserializeEnvelopeBody()
    {
        // Arrange
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                statusCode = 200,
                reasonPhrase = "OK",
                body = "{\"method\":\"GET\"}"
            }))
        });
        var transport = CreateTransport(handler);
        var request = new ApiRequest { Url = "https://httpbin.org/anything" };

        // Act
        var result = await transport.SendAsync<TargetResponse>(request, CancellationToken.None);

        // Assert
        result!.Method.Should().Be("GET");
        handler.RequestUri.Should().Be(new Uri("https://cmsx.runasp.net/api/proxy"));
        handler.ApiKey.Should().Be("cmsx-secret");
        handler.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_ThroughAlternateProxy_ShouldKeepBearerAuthenticationAndRawResponse()
    {
        // Arrange
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"serverTime\":123}")
        });
        var transport = CreateTransport(handler);
        var request = new ApiRequest { Url = "https://api.binance.com/api/v3/time" };

        // Act
        var result = await transport.SendAsync<ServerTimeResponse>(request, CancellationToken.None);

        // Assert
        result!.ServerTime.Should().Be(123);
        handler.RequestUri.Should().Be(new Uri("https://binance-proxy.example/api/proxy"));
        handler.ApiKey.Should().BeNull();
        handler.Authorization.Should().Be("Bearer cmsx-secret");
    }

    [Fact]
    public async Task SendAsync_WhenCmsxTargetReturnsError_ShouldThrowWithTargetStatusCode()
    {
        // Arrange
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                statusCode = 429,
                reasonPhrase = "Too Many Requests",
                body = "{\"error\":\"rate limited\"}"
            }))
        });
        var transport = CreateTransport(handler);
        var request = new ApiRequest { Url = "https://httpbin.org/status/429" };

        // Act
        var act = () => transport.SendAsync<TargetResponse>(request, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<HttpRequestException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    private static ProxyTransport CreateTransport(RecordingHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://cmsx.runasp.net/")
        };
        var options = Options.Create(new ProxyOptions
        {
            BaseUrl = "https://cmsx.runasp.net",
            BinanceBaseUrl = "https://binance-proxy.example",
            InvestingBaseUrl = "https://investing-proxy.example",
            Token = "cmsx-secret"
        });

        return new ProxyTransport(httpClient, options);
    }

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }
        public string? ApiKey { get; private set; }
        public string? Authorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            ApiKey = request.Headers.TryGetValues("X-Content-Management-Key", out var values)
                ? values.Single()
                : null;
            Authorization = request.Headers.Authorization?.ToString();

            return Task.FromResult(responseFactory(request));
        }
    }

    private sealed class TargetResponse
    {
        public string? Method { get; init; }
    }

    private sealed class ServerTimeResponse
    {
        public long ServerTime { get; init; }
    }
}
