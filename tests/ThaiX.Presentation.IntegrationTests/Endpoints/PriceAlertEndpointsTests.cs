using System.Net;
using System.Net.Http.Json;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.SearchSymbols;
using ThaiX.Application.Features.PriceAlerts.Queries.GetPriceAlerts;
using ThaiX.Domain.Aggregates.PriceAlerts;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class PriceAlertEndpointsTests : IntegrationTestBase
{
    public PriceAlertEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    #region List

    [Fact]
    public async Task GetPriceAlerts_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/price-alerts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPriceAlerts_WithoutPriceAlertReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"alert-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/price-alerts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPriceAlerts_WithPriceAlertReadPermission_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var symbol = $"BTC{Guid.NewGuid():N}"[..15].ToUpperInvariant();
        var createdAlertId = await CreatePriceAlertAsync(symbol, AssetType.CryptoSpot, AlertCondition.Above, 12345.67m, "List happy path", isOneTime: false);

        // Act
        var response = await Client.GetAsync($"/api/price-alerts?searchTerm={Uri.EscapeDataString(symbol)}&pageNumber=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ReadPagedResponseAsync<PriceAlertListItemDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNullOrEmpty();
        envelope.Data!.Any(item => item.Id == createdAlertId).Should().BeTrue();
    }

    [Fact]
    public async Task GetPriceAlerts_WithSearchTerm_ShouldReturnFilteredItems()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var matchedSymbol = $"M{Guid.NewGuid():N}"[..12].ToUpperInvariant();
        var unmatchedSymbol = $"O{Guid.NewGuid():N}"[..12].ToUpperInvariant();
        await CreatePriceAlertAsync(matchedSymbol, AssetType.CryptoSpot, AlertCondition.Above, 100m, null, isOneTime: false);
        await CreatePriceAlertAsync(unmatchedSymbol, AssetType.CryptoSpot, AlertCondition.Below, 200m, null, isOneTime: true);

        // Act
        var response = await Client.GetAsync($"/api/price-alerts?searchTerm={Uri.EscapeDataString(matchedSymbol)}&pageNumber=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ReadPagedResponseAsync<PriceAlertListItemDto>(response);
        envelope.Data.Should().NotBeNullOrEmpty();
        envelope.Data!.All(item => item.Symbol.Contains(matchedSymbol, StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public async Task GetPriceAlerts_WithIsEnabledFilter_ShouldReturnFilteredItems()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var symbol = $"E{Guid.NewGuid():N}"[..12].ToUpperInvariant();
        var alertId = await CreatePriceAlertAsync(symbol, AssetType.CryptoSpot, AlertCondition.Above, 100m, null, isOneTime: false);

        await Client.PutAsJsonAsync($"/api/price-alerts/{alertId}", new
        {
            Condition = AlertCondition.Below,
            TargetPrice = 90m,
            Note = "Disabled for filter",
            IsOneTime = true,
            IsEnabled = false
        });

        // Act
        var response = await Client.GetAsync($"/api/price-alerts?searchTerm={Uri.EscapeDataString(symbol)}&isEnabled=false&pageNumber=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ReadPagedResponseAsync<PriceAlertListItemDto>(response);
        envelope.Data.Should().NotBeNullOrEmpty();
        envelope.Data!.Single(item => item.Id == alertId).IsEnabled.Should().BeFalse();
    }

    #endregion

    #region Symbols

    [Fact]
    public async Task GetPriceAlertSymbols_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/master-data/symbols/search?assetType=CryptoSpot&search=BTC&page=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPriceAlertSymbols_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"symbols-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/master-data/symbols/search?assetType=CryptoSpot&search=BTC&page=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Create

    [Fact]
    public async Task CreatePriceAlert_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync("/api/price-alerts", new
        {
            Symbol = "BTCUSDT",
            AssetType = AssetType.CryptoSpot,
            Condition = AlertCondition.Above,
            TargetPrice = 100000m,
            Note = "No auth",
            IsOneTime = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePriceAlert_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"pricealert-create-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/price-alerts", new
        {
            Symbol = "BTCUSDT",
            AssetType = AssetType.CryptoSpot,
            Condition = AlertCondition.Above,
            TargetPrice = 100000m,
            Note = "No permission",
            IsOneTime = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePriceAlert_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var symbol = $"C{Guid.NewGuid():N}"[..12].ToUpperInvariant();
        var request = new
        {
            Symbol = symbol,
            AssetType = AssetType.CryptoSpot,
            Condition = AlertCondition.Above,
            TargetPrice = 12345.67m,
            Note = "Created by integration test",
            IsOneTime = true
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/price-alerts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var envelope = await ReadResponseAsync<Guid>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreatePriceAlert_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var request = new
        {
            Symbol = string.Empty,
            AssetType = 0,
            Condition = 0,
            TargetPrice = 0m,
            Note = new string('x', 501),
            IsOneTime = false
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/price-alerts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ReadResponseAsync<object>(response);
        envelope.Success.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Details.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Update

    [Fact]
    public async Task UpdatePriceAlert_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PutAsJsonAsync($"/api/price-alerts/{Guid.NewGuid()}", new
        {
            Condition = AlertCondition.Below,
            TargetPrice = 999m,
            Note = "No auth",
            IsOneTime = false,
            IsEnabled = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePriceAlert_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"pricealert-update-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/price-alerts/{Guid.NewGuid()}", new
        {
            Condition = AlertCondition.Below,
            TargetPrice = 999m,
            Note = "No permission",
            IsOneTime = false,
            IsEnabled = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdatePriceAlert_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var symbol = $"U{Guid.NewGuid():N}"[..12].ToUpperInvariant();
        var alertId = await CreatePriceAlertAsync(symbol, AssetType.CryptoSpot, AlertCondition.Above, 100m, "Before update", isOneTime: false);
        var request = new
        {
            Condition = AlertCondition.Below,
            TargetPrice = 90m,
            Note = "After update",
            IsOneTime = true,
            IsEnabled = false
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/price-alerts/{alertId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await Client.GetAsync($"/api/price-alerts?searchTerm={Uri.EscapeDataString(symbol)}&pageNumber=1&pageSize=20");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listEnvelope = await ReadPagedResponseAsync<PriceAlertListItemDto>(listResponse);
        var updated = listEnvelope.Data!.Single(item => item.Id == alertId);
        updated.Condition.Should().Be(AlertCondition.Below);
        updated.TargetPrice.Should().Be(90m);
        updated.Note.Should().Be("After update");
        updated.IsEnabled.Should().BeFalse();
        updated.IsOneTime.Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePriceAlert_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var alertId = await CreatePriceAlertAsync($"I{Guid.NewGuid():N}"[..12].ToUpperInvariant(), AssetType.CryptoSpot, AlertCondition.Above, 100m, null, isOneTime: false);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/price-alerts/{alertId}", new
        {
            Condition = 0,
            TargetPrice = 0m,
            Note = new string('x', 501),
            IsOneTime = false,
            IsEnabled = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ReadResponseAsync<object>(response);
        envelope.Success.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Details.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdatePriceAlert_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/price-alerts/{Guid.NewGuid()}", new
        {
            Condition = AlertCondition.Above,
            TargetPrice = 100m,
            Note = "Unknown id",
            IsOneTime = false,
            IsEnabled = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeletePriceAlert_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.DeleteAsync($"/api/price-alerts/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeletePriceAlert_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"pricealert-delete-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.DeleteAsync($"/api/price-alerts/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeletePriceAlert_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var symbol = $"D{Guid.NewGuid():N}"[..12].ToUpperInvariant();
        var alertId = await CreatePriceAlertAsync(symbol, AssetType.CryptoSpot, AlertCondition.Above, 100m, null, isOneTime: false);

        // Act
        var response = await Client.DeleteAsync($"/api/price-alerts/{alertId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await Client.GetAsync($"/api/price-alerts?searchTerm={Uri.EscapeDataString(symbol)}&pageNumber=1&pageSize=20");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listEnvelope = await ReadPagedResponseAsync<PriceAlertListItemDto>(listResponse);
        listEnvelope.Data.Should().NotBeNull();
        listEnvelope.Data!.Any(item => item.Id == alertId).Should().BeFalse();
    }

    [Fact]
    public async Task DeletePriceAlert_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/price-alerts/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    private async Task<Guid> CreatePriceAlertAsync(
        string symbol,
        AssetType assetType,
        AlertCondition condition,
        decimal targetPrice,
        string? note,
        bool isOneTime)
    {
        var response = await Client.PostAsJsonAsync("/api/price-alerts", new
        {
            Symbol = symbol,
            AssetType = assetType,
            Condition = condition,
            TargetPrice = targetPrice,
            Note = note,
            IsOneTime = isOneTime
        });

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Create price alert failed with {response.StatusCode}: {errorBody}");
        }

        var envelope = await ReadResponseAsync<Guid>(response);
        return envelope.Data;
    }

    private static async Task<PagedApiResponse<T>> ReadPagedResponseAsync<T>(HttpResponseMessage response)
    {
        var envelope = await response.Content.ReadFromJsonAsync<PagedApiResponse<T>>(JsonOptions);
        return envelope ?? throw new InvalidOperationException("Unable to deserialize paged API response.");
    }
}
