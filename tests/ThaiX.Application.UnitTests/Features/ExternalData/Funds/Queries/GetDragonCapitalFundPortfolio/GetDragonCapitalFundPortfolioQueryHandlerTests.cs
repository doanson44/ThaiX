using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.Funds.Queries.GetDragonCapitalFundPortfolio;

namespace ThaiX.Application.UnitTests.Features.ExternalData.Funds.Queries.GetDragonCapitalFundPortfolio;

public sealed class GetDragonCapitalFundPortfolioQueryHandlerTests
{
    private readonly Mock<IExternalDataService> _externalDataServiceMock = new();
    private readonly GetDragonCapitalFundPortfolioQueryHandler _sut;

    public GetDragonCapitalFundPortfolioQueryHandlerTests()
    {
        _sut = new GetDragonCapitalFundPortfolioQueryHandler(_externalDataServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithInvalidFundCode_ShouldReturnFailedResponseWithoutCallingService()
    {
        // Arrange
        var query = new GetDragonCapitalFundPortfolioQuery { FundCode = "invalid" };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid fund code");
        _externalDataServiceMock.Verify(
            x => x.GetDragonCapitalFundPortfolioAsync<object>(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidFundCodeAndNullExternalData_ShouldReturnFailedResponse()
    {
        // Arrange
        var query = new GetDragonCapitalFundPortfolioQuery { FundCode = "VF4" };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Failed to retrieve Dragon Capital fund portfolio from external API.");
    }
}
