using System.Net;
using System.Net.Http.Json;
using ThaiX.Application.Features.AssetPositions.Queries.GetCryptoPositions;
using ThaiX.Application.Features.AssetPositions.Queries.GetPositionTransactions;
using ThaiX.Domain.Aggregates.AssetPositions;
using ThaiX.Domain.Aggregates.Portfolios;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class CryptoPositionEndpointsTests : IntegrationTestBase
{
    public CryptoPositionEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    #region List

    [Fact]
    public async Task GetCryptoPositions_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/crypto-positions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCryptoPositions_WithoutPositionReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"crypto-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/crypto-positions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetCryptoPositions_WithPositionReadPermission_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Crypto List Portfolio");
        var createdId = await CreateCryptoPositionAsync(portfolioId, "BTCUSDT", quantity: 1.5m, price: 50000m, targetPrice: 60000m, stopLoss: 45000m, note: "List case");

        // Act
        var response = await Client.GetAsync($"/api/crypto-positions?portfolioId={portfolioId}&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ReadPagedResponseAsync<CryptoPositionListItemDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNullOrEmpty();
        var created = envelope.Data!.Single(item => item.Id == createdId);
        created.Symbol.Should().Be("BTCUSDT");
    }

    #endregion

    #region Create

    [Fact]
    public async Task CreateCryptoPosition_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync("/api/crypto-positions", new
        {
            PortfolioId = Guid.NewGuid(),
            Symbol = "BTCUSDT",
            Quantity = 1m,
            Price = 50000m,
            Fee = 1m,
            TransactedAt = DateTime.UtcNow,
            TargetPrice = 60000m,
            StopLoss = 45000m,
            Note = "No auth"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCryptoPosition_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"crypto-create-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/crypto-positions", new
        {
            PortfolioId = Guid.NewGuid(),
            Symbol = "BTCUSDT",
            Quantity = 1m,
            Price = 50000m,
            Fee = 1m,
            TransactedAt = DateTime.UtcNow,
            TargetPrice = 60000m,
            StopLoss = 45000m,
            Note = "No permission"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCryptoPosition_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Crypto Create Portfolio");

        // Act
        var response = await Client.PostAsJsonAsync("/api/crypto-positions", new
        {
            PortfolioId = portfolioId,
            Symbol = "ETHUSDT",
            Quantity = 2m,
            Price = 3000m,
            Fee = 1m,
            TransactedAt = DateTime.UtcNow,
            TargetPrice = 3600m,
            StopLoss = 2500m,
            Note = "Created by integration test"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await ReadResponseAsync<Guid>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateCryptoPosition_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Crypto Invalid Portfolio");

        // Act
        var response = await Client.PostAsJsonAsync("/api/crypto-positions", new
        {
            PortfolioId = portfolioId,
            Symbol = new string('X', 33),
            Quantity = 0m,
            Price = -1m,
            Fee = -1m,
            TransactedAt = DateTime.UtcNow,
            TargetPrice = -1m,
            StopLoss = -1m,
            Note = new string('n', 501)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ReadResponseAsync<object>(response);
        envelope.Success.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Details.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateCryptoPosition_WithUnknownPortfolio_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/crypto-positions", new
        {
            PortfolioId = Guid.NewGuid(),
            Symbol = "SOLUSDT",
            Quantity = 3m,
            Price = 150m,
            Fee = 1m,
            TransactedAt = DateTime.UtcNow,
            TargetPrice = 200m,
            StopLoss = 100m,
            Note = "Unknown portfolio"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Update

    [Fact]
    public async Task UpdateCryptoPositionTargets_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PutAsJsonAsync($"/api/crypto-positions/{Guid.NewGuid()}/targets", new
        {
            TargetPrice = 65000m,
            StopLoss = 43000m,
            Note = "No auth"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCryptoPositionTargets_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"crypto-update-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/crypto-positions/{Guid.NewGuid()}/targets", new
        {
            TargetPrice = 65000m,
            StopLoss = 43000m,
            Note = "No permission"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCryptoPositionTargets_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Crypto Update Portfolio");
        var positionId = await CreateCryptoPositionAsync(portfolioId, "BNBUSDT", quantity: 4m, price: 550m, targetPrice: 600m, stopLoss: 450m, note: "Before update");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/crypto-positions/{positionId}/targets", new
        {
            TargetPrice = 700m,
            StopLoss = 500m,
            Note = "After update"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await Client.GetAsync($"/api/crypto-positions?portfolioId={portfolioId}&pageNumber=1&pageSize=20");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listEnvelope = await ReadPagedResponseAsync<CryptoPositionListItemDto>(listResponse);
        var updated = listEnvelope.Data!.Single(item => item.Id == positionId);
        updated.TargetPrice.Should().Be(700m);
        updated.StopLoss.Should().Be(500m);
        updated.Note.Should().Be("After update");
    }

    [Fact]
    public async Task UpdateCryptoPositionTargets_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/crypto-positions/{Guid.NewGuid()}/targets", new
        {
            TargetPrice = 700m,
            StopLoss = 500m,
            Note = "Unknown"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteCryptoPosition_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.DeleteAsync($"/api/crypto-positions/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteCryptoPosition_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"crypto-delete-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.DeleteAsync($"/api/crypto-positions/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteCryptoPosition_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Crypto Delete Portfolio");
        var positionId = await CreateCryptoPositionAsync(portfolioId, "ADAUSDT", quantity: 100m, price: 1m, targetPrice: 1.2m, stopLoss: 0.8m, note: "Delete case");

        // Act
        var response = await Client.DeleteAsync($"/api/crypto-positions/{positionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await Client.GetAsync($"/api/crypto-positions?portfolioId={portfolioId}&pageNumber=1&pageSize=20");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listEnvelope = await ReadPagedResponseAsync<CryptoPositionListItemDto>(listResponse);
        listEnvelope.Data.Should().NotBeNull();
        listEnvelope.Data!.Any(item => item.Id == positionId).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCryptoPosition_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/crypto-positions/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddCryptoTransaction_WhenBusinessRuleRejectsRequest_ShouldReturnConflict()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Crypto Transactions Portfolio");
        var positionId = await CreateCryptoPositionAsync(portfolioId, "BTCUSDT", quantity: 1m, price: 50000m, targetPrice: 60000m, stopLoss: 45000m, note: "Transaction base");

        // Act
        var addResponse = await Client.PostAsJsonAsync($"/api/crypto-positions/{positionId}/transactions", new
        {
            TransactionType = TransactionType.Sell,
            Quantity = 0.25m,
            Price = 52000m,
            Fee = 1m,
            TransactedAt = DateTime.UtcNow,
            Note = $"Crypto tx {Guid.NewGuid():N}",
            ExternalRef = $"EXT-{Guid.NewGuid():N}"
        });

        // Assert
        addResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var listResponse = await Client.GetAsync($"/api/crypto-positions/{positionId}/transactions?pageNumber=1&pageSize=20");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await ReadPagedResponseAsync<PositionTransactionDto>(listResponse);
        list.Data.Should().NotBeNullOrEmpty();
    }

    #endregion

    private async Task<Guid> CreatePortfolioAsync(string name)
    {
        var createResponse = await Client.PostAsJsonAsync("/api/portfolios", new
        {
            Name = name,
            PortfolioType = PortfolioType.Trading,
            Description = "Created by integration test"
        });

        createResponse.EnsureSuccessStatusCode();
        var envelope = await ReadResponseAsync<Guid>(createResponse);
        return envelope.Data;
    }

    private async Task<Guid> CreateCryptoPositionAsync(
        Guid portfolioId,
        string symbol,
        decimal quantity,
        decimal price,
        decimal? targetPrice,
        decimal? stopLoss,
        string? note)
    {
        var response = await Client.PostAsJsonAsync("/api/crypto-positions", new
        {
            PortfolioId = portfolioId,
            Symbol = symbol,
            Quantity = quantity,
            Price = price,
            Fee = 1m,
            TransactedAt = DateTime.UtcNow,
            TargetPrice = targetPrice,
            StopLoss = stopLoss,
            Note = note
        });

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Create crypto position failed with {(int)response.StatusCode}: {body}");
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
