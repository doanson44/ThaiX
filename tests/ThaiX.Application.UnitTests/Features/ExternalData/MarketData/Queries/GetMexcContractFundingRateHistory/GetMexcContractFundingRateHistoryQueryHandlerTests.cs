using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractFundingRateHistory;

namespace ThaiX.Application.UnitTests.Features.ExternalData.MarketData.Queries.GetMexcContractFundingRateHistory;

public sealed class GetMexcContractFundingRateHistoryQueryHandlerTests
{
    private readonly Mock<IExternalDataService> _externalDataServiceMock = new();
    private readonly GetMexcContractFundingRateHistoryQueryHandler _sut;

    public GetMexcContractFundingRateHistoryQueryHandlerTests()
    {
        _sut = new GetMexcContractFundingRateHistoryQueryHandler(_externalDataServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithInvalidSymbol_ShouldReturnFailedResponseWithoutCallingService()
    {
        // Arrange
        var query = new GetMexcContractFundingRateHistoryQuery { Symbol = "BTC@USDT" };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid symbol");
        _externalDataServiceMock.Verify(
            x => x.GetMexcContractFundingRateHistoryAsync<object>(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidPageSize_ShouldReturnFailedResponseWithoutCallingService()
    {
        // Arrange
        var query = new GetMexcContractFundingRateHistoryQuery { Symbol = "BTC_USDT", PageSize = 1001 };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("PageSize");
        _externalDataServiceMock.Verify(
            x => x.GetMexcContractFundingRateHistoryAsync<object>(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullExternalResponse_ShouldReturnFailedResponse()
    {
        // Arrange
        var query = new GetMexcContractFundingRateHistoryQuery { Symbol = "BTC_USDT" };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeEmpty();
        result.Message.Should().Contain("Failed to retrieve");
    }
}
