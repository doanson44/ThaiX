using System.Net;
using System.Net.Http.Json;
using ThaiX.Application.Features.AssetPositions.Queries.GetPositionTransactions;
using ThaiX.Application.Features.AssetPositions.Queries.GetStockPositions;
using ThaiX.Domain.Aggregates.AssetPositions;
using ThaiX.Domain.Aggregates.Portfolios;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class StockPositionEndpointsTests : IntegrationTestBase
{
    public StockPositionEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    #region List

    [Fact]
    public async Task GetStockPositions_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/stock-positions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetStockPositions_WithoutPositionReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"stock-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/stock-positions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetStockPositions_WithPositionReadPermission_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Stock List Portfolio");
        var createdId = await CreateStockPositionAsync(portfolioId, "VNM", StockExchange.HOSE, quantity: 10m, price: 100m, targetPrice: 120m, stopLoss: 90m, note: "List case");

        // Act
        var response = await Client.GetAsync($"/api/stock-positions?portfolioId={portfolioId}&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ReadPagedResponseAsync<StockPositionListItemDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNullOrEmpty();
        var created = envelope.Data!.Single(item => item.Id == createdId);
        created.Symbol.Should().Be("VNM");
        created.Exchange.Should().Be(StockExchange.HOSE);
    }

    #endregion

    #region Create

    [Fact]
    public async Task CreateStockPosition_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync("/api/stock-positions", new
        {
            PortfolioId = Guid.NewGuid(),
            Symbol = "VNM",
            Exchange = StockExchange.HOSE,
            Quantity = 10m,
            Price = 100m,
            Fee = 1m,
            TransactedAt = DateTime.UtcNow,
            TargetPrice = 120m,
            StopLoss = 90m,
            Note = "No auth"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateStockPosition_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"stock-create-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/stock-positions", new
        {
            PortfolioId = Guid.NewGuid(),
            Symbol = "VNM",
            Exchange = StockExchange.HOSE,
            Quantity = 10m,
            Price = 100m,
            Fee = 1m,
            TransactedAt = DateTime.UtcNow,
            TargetPrice = 120m,
            StopLoss = 90m,
            Note = "No permission"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateStockPosition_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Stock Create Portfolio");

        // Act
        var response = await Client.PostAsJsonAsync("/api/stock-positions", new
        {
            PortfolioId = portfolioId,
            Symbol = "FPT",
            Exchange = StockExchange.HOSE,
            Quantity = 20m,
            Price = 110m,
            Fee = 1m,
            TransactedAt = DateTime.UtcNow,
            TargetPrice = 130m,
            StopLoss = 95m,
            Note = "Created by integration test"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await ReadResponseAsync<Guid>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateStockPosition_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Stock Invalid Portfolio");

        // Act
        var response = await Client.PostAsJsonAsync("/api/stock-positions", new
        {
            PortfolioId = portfolioId,
            Symbol = new string('X', 17),
            Exchange = StockExchange.HOSE,
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
    public async Task CreateStockPosition_WithUnknownPortfolio_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/stock-positions", new
        {
            PortfolioId = Guid.NewGuid(),
            Symbol = "VHM",
            Exchange = StockExchange.HOSE,
            Quantity = 5m,
            Price = 80m,
            Fee = 1m,
            TransactedAt = DateTime.UtcNow,
            TargetPrice = 90m,
            StopLoss = 70m,
            Note = "Unknown portfolio"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Update

    [Fact]
    public async Task UpdateStockPositionTargets_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PutAsJsonAsync($"/api/stock-positions/{Guid.NewGuid()}/targets", new
        {
            TargetPrice = 150m,
            StopLoss = 90m,
            Note = "No auth"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateStockPositionTargets_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"stock-update-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/stock-positions/{Guid.NewGuid()}/targets", new
        {
            TargetPrice = 150m,
            StopLoss = 90m,
            Note = "No permission"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateStockPositionTargets_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Stock Update Portfolio");
        var positionId = await CreateStockPositionAsync(portfolioId, "HPG", StockExchange.HOSE, quantity: 12m, price: 40m, targetPrice: 48m, stopLoss: 35m, note: "Before update");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/stock-positions/{positionId}/targets", new
        {
            TargetPrice = 55m,
            StopLoss = 38m,
            Note = "After update"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await Client.GetAsync($"/api/stock-positions?portfolioId={portfolioId}&pageNumber=1&pageSize=20");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listEnvelope = await ReadPagedResponseAsync<StockPositionListItemDto>(listResponse);
        var updated = listEnvelope.Data!.Single(item => item.Id == positionId);
        updated.TargetPrice.Should().Be(55m);
        updated.StopLoss.Should().Be(38m);
        updated.Note.Should().Be("After update");
    }

    [Fact]
    public async Task UpdateStockPositionTargets_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/stock-positions/{Guid.NewGuid()}/targets", new
        {
            TargetPrice = 55m,
            StopLoss = 38m,
            Note = "Unknown"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteStockPosition_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.DeleteAsync($"/api/stock-positions/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteStockPosition_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"stock-delete-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.DeleteAsync($"/api/stock-positions/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteStockPosition_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Stock Delete Portfolio");
        var positionId = await CreateStockPositionAsync(portfolioId, "ACB", StockExchange.HNX, quantity: 5m, price: 30m, targetPrice: 40m, stopLoss: 25m, note: "Delete case");

        // Act
        var response = await Client.DeleteAsync($"/api/stock-positions/{positionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await Client.GetAsync($"/api/stock-positions?portfolioId={portfolioId}&pageNumber=1&pageSize=20");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listEnvelope = await ReadPagedResponseAsync<StockPositionListItemDto>(listResponse);
        listEnvelope.Data.Should().NotBeNull();
        listEnvelope.Data!.Any(item => item.Id == positionId).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteStockPosition_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/stock-positions/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddStockTransaction_WhenBusinessRuleRejectsRequest_ShouldReturnConflict()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Stock Transactions Portfolio");
        var positionId = await CreateStockPositionAsync(portfolioId, "VNM", StockExchange.HOSE, quantity: 10m, price: 100m, targetPrice: 120m, stopLoss: 90m, note: "Transaction base");

        // Act
        var addResponse = await Client.PostAsJsonAsync($"/api/stock-positions/{positionId}/transactions", new
        {
            TransactionType = TransactionType.Sell,
            Quantity = 1m,
            Price = 105m,
            Fee = 1m,
            TransactedAt = DateTime.UtcNow,
            Note = $"Stock tx {Guid.NewGuid():N}",
            ExternalRef = $"EXT-{Guid.NewGuid():N}"
        });

        // Assert
        addResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var listResponse = await Client.GetAsync($"/api/stock-positions/{positionId}/transactions?pageNumber=1&pageSize=20");
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

    private async Task<Guid> CreateStockPositionAsync(
        Guid portfolioId,
        string symbol,
        StockExchange exchange,
        decimal quantity,
        decimal price,
        decimal? targetPrice,
        decimal? stopLoss,
        string? note)
    {
        var response = await Client.PostAsJsonAsync("/api/stock-positions", new
        {
            PortfolioId = portfolioId,
            Symbol = symbol,
            Exchange = exchange,
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
            throw new InvalidOperationException($"Create stock position failed with {(int)response.StatusCode}: {body}");
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
