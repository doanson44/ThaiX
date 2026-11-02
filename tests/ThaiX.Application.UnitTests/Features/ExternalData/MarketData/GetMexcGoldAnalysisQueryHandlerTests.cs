using System.Text.Json;
using FluentAssertions;
using Moq;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcGoldAnalysis;

namespace ThaiX.Application.UnitTests.Features.ExternalData.MarketData;

public sealed class GetMexcGoldAnalysisQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenApiReturnsDailyCandles_CalculatesVnDirectStylePriceChange()
    {
        using var document = JsonDocument.Parse(
            """
            [
                [1700000000000, "1900.00", "1950.00", "1880.00", "1925.00", "8.0", 1700086399999, "15300.00"],
                [1700086400000, "2000.00", "2050.00", "1980.00", "2025.00", "10.0", 1700172799999, "20250.00"]
            ]
            """);
        var rows = document.RootElement.EnumerateArray()
            .Select(row => row.EnumerateArray().Select(value => value.Clone()).ToList())
            .ToList();

        var externalDataService = new Mock<IExternalDataService>();
        externalDataService
            .Setup(service => service.GetMexcSpotKlinesAsync<List<List<JsonElement>>>(
                "GOLD(XAUT)USDT",
                "1d",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rows);

        var handler = new GetMexcGoldAnalysisQueryHandler(externalDataService.Object);

        var result = await handler.Handle(new GetMexcGoldAnalysisQuery(), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Symbol.Should().Be("GOLD(XAUT)USDT");
        result.Price.Should().Be(2025m);
        result.BopPrice.Should().Be(2000m);
        result.Change.Should().Be(25m);
        result.ChangePct.Should().Be(1.25m);
        result.High.Should().Be(2050m);
        result.Low.Should().Be(1980m);
        result.Volume.Should().Be(10m);
        result.QuoteVolume.Should().Be(20250m);
        result.LastUpdated.Should().Be("2023-11-15");
    }

    [Fact]
    public async Task Handle_WhenApiReturnsNoCandles_ReturnsFailureResponse()
    {
        var externalDataService = new Mock<IExternalDataService>();
        externalDataService
            .Setup(service => service.GetMexcSpotKlinesAsync<List<List<JsonElement>>>(
                "GOLD(XAUT)USDT",
                "1d",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetMexcGoldAnalysisQueryHandler(externalDataService.Object);

        var result = await handler.Handle(new GetMexcGoldAnalysisQuery(), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("No valid daily gold candles");
    }
}
