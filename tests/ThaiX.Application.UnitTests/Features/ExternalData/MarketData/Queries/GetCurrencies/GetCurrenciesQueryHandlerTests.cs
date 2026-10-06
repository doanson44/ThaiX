using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCurrencies;

namespace ThaiX.Application.UnitTests.Features.ExternalData.MarketData.Queries.GetCurrencies;

public sealed class GetCurrenciesQueryHandlerTests
{
    private readonly Mock<IExternalDataService> _externalDataServiceMock = new();
    private readonly GetCurrenciesQueryHandler _sut;

    public GetCurrenciesQueryHandlerTests()
    {
        _sut = new GetCurrenciesQueryHandler(_externalDataServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenExternalDataIsNull_ShouldReturnFailedResponse()
    {
        // Arrange
        _externalDataServiceMock
            .Setup(x => x.GetCurrenciesAsync<CurrenciesResponse>(It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrenciesResponse?)null);

        // Act
        var result = await _sut.Handle(new GetCurrenciesQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenExternalDataIsValid_ShouldReturnPayload()
    {
        // Arrange
        var payload = new CurrenciesResponse
        {
            Success = true,
            Message = "ok",
            Data =
            [
                new CurrencyDto
                {
                    ProductName = "USD/VND",
                    CurrentPrice = 25500m,
                    OtherPrice = 25520m,
                    PrevPrice = 25400m,
                    Change24H = 100m,
                    Change7D = 150m,
                    UpdateDate = "now"
                }
            ]
        };

        _externalDataServiceMock
            .Setup(x => x.GetCurrenciesAsync<CurrenciesResponse>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(payload);

        // Act
        var result = await _sut.Handle(new GetCurrenciesQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data[0].ProductName.Should().Be("USD/VND");
    }
}
