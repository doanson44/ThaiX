using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;
using ThaiX.Infrastructure.ExternalApis.Resilience;
using ThaiX.Infrastructure.ExternalApis.Transport;

namespace ThaiX.Presentation.IntegrationTests.ExternalApis;

public sealed class ApiTransportRouterTests
{
    private const string BybitUrl = "https://api.bybit.com/v5/market/tickers?category=linear";

    [Fact]
    public async Task SendAsync_WhenDirectRequestFails_ShouldFallbackToProxy()
    {
        // Arrange
        var directHandler = new RecordingHandler(_ => throw new HttpRequestException("Network unavailable."));
        var proxyHandler = new RecordingHandler(_ => CreateProxyResponse("[\"BTCUSDT\"]"));
        var router = CreateRouter(directHandler, proxyHandler);
        var request = CreateRequest();

        // Act
        var result = await router.SendAsync<List<string>>(request, CancellationToken.None);

        // Assert
        result.Should().ContainSingle().Which.Should().Be("BTCUSDT");
        directHandler.RequestCount.Should().Be(1);
        proxyHandler.RequestCount.Should().Be(1);
    }

    [Fact]
    public async Task SendAsync_WhenDirectResponseIsEmpty_ShouldFallbackToProxy()
    {
        // Arrange
        var directHandler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]")
        });
        var proxyHandler = new RecordingHandler(_ => CreateProxyResponse("[\"BTCUSDT\"]"));
        var router = CreateRouter(directHandler, proxyHandler);
        var request = CreateRequest();

        // Act
        var result = await router.SendAsync<List<string>>(request, CancellationToken.None);

        // Assert
        result.Should().ContainSingle().Which.Should().Be("BTCUSDT");
        directHandler.RequestCount.Should().Be(1);
        proxyHandler.RequestCount.Should().Be(1);
    }

    [Fact]
    public async Task SendAsync_WhenDirectResponseHasData_ShouldNotUseProxy()
    {
        // Arrange
        var directHandler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[\"ETHUSDT\"]")
        });
        var proxyHandler = new RecordingHandler(_ => throw new InvalidOperationException("Proxy should not be called."));
        var router = CreateRouter(directHandler, proxyHandler);
        var request = CreateRequest();

        // Act
        var result = await router.SendAsync<List<string>>(request, CancellationToken.None);

        // Assert
        result.Should().ContainSingle().Which.Should().Be("ETHUSDT");
        directHandler.RequestCount.Should().Be(1);
        proxyHandler.RequestCount.Should().Be(0);
    }

    [Fact]
    public async Task SendAsync_WhenDirectRequestIsRateLimited_ShouldNotFallbackToProxy()
    {
        // Arrange
        var directHandler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests));
        var proxyHandler = new RecordingHandler(_ => CreateProxyResponse("[\"BTCUSDT\"]"));
        var router = CreateRouter(directHandler, proxyHandler);
        var request = CreateRequest();

        // Act
        var act = () => router.SendAsync<List<string>>(request, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<HttpRequestException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        directHandler.RequestCount.Should().Be(1);
        proxyHandler.RequestCount.Should().Be(0);
    }

    [Fact]
    public async Task SendAsync_WhenFallbackIsDisabled_ShouldPropagateDirectFailure()
    {
        // Arrange
        var directHandler = new RecordingHandler(_ => throw new HttpRequestException("Network unavailable."));
        var proxyHandler = new RecordingHandler(_ => CreateProxyResponse("[\"BTCUSDT\"]"));
        var router = CreateRouter(directHandler, proxyHandler);
        var request = CreateRequest();
        request.EnableProxyFallback = false;

        // Act
        var act = () => router.SendAsync<List<string>>(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
        directHandler.RequestCount.Should().Be(1);
        proxyHandler.RequestCount.Should().Be(0);
    }

    private static ApiTransportRouter CreateRouter(
        HttpMessageHandler directHandler,
        HttpMessageHandler proxyHandler)
    {
        var directClient = new HttpClient(directHandler);
        var proxyClient = new HttpClient(proxyHandler)
        {
            BaseAddress = new Uri("https://proxy.example/")
        };
        var proxyOptions = Options.Create(new ProxyOptions
        {
            BaseUrl = "https://proxy.example",
            Token = "test-token"
        });

        return new ApiTransportRouter(
            Options.Create(new ExternalApisOptions { UseProxy = false }),
            new HttpTransport(directClient),
            new ProxyTransport(proxyClient, proxyOptions),
            new BrowserTransport(null!),
            new ExternalApiRateLimiter(5),
            NullLogger<ApiTransportRouter>.Instance);
    }

    private static ApiRequest CreateRequest()
    {
        return new ApiRequest
        {
            Url = BybitUrl,
            Method = HttpMethod.Get,
            UseProxy = false,
            EnableProxyFallback = true
        };
    }

    private static HttpResponseMessage CreateProxyResponse(string body)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($$"""{"statusCode":200,"reasonPhrase":"OK","body":{{System.Text.Json.JsonSerializer.Serialize(body)}}}""")
        };
    }

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            return Task.FromResult(responseFactory(request));
        }
    }
}
