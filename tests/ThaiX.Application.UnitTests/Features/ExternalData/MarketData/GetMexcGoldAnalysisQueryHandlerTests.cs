using System.Text.Json;
using FluentAssertions;
using Moq;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcGoldAnalysis;
using ThaiX.Application.Trading;

namespace ThaiX.Application.UnitTests.Features.ExternalData.MarketData;

public sealed class GetMexcGoldAnalysisQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenApiReturns500DailyCandles_ComputesTechnicalAnalysis()
    {
        // Arrange
        var rows = CreateCandles(500);
        var externalDataService = new Mock<IExternalDataService>();
        externalDataService
            .Setup(service => service.GetMexcSpotKlinesAsync<List<List<JsonElement>>>(
                "GOLD(XAUT)USDT", "1d", 500, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rows);

        var indicatorService = new Mock<IIndicatorService>();
        indicatorService.Setup(service => service.GetMinimumBars(It.IsAny<KlineExecutionContext>())).Returns(60);
        indicatorService.Setup(service => service.Compute(
                It.IsAny<KlineExecutionContext>(),
                It.Is<IReadOnlyList<Kline>>(klines => klines.Count == 500)))
            .Returns(new IndicatorResult
            {
                Ema9 = 2400m,
                Ema21 = 2399m,
                Ema50 = 2398m,
                Rsi14 = 61m,
                Atr14 = 12m,
                AverageVolume20 = 10m,
                LatestClose = 2400m,
                HighestHigh20 = 2410m,
                LowestLow20 = 2370m
            });

        var executionEngine = new Mock<IKlineExecutionEngine>();
        executionEngine.Setup(engine => engine.Execute(
                It.IsAny<KlineExecutionContext>(),
                It.Is<IReadOnlyList<Kline>>(klines => klines.Count == 500)))
            .Returns(new KlineExecutionResult
            {
                Trend = TrendDirection.Up,
                Momentum = MomentumStrength.Strong,
                Setup = SetupType.Pullback,
                Signal = SignalType.Long,
                Confidence = 0.85m,
                Trade = new TradePlan
                {
                    Side = SignalType.Long,
                    Entry = 2400m,
                    StopLoss = 2382m,
                    TakeProfit1 = 2423m,
                    TakeProfit2 = 2439m
                }
            });

        var handler = new GetMexcGoldAnalysisQueryHandler(
            externalDataService.Object,
            indicatorService.Object,
            executionEngine.Object);

        // Act
        var result = await handler.Handle(new GetMexcGoldAnalysisQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Symbol.Should().Be("GOLD(XAUT)USDT");
        result.HistoryCount.Should().Be(500);
        result.History.Should().HaveCount(500);
        result.Price.Should().Be(2400m);
        result.PreviousClose.Should().Be(2399m);
        result.Change.Should().Be(1m);
        result.ChangePct.Should().BeApproximately(1m / 2399m * 100m, 0.0001m);
        result.Ema9.Should().Be(2400m);
        result.Rsi14.Should().Be(61m);
        result.Trend.Should().Be("Up");
        result.Signal.Should().Be("Long");
        result.Entry.Should().Be(2400m);
        result.StopLoss.Should().Be(2382m);
        indicatorService.Verify(service => service.GetMinimumBars(It.Is<KlineExecutionContext>(context =>
            context.MarketType == MarketType.CryptoSpot && context.Timeframe == "1d")), Times.Once);
        externalDataService.Verify(service => service.GetMexcSpotKlinesAsync<List<List<JsonElement>>>(
            "GOLD(XAUT)USDT", "1d", 500, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenHistoryIsBelowIndicatorMinimum_ReturnsInsufficientHistory()
    {
        // Arrange
        var externalDataService = new Mock<IExternalDataService>();
        externalDataService
            .Setup(service => service.GetMexcSpotKlinesAsync<List<List<JsonElement>>>(
                "GOLD(XAUT)USDT", "1d", 500, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCandles(20));

        var indicatorService = new Mock<IIndicatorService>();
        indicatorService.Setup(service => service.GetMinimumBars(It.IsAny<KlineExecutionContext>())).Returns(60);
        var executionEngine = new Mock<IKlineExecutionEngine>();
        var handler = new GetMexcGoldAnalysisQueryHandler(
            externalDataService.Object,
            indicatorService.Object,
            executionEngine.Object);

        // Act
        var result = await handler.Handle(new GetMexcGoldAnalysisQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.HistoryCount.Should().Be(20);
        result.History.Should().HaveCount(20);
        result.Message.Should().Contain("Insufficient daily gold candle history");
        executionEngine.Verify(engine => engine.Execute(
            It.IsAny<KlineExecutionContext>(), It.IsAny<IReadOnlyList<Kline>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenApiReturnsNoCandles_ReturnsFailureResponse()
    {
        // Arrange
        var externalDataService = new Mock<IExternalDataService>();
        externalDataService
            .Setup(service => service.GetMexcSpotKlinesAsync<List<List<JsonElement>>>(
                "GOLD(XAUT)USDT", "1d", 500, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetMexcGoldAnalysisQueryHandler(
            externalDataService.Object,
            Mock.Of<IIndicatorService>(),
            Mock.Of<IKlineExecutionEngine>());

        // Act
        var result = await handler.Handle(new GetMexcGoldAnalysisQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("No valid daily gold candles");
    }

    private static List<List<JsonElement>> CreateCandles(int count)
    {
        var rows = new List<List<JsonElement>>(count);
        var startTime = DateTimeOffset.FromUnixTimeMilliseconds(1_700_000_000_000);
        for (var i = 0; i < count; i++)
        {
            var open = 1900m + i;
            var close = open + 1m;
            var high = close + 10m;
            var low = open - 10m;
            var time = startTime.AddDays(i).ToUnixTimeMilliseconds();
            var closeTime = startTime.AddDays(i + 1).AddMilliseconds(-1).ToUnixTimeMilliseconds();
            var json = $"[{time},\"{open.ToString(System.Globalization.CultureInfo.InvariantCulture)}\",\"{high.ToString(System.Globalization.CultureInfo.InvariantCulture)}\",\"{low.ToString(System.Globalization.CultureInfo.InvariantCulture)}\",\"{close.ToString(System.Globalization.CultureInfo.InvariantCulture)}\",\"10\",{closeTime},\"{(close * 10m).ToString(System.Globalization.CultureInfo.InvariantCulture)}\"]";
            using var document = JsonDocument.Parse(json);
            rows.Add(document.RootElement.EnumerateArray().Select(element => element.Clone()).ToList());
        }

        return rows;
    }
}
