using System.Net;
using System.Net.Http.Json;
using ThaiX.Application.Features.ExpenseTracker;
using ThaiX.Application.Features.ExpenseTracker.Models;
using ThaiX.Client.Models.ExpenseTracker;
using ThaiX.Domain.Aggregates.ExpenseTracker;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class ExpenseTrackerEndpointsTests : IntegrationTestBase
{
    public ExpenseTrackerEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    private static async Task<PagedApiResponse<T>> ReadPagedResponseAsync<T>(HttpResponseMessage response)
    {
        var envelope = await response.Content.ReadFromJsonAsync<PagedApiResponse<T>>(JsonOptions);
        return envelope ?? throw new InvalidOperationException("Unable to deserialize paged API response.");
    }

    [Fact]
    public async Task GetWallets_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/expense-tracker/wallets");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWallets_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"wallet-read-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", [Permissions.PriceAlertRead]);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/expense-tracker/wallets");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateWallet_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/expense-tracker/wallets", new
        {
            Name = $"Wallet-{Guid.NewGuid():N}",
            WalletType = WalletType.Cash,
            Currency = "USD",
            InitialBalance = 100m
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await ReadResponseAsync<Guid>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateWallet_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/expense-tracker/wallets", new
        {
            Name = string.Empty,
            WalletType = 0,
            Currency = string.Empty,
            InitialBalance = 0m
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WalletCrud_HappyPath_ShouldCreateUpdateListAndDelete()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var createResponse = await Client.PostAsJsonAsync("/api/expense-tracker/wallets", new
        {
            Name = $"Wallet-{Guid.NewGuid():N}",
            WalletType = WalletType.Bank,
            Currency = "USD",
            InitialBalance = 500m
        });
        var walletId = (await ReadResponseAsync<Guid>(createResponse)).Data;

        // Act
        var updateResponse = await Client.PutAsJsonAsync($"/api/expense-tracker/wallets/{walletId}", new
        {
            Id = walletId,
            Name = "Updated Wallet",
            WalletType = WalletType.EWallet,
            Currency = "VND"
        });

        var listResponse = await Client.GetAsync("/api/expense-tracker/wallets?pageNumber=1&pageSize=20&searchTerm=Updated");

        var deleteResponse = await Client.DeleteAsync($"/api/expense-tracker/wallets/{walletId}");

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listEnvelope = await ReadPagedResponseAsync<Application.Features.ExpenseTracker.Models.ExpenseWalletDto>(listResponse);
        listEnvelope.Data.Should().Contain(x => x.Id == walletId);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateWallet_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var id = Guid.NewGuid();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/expense-tracker/wallets/{id}", new
        {
            Id = id,
            Name = "Unknown",
            WalletType = WalletType.Cash,
            Currency = "USD"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
