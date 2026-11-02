using System.Globalization;
using System.Text.Json;
using FluentAssertions;
using Moq;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcBitcoinAnalysis;
using ThaiX.Application.Trading;

namespace ThaiX.Application.UnitTests.Features.ExternalData.MarketData;

public sealed class GetMexcBitcoinAnalysisQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenApiReturns500DailyCandles_ComputesTechnicalAnalysis()
    {
        // Arrange
        var rows = CreateCandles(500);
        var externalDataService = new Mock<IExternalDataService>();
        externalDataService
            .Setup(service => service.GetMexcSpotKlinesAsync<List<List<JsonElement>>>(
                "BTCUSDT", "1d", 500, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rows);

        var indicatorService = new Mock<IIndicatorService>();
        indicatorService.Setup(service => service.GetMinimumBars(It.IsAny<KlineExecutionContext>())).Returns(60);
        indicatorService.Setup(service => service.Compute(
                It.IsAny<KlineExecutionContext>(),
                It.Is<IReadOnlyList<Kline>>(klines => klines.Count == 500)))
            .Returns(new IndicatorResult
            {
                Ema9 = 64000m,
                Ema21 = 63900m,
                Ema50 = 63800m,
                Rsi14 = 61m,
                Atr14 = 1200m,
                AverageVolume20 = 10m,
                LatestClose = 64000m,
                HighestHigh20 = 64500m,
                LowestLow20 = 63000m
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
                    Entry = 64000m,
                    StopLoss = 62200m,
                    TakeProfit1 = 66300m,
                    TakeProfit2 = 67900m
                }
            });

        var handler = new GetMexcBitcoinAnalysisQueryHandler(
            externalDataService.Object, indicatorService.Object, executionEngine.Object);

        // Act
        var result = await handler.Handle(new GetMexcBitcoinAnalysisQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Symbol.Should().Be("BTCUSDT");
        result.HistoryCount.Should().Be(500);
        result.History.Should().HaveCount(500);
        result.Price.Should().Be(64000m);
        result.PreviousClose.Should().Be(63999m);
        result.Change.Should().Be(1m);
        result.ChangePct.Should().BeApproximately(1m / 63999m * 100m, 0.0001m);
        result.Ema9.Should().Be(64000m);
        result.Rsi14.Should().Be(61m);
        result.Trend.Should().Be("Up");
        result.Signal.Should().Be("Long");
        result.Entry.Should().Be(64000m);
        result.StopLoss.Should().Be(62200m);
        indicatorService.Verify(service => service.GetMinimumBars(It.Is<KlineExecutionContext>(context =>
            context.MarketType == MarketType.CryptoSpot && context.Timeframe == "1d")), Times.Once);
        externalDataService.Verify(service => service.GetMexcSpotKlinesAsync<List<List<JsonElement>>>(
            "BTCUSDT", "1d", 500, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenHistoryIsBelowIndicatorMinimum_ReturnsInsufficientHistory()
    {
        // Arrange
        var externalDataService = new Mock<IExternalDataService>();
        externalDataService
            .Setup(service => service.GetMexcSpotKlinesAsync<List<List<JsonElement>>>(
                "BTCUSDT", "1d", 500, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCandles(20));

        var indicatorService = new Mock<IIndicatorService>();
        indicatorService.Setup(service => service.GetMinimumBars(It.IsAny<KlineExecutionContext>())).Returns(60);
        var executionEngine = new Mock<IKlineExecutionEngine>();
        var handler = new GetMexcBitcoinAnalysisQueryHandler(
            externalDataService.Object, indicatorService.Object, executionEngine.Object);

        // Act
        var result = await handler.Handle(new GetMexcBitcoinAnalysisQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.HistoryCount.Should().Be(20);
        result.History.Should().HaveCount(20);
        result.Message.Should().Contain("Insufficient daily Bitcoin candle history");
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
                "BTCUSDT", "1d", 500, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetMexcBitcoinAnalysisQueryHandler(
            externalDataService.Object, Mock.Of<IIndicatorService>(), Mock.Of<IKlineExecutionEngine>());

        // Act
        var result = await handler.Handle(new GetMexcBitcoinAnalysisQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("No valid daily Bitcoin candles");
    }

    private static List<List<JsonElement>> CreateCandles(int count)
    {
        var rows = new List<List<JsonElement>>(count);
        var startTime = DateTimeOffset.FromUnixTimeMilliseconds(1_700_000_000_000);
        for (var i = 0; i < count; i++)
        {
            var open = 63500m + i;
            var close = open + 1m;
            var high = close + 100m;
            var low = open - 100m;
            var time = startTime.AddDays(i).ToUnixTimeMilliseconds();
            var closeTime = startTime.AddDays(i + 1).AddMilliseconds(-1).ToUnixTimeMilliseconds();
            var json = $"[{time},\"{open.ToString(CultureInfo.InvariantCulture)}\",\"{high.ToString(CultureInfo.InvariantCulture)}\",\"{low.ToString(CultureInfo.InvariantCulture)}\",\"{close.ToString(CultureInfo.InvariantCulture)}\",\"10\",{closeTime},\"{(close * 10m).ToString(CultureInfo.InvariantCulture)}\"]";
            using var document = JsonDocument.Parse(json);
            rows.Add(document.RootElement.EnumerateArray().Select(element => element.Clone()).ToList());
        }

        return rows;
    }
}
