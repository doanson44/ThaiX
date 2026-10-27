using System.Net;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class MexcFundingRateHistoryEndpointsTests : IntegrationTestBase
{
    public MexcFundingRateHistoryEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetFundingRateHistory_WhenAnonymous_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/external-data/mexc/funding-rate/history/BTC_USDT");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetFundingRateHistory_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"mexc-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/external-data/mexc/funding-rate/history/BTC_USDT");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetFundingRateHistory_WithInvalidSymbol_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/external-data/mexc/funding-rate/history/BTC%40USDT");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
