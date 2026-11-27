using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;
using ThaiX.Infrastructure.ExternalApis.Resilience;
using ThaiX.Infrastructure.ExternalApis.Transport;

namespace ThaiX.Presentation.IntegrationTests.ExternalApis;

public sealed class ApiTransportRouterTests
{
    private const string BybitUrl = "https://api.bybit.com/v5/market/tickers?category=linear";

    [Fact]
    public async Task SendAsync_WhenRequestHasBrowserHeaders_ShouldForwardThemToDirectApi()
    {
        // Arrange
        string? accept = null;
        string? origin = null;
        string? referer = null;
        string? acceptLanguage = null;
        var directHandler = new RecordingHandler(message =>
        {
            message.Headers.TryGetValues("Accept", out var acceptValues);
            message.Headers.TryGetValues("Origin", out var originValues);
            message.Headers.TryGetValues("Referer", out var refererValues);
            message.Headers.TryGetValues("Accept-Language", out var acceptLanguageValues);
            accept = acceptValues is null ? null : string.Join(", ", acceptValues);
            origin = originValues?.FirstOrDefault();
            referer = refererValues?.FirstOrDefault();
            acceptLanguage = acceptLanguageValues is null ? null : string.Join(", ", acceptLanguageValues);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""["BTCUSDT"]""")
            };
        });
        var proxyHandler = new RecordingHandler(_ => throw new InvalidOperationException("Proxy should not be called."));
        var router = CreateRouter(directHandler, proxyHandler);
        var request = CreateRequest();
        request.Url = "https://api.binance.com/api/v3/ticker/price?symbol=BTCUSDT";
        request.Headers = new Dictionary<string, string>
        {
            ["Accept"] = "application/json, text/plain, */*",
            ["Accept-Language"] = "en-US,en;q=0.9",
            ["Origin"] = "https://www.binance.com",
            ["Referer"] = "https://www.binance.com/"
        };

        // Act
        var result = await router.SendAsync<List<string>>(request, CancellationToken.None);

        // Assert
        result.Should().ContainSingle().Which.Should().Be("BTCUSDT");
        accept.Should().Contain("application/json, text/plain, */*");
        origin.Should().Be("https://www.binance.com");
        referer.Should().Be("https://www.binance.com/");
        acceptLanguage.Should().Contain("en-US").And.Contain("en; q=0.9");
        proxyHandler.RequestCount.Should().Be(0);
    }

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
    public async Task SendAsync_WhenDirectResponseDataCollectionIsEmpty_ShouldFallbackToProxy()
    {
        // Arrange
        var directHandler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"data\":[]}")
        });
        var proxyHandler = new RecordingHandler(_ => CreateProxyResponse("{\"data\":[\"BTCUSDT\"]}"));
        var router = CreateRouter(directHandler, proxyHandler);
        var request = CreateRequest();

        // Act
        var result = await router.SendAsync<ApiEnvelope>(request, CancellationToken.None);

        // Assert
        result!.Data.Should().ContainSingle().Which.Should().Be("BTCUSDT");
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

    public sealed class ApiEnvelope
    {
        public List<string> Data { get; init; } = [];
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