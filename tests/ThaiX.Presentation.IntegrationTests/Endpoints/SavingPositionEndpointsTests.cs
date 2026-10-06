using System.Net;
using System.Net.Http.Json;
using ThaiX.Application.Features.AssetPositions.Queries.GetSavingPositions;
using ThaiX.Domain.Aggregates.AssetPositions;
using ThaiX.Domain.Aggregates.Portfolios;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class SavingPositionEndpointsTests : IntegrationTestBase
{
    public SavingPositionEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    #region List

    [Fact]
    public async Task GetSavingPositions_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/saving-positions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSavingPositions_WithoutPositionReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"saving-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/saving-positions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSavingPositions_WithPositionReadPermission_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Saving List Portfolio", PortfolioType.Savings);
        var positionId = await CreateSavingPositionAsync(portfolioId, "Bank Alpha", 100000m, 6.5m, InterestType.Compound, DateOnly.FromDateTime(DateTime.UtcNow.Date), DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(12)), "List happy path");

        // Act
        var response = await Client.GetAsync($"/api/saving-positions?portfolioId={portfolioId}&status=Active&pageNumber=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ReadPagedResponseAsync<SavingPositionListItemDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNullOrEmpty();
        var created = envelope.Data!.Single(item => item.Id == positionId);
        created.BankName.Should().Be("Bank Alpha");
        created.Status.Should().Be(SavingStatus.Active);
    }

    [Fact]
    public async Task GetSavingPositions_WithUnknownPortfolio_ShouldReturnEmptyOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var unknownPortfolioId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/saving-positions?portfolioId={unknownPortfolioId}&pageNumber=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ReadPagedResponseAsync<SavingPositionListItemDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSavingPositions_WithStatusFilter_ShouldReturnFilteredItems()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Saving Filter Portfolio", PortfolioType.Savings);
        var positionId = await CreateSavingPositionAsync(portfolioId, "Bank Beta", 150000m, 7m, InterestType.Simple, DateOnly.FromDateTime(DateTime.UtcNow.Date), DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(6)), null);

        // Act
        var response = await Client.GetAsync($"/api/saving-positions?portfolioId={portfolioId}&status=Active&pageNumber=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ReadPagedResponseAsync<SavingPositionListItemDto>(response);
        envelope.Data.Should().NotBeNullOrEmpty();
        envelope.Data!.Single(item => item.Id == positionId).Status.Should().Be(SavingStatus.Active);
    }

    #endregion

    #region Create

    [Fact]
    public async Task CreateSavingPosition_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync("/api/saving-positions", new
        {
            PortfolioId = Guid.NewGuid(),
            BankName = "Bank",
            AccountNumber = "ACC-001",
            PrincipalAmount = 1000m,
            InterestRate = 5m,
            InterestType = InterestType.Simple,
            DepositDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(6)),
            Note = "No auth"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateSavingPosition_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"saving-create-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/saving-positions", new
        {
            PortfolioId = Guid.NewGuid(),
            BankName = "Bank",
            AccountNumber = "ACC-001",
            PrincipalAmount = 1000m,
            InterestRate = 5m,
            InterestType = InterestType.Simple,
            DepositDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(6)),
            Note = "No permission"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateSavingPosition_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Saving Create Portfolio", PortfolioType.Savings);
        var request = new
        {
            PortfolioId = portfolioId,
            BankName = "Bank Gamma",
            AccountNumber = "ACC-123",
            PrincipalAmount = 500000m,
            InterestRate = 6.5m,
            InterestType = InterestType.Compound,
            DepositDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(12)),
            Note = "Created by integration test"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/saving-positions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var envelope = await ReadResponseAsync<Guid>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateSavingPosition_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Saving Invalid Portfolio", PortfolioType.Savings);

        // Act
        var response = await Client.PostAsJsonAsync("/api/saving-positions", new
        {
            PortfolioId = portfolioId,
            BankName = string.Empty,
            AccountNumber = new string('x', 51),
            PrincipalAmount = 0m,
            InterestRate = -1m,
            InterestType = 0,
            DepositDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)),
            Note = new string('y', 501)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ReadResponseAsync<object>(response);
        envelope.Success.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Details.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateSavingPosition_WithUnknownPortfolio_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/saving-positions", new
        {
            PortfolioId = Guid.NewGuid(),
            BankName = "Bank",
            AccountNumber = "ACC-001",
            PrincipalAmount = 1000m,
            InterestRate = 5m,
            InterestType = InterestType.Simple,
            DepositDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(6)),
            Note = "Unknown portfolio"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Update

    [Fact]
    public async Task UpdateSavingPosition_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PutAsJsonAsync($"/api/saving-positions/{Guid.NewGuid()}", new
        {
            BankName = "Updated Bank",
            AccountNumber = "ACC-999",
            PrincipalAmount = 2000m,
            InterestRate = 6m,
            InterestType = InterestType.Simple,
            DepositDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(6)),
            Note = "No auth"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateSavingPosition_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"saving-update-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/saving-positions/{Guid.NewGuid()}", new
        {
            BankName = "Updated Bank",
            AccountNumber = "ACC-999",
            PrincipalAmount = 2000m,
            InterestRate = 6m,
            InterestType = InterestType.Simple,
            DepositDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(6)),
            Note = "No permission"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateSavingPosition_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Saving Update Portfolio", PortfolioType.Savings);
        var positionId = await CreateSavingPositionAsync(portfolioId, "Bank Delta", 100000m, 5m, InterestType.Simple, DateOnly.FromDateTime(DateTime.UtcNow.Date), DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(6)), "Before update");
        var request = new
        {
            BankName = "Bank Delta Updated",
            AccountNumber = "ACC-456",
            PrincipalAmount = 150000m,
            InterestRate = 6.25m,
            InterestType = InterestType.Compound,
            DepositDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(9)),
            Note = "After update"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/saving-positions/{positionId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await Client.GetAsync($"/api/saving-positions?portfolioId={portfolioId}&status=Active&pageNumber=1&pageSize=20");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listEnvelope = await ReadPagedResponseAsync<SavingPositionListItemDto>(listResponse);
        var updated = listEnvelope.Data!.Single(item => item.Id == positionId);
        updated.BankName.Should().Be("Bank Delta Updated");
        updated.PrincipalAmount.Should().Be(150000m);
        updated.InterestRate.Should().Be(6.25m);
        updated.InterestType.Should().Be(InterestType.Compound);
        updated.Note.Should().Be("After update");
    }

    [Fact]
    public async Task UpdateSavingPosition_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Saving Update Invalid Portfolio", PortfolioType.Savings);
        var positionId = await CreateSavingPositionAsync(portfolioId, "Bank Epsilon", 100000m, 5m, InterestType.Simple, DateOnly.FromDateTime(DateTime.UtcNow.Date), DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(6)), null);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/saving-positions/{positionId}", new
        {
            BankName = string.Empty,
            AccountNumber = new string('x', 51),
            PrincipalAmount = 0m,
            InterestRate = -1m,
            InterestType = 0,
            DepositDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)),
            Note = new string('y', 501)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ReadResponseAsync<object>(response);
        envelope.Success.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Details.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateSavingPosition_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/saving-positions/{Guid.NewGuid()}", new
        {
            BankName = "Unknown",
            AccountNumber = "ACC-000",
            PrincipalAmount = 1000m,
            InterestRate = 5m,
            InterestType = InterestType.Simple,
            DepositDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(6)),
            Note = "Unknown id"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Withdraw

    [Fact]
    public async Task WithdrawSavingPosition_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync($"/api/saving-positions/{Guid.NewGuid()}/withdraw", new
        {
            WithdrawalDate = DateOnly.FromDateTime(DateTime.UtcNow.Date)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WithdrawSavingPosition_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"saving-withdraw-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync($"/api/saving-positions/{Guid.NewGuid()}/withdraw", new
        {
            WithdrawalDate = DateOnly.FromDateTime(DateTime.UtcNow.Date)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task WithdrawSavingPosition_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Saving Withdraw Portfolio", PortfolioType.Savings);
        var positionId = await CreateSavingPositionAsync(portfolioId, "Bank Zeta", 200000m, 6m, InterestType.Compound, DateOnly.FromDateTime(DateTime.UtcNow.Date), DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(6)), null);
        var withdrawalDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        // Act
        var response = await Client.PostAsJsonAsync($"/api/saving-positions/{positionId}/withdraw", new
        {
            WithdrawalDate = withdrawalDate
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await Client.GetAsync($"/api/saving-positions?portfolioId={portfolioId}&pageNumber=1&pageSize=20");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listEnvelope = await ReadPagedResponseAsync<SavingPositionListItemDto>(listResponse);
        listEnvelope.Data!.Single(item => item.Id == positionId).Status.Should().Be(SavingStatus.EarlyWithdrawn);
    }

    [Fact]
    public async Task WithdrawSavingPosition_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Saving Withdraw Invalid Portfolio", PortfolioType.Savings);
        var positionId = await CreateSavingPositionAsync(portfolioId, "Bank Eta", 100000m, 5m, InterestType.Simple, DateOnly.FromDateTime(DateTime.UtcNow.Date), DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(6)), null);

        // Act
        var response = await Client.PostAsJsonAsync($"/api/saving-positions/{positionId}/withdraw", new
        {
            WithdrawalDate = "not-a-date"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WithdrawSavingPosition_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync($"/api/saving-positions/{Guid.NewGuid()}/withdraw", new
        {
            WithdrawalDate = DateOnly.FromDateTime(DateTime.UtcNow.Date)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteSavingPosition_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.DeleteAsync($"/api/saving-positions/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteSavingPosition_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"saving-delete-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.DeleteAsync($"/api/saving-positions/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteSavingPosition_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Saving Delete Portfolio", PortfolioType.Savings);
        var positionId = await CreateSavingPositionAsync(portfolioId, "Bank Theta", 100000m, 5m, InterestType.Simple, DateOnly.FromDateTime(DateTime.UtcNow.Date), DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(6)), null);

        // Act
        var response = await Client.DeleteAsync($"/api/saving-positions/{positionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await Client.GetAsync($"/api/saving-positions?portfolioId={portfolioId}&pageNumber=1&pageSize=20");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listEnvelope = await ReadPagedResponseAsync<SavingPositionListItemDto>(listResponse);
        listEnvelope.Data.Should().NotBeNull();
        listEnvelope.Data!.Any(item => item.Id == positionId).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteSavingPosition_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/saving-positions/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    private async Task<Guid> CreatePortfolioAsync(string name, PortfolioType portfolioType)
    {
        var response = await Client.PostAsJsonAsync("/api/portfolios", new
        {
            Name = name,
            PortfolioType = portfolioType,
            Description = "Created by saving endpoint tests"
        });

        response.EnsureSuccessStatusCode();
        var envelope = await ReadResponseAsync<Guid>(response);
        return envelope.Data;
    }

    private async Task<Guid> CreateSavingPositionAsync(
        Guid portfolioId,
        string bankName,
        decimal principalAmount,
        decimal interestRate,
        InterestType interestType,
        DateOnly depositDate,
        DateOnly? maturityDate,
        string? note)
    {
        var response = await Client.PostAsJsonAsync("/api/saving-positions", new
        {
            PortfolioId = portfolioId,
            BankName = bankName,
            AccountNumber = "ACC-001",
            PrincipalAmount = principalAmount,
            InterestRate = interestRate,
            InterestType = interestType,
            DepositDate = depositDate,
            MaturityDate = maturityDate,
            Note = note
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
