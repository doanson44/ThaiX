using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractTickerBySymbol;

namespace ThaiX.Application.UnitTests.Features.ExternalData.MarketData.Queries.GetMexcContractTickerBySymbol;

public sealed class GetMexcContractTickerBySymbolQueryHandlerTests
{
    private readonly Mock<IExternalDataService> _externalDataServiceMock = new();
    private readonly GetMexcContractTickerBySymbolQueryHandler _sut;

    public GetMexcContractTickerBySymbolQueryHandlerTests()
    {
        _sut = new GetMexcContractTickerBySymbolQueryHandler(_externalDataServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithInvalidSymbol_ShouldReturnFailedResponseWithoutCallingService()
    {
        // Arrange
        var query = new GetMexcContractTickerBySymbolQuery { Symbol = "INV@LID" };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid symbol");
        _externalDataServiceMock.Verify(
            x => x.GetMexcContractTickerBySymbolAsync<object>(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidSymbolAndNullExternalData_ShouldReturnFailedResponse()
    {
        // Arrange
        var query = new GetMexcContractTickerBySymbolQuery { Symbol = "BTC_USDT" };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Symbol.Should().Be("BTC_USDT");
        result.Message.Should().Be("Failed to retrieve MEXC contract ticker from external API.");
    }
}
