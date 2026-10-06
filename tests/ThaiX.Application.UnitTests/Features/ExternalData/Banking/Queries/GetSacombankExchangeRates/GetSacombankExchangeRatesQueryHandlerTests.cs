using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.Banking.Queries.GetSacombankExchangeRates;

namespace ThaiX.Application.UnitTests.Features.ExternalData.Banking.Queries.GetSacombankExchangeRates;

public sealed class GetSacombankExchangeRatesQueryHandlerTests
{
    private readonly Mock<IExternalDataService> _externalDataServiceMock = new();
    private readonly GetSacombankExchangeRatesQueryHandler _sut;

    public GetSacombankExchangeRatesQueryHandlerTests()
    {
        _sut = new GetSacombankExchangeRatesQueryHandler(_externalDataServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenExternalDataIsNull_ShouldReturnFailedResponse()
    {
        // Arrange
        // No setup required because default mocked generic call returns null.

        // Act
        var result = await _sut.Handle(new GetSacombankExchangeRatesQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ExchangeRates.Should().BeEmpty();
        result.Message.Should().NotBeNullOrWhiteSpace();
    }
}
