using System.Net;
using System.Net.Http.Json;
using ThaiX.Domain.Aggregates.TradingSuggestions;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class TradingEndpointsTests : IntegrationTestBase
{
    public TradingEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetTradeSuggestion_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/trading/suggestion?symbol=BTCUSDT&marketType=Crypto&timeframe=Min15");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTradeSuggestion_WithoutTradingViewPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"trading-suggestion-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/trading/suggestion?symbol=BTCUSDT&marketType=Crypto&timeframe=Min15");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTradeSuggestion_WithInvalidTimeframe_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"trading-suggestion-reader-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.TradingView });
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/trading/suggestion?symbol=BTCUSDT&marketType=Crypto&timeframe=InvalidFrame");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTradeVerdict_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync("/api/trading/verdict", BuildVerdictRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTradeVerdict_WithoutTradingViewPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"trading-verdict-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/trading/verdict", BuildVerdictRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTradeVerdict_WithValidRequest_ShouldReturnVerdictAndReasoning()
    {
        // Arrange
        var email = $"trading-verdict-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.TradingView });
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/trading/verdict", BuildVerdictRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<TradeVerdictResponseDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.Verdict.Should().BeOneOf("TRADE", "NO TRADE", "REVIEW");
        envelope.Data.Reasoning.Should().NotBeNullOrWhiteSpace();
        envelope.Data.Reasoning.Should().Contain("Bullish");
    }

    private static object BuildVerdictRequest() => new
    {
        Symbol = "BTCUSDT",
        MarketType = "Crypto",
        Timeframe = "Min15",
        Trend = "Bullish",
        Momentum = "Strong",
        Setup = "Breakout",
        Signal = "BUY",
        EntryPrice = 65000m,
        StopLoss = 63500m,
        TakeProfit1 = 68000m,
        TakeProfit2 = 71000m,
        Confidence = 0.85m
    };

    private sealed record TradeVerdictResponseDto
    {
        public string Verdict { get; init; } = string.Empty;
        public string Reasoning { get; init; } = string.Empty;
    }

    [Fact]
    public async Task GetSuggestionHistory_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/trading/suggestion-history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSuggestionHistory_WithoutTradingViewPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"trading-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/trading/suggestion-history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSuggestionHistory_WithTradingViewPermission_ShouldReturnOk()
    {
        // Arrange
        var email = $"trading-reader-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.TradingView });
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/trading/suggestion-history?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EvaluateSuggestionHistory_WithEmptySymbols_ShouldReturnOkWithZeroMatches()
    {
        // Arrange
        var email = $"trading-evaluate-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.TradingView });
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/trading/suggestion-history/evaluate", new
        {
            AssetClass = SuggestionAssetClass.Crypto,
            LookbackReports = 12,
            Symbols = Array.Empty<object>()
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<EvaluateSuggestionHistoryResponse>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.MatchedCount.Should().Be(0);
        envelope.Data.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ExportSuggestionHistory_WithInvalidFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"trading-export-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.TradingView });
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/trading/suggestion-history/export?reportKey=test&assetClass=Crypto&format=zip");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExportSuggestionHistory_WithUnknownReportKey_ShouldReturnNotFound()
    {
        // Arrange
        var email = $"trading-export-notfound-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.TradingView });
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/trading/suggestion-history/export?reportKey=missing-report&assetClass=Crypto&format=csv");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TriggerWeeklySuggestionRun_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsync("/api/trading/weekly-suggestion/run", content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TriggerWeeklySuggestionRun_WithoutTradingViewPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"trading-run-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsync("/api/trading/weekly-suggestion/run", content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TriggerWeeklySuggestionRunSpot_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsync("/api/trading/weekly-suggestion/run-spot", content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TriggerWeeklySuggestionRunSpot_WithoutTradingViewPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"trading-runspot-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsync("/api/trading/weekly-suggestion/run-spot", content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private sealed record EvaluateSuggestionHistoryResponse
    {
        public int MatchedCount { get; init; }
        public IReadOnlyList<object> Items { get; init; } = [];
    }
}
