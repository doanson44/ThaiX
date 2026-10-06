using System.Net;
using System.Net.Http.Json;
using ThaiX.Application.Features.MarketScanner.Queries.GetMarketScannerRules;
using ThaiX.Domain.Aggregates.MarketScanner;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class MarketScannerEndpointsTests : IntegrationTestBase
{
    public MarketScannerEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetMarketScannerRules_WithReadPermission_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var ruleId = await CreateRuleAsync("Pump Rule", MarketSignalType.PricePump, MarketTimeWindow.ThreeMinutes, 2.5m);

        // Act
        var response = await Client.GetAsync("/api/market-scanner/rules?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadPagedResponseAsync<MarketScannerRuleListItemDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNullOrEmpty();
        envelope.Data!.Single(x => x.Id == ruleId).Name.Should().Be("Pump Rule");
    }

    [Fact]
    public async Task GetMarketScannerRules_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/market-scanner/rules");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMarketScannerRules_WithoutReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"scanner-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/market-scanner/rules");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateMarketScannerRule_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync("/api/market-scanner/rules", new
        {
            Name = "Rule",
            SignalType = MarketSignalType.PricePump,
            Window = MarketTimeWindow.ThreeMinutes,
            Threshold = 2m
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateMarketScannerRule_WithoutWritePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"scanner-create-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/market-scanner/rules", new
        {
            Name = "Rule",
            SignalType = MarketSignalType.PricePump,
            Window = MarketTimeWindow.ThreeMinutes,
            Threshold = 2m
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateMarketScannerRule_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/market-scanner/rules", new
        {
            Name = "Volume Spike Rule",
            SignalType = MarketSignalType.VolumeSpike,
            Window = MarketTimeWindow.SixMinutes,
            Threshold = 3.2m
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await ReadResponseAsync<Guid>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateMarketScannerRule_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/market-scanner/rules", new
        {
            Name = string.Empty,
            SignalType = 0,
            Window = 0,
            Threshold = -1m
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ReadResponseAsync<object>(response);
        envelope.Success.Should().BeFalse();
        envelope.Error!.Details.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateMarketScannerRule_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var ruleId = await CreateRuleAsync("Funding Rule", MarketSignalType.FundingFlip, MarketTimeWindow.TwelveMinutes, 1.5m);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/market-scanner/rules/{ruleId}", new
        {
            Name = "Funding Rule Updated",
            Window = MarketTimeWindow.OneHour,
            Threshold = 2.1m,
            IsEnabled = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var listResponse = await Client.GetAsync("/api/market-scanner/rules?pageNumber=1&pageSize=20");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await ReadPagedResponseAsync<MarketScannerRuleListItemDto>(listResponse);
        var updated = list.Data!.Single(x => x.Id == ruleId);
        updated.Name.Should().Be("Funding Rule Updated");
        updated.Window.Should().Be(MarketTimeWindow.OneHour);
        updated.Threshold.Should().Be(2.1m);
        updated.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateMarketScannerRule_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var ruleId = await CreateRuleAsync("Invalid Update Rule", MarketSignalType.PricePump, MarketTimeWindow.ThreeMinutes, 1.2m);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/market-scanner/rules/{ruleId}", new
        {
            Name = string.Empty,
            Window = 0,
            Threshold = -1m,
            IsEnabled = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateMarketScannerRule_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/market-scanner/rules/{Guid.NewGuid()}", new
        {
            Name = "Unknown",
            Window = MarketTimeWindow.OneHour,
            Threshold = 1m,
            IsEnabled = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteMarketScannerRule_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var ruleId = await CreateRuleAsync("Delete Rule", MarketSignalType.FundingMultiple, MarketTimeWindow.ThirtyMinutes, 4m);

        // Act
        var response = await Client.DeleteAsync($"/api/market-scanner/rules/{ruleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var listResponse = await Client.GetAsync("/api/market-scanner/rules?pageNumber=1&pageSize=20");
        var list = await ReadPagedResponseAsync<MarketScannerRuleListItemDto>(listResponse);
        list.Data!.Any(x => x.Id == ruleId).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteMarketScannerRule_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/market-scanner/rules/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> CreateRuleAsync(string name, MarketSignalType signalType, MarketTimeWindow window, decimal? threshold)
    {
        var response = await Client.PostAsJsonAsync("/api/market-scanner/rules", new
        {
            Name = name,
            SignalType = signalType,
            Window = window,
            Threshold = threshold
        });

        response.EnsureSuccessStatusCode();
        var envelope = await ReadResponseAsync<Guid>(response);
        return envelope.Data;
    }

    private static async Task<PagedApiResponse<T>> ReadPagedResponseAsync<T>(HttpResponseMessage response)
    {
        var envelope = await response.Content.ReadFromJsonAsync<PagedApiResponse<T>>(JsonOptions);
        return envelope ?? throw new InvalidOperationException("Unable to deserialize paged API response.");
    }
}
