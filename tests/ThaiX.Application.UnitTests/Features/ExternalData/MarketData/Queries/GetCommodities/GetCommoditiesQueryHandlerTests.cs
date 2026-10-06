using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCommodities;

namespace ThaiX.Application.UnitTests.Features.ExternalData.MarketData.Queries.GetCommodities;

public sealed class GetCommoditiesQueryHandlerTests
{
    private readonly Mock<IExternalDataService> _externalDataServiceMock = new();
    private readonly GetCommoditiesQueryHandler _sut;

    public GetCommoditiesQueryHandlerTests()
    {
        _sut = new GetCommoditiesQueryHandler(_externalDataServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenExternalDataIsNull_ShouldReturnFailedResponse()
    {
        // Arrange
        _externalDataServiceMock
            .Setup(x => x.GetCommoditiesAsync<CommoditiesResponse>(It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommoditiesResponse?)null);

        // Act
        var result = await _sut.Handle(new GetCommoditiesQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenExternalDataIsValid_ShouldReturnPayload()
    {
        // Arrange
        var payload = new CommoditiesResponse
        {
            Success = true,
            Message = "ok",
            Data =
            [
                new CommodityDto
                {
                    Goods = "XAUUSD",
                    Last = 2500m,
                    High = 2510m,
                    Low = 2490m,
                    Change = 10m,
                    ChangePercent = 0.4m,
                    LastUpdate = "now"
                }
            ]
        };

        _externalDataServiceMock
            .Setup(x => x.GetCommoditiesAsync<CommoditiesResponse>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(payload);

        // Act
        var result = await _sut.Handle(new GetCommoditiesQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data[0].Goods.Should().Be("XAUUSD");
    }
}
