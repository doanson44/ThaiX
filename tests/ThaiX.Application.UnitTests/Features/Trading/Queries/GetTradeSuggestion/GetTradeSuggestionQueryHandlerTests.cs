using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Trading.Queries.GetTradeSuggestion;
using ThaiX.Application.Trading;

namespace ThaiX.Application.UnitTests.Features.Trading.Queries.GetTradeSuggestion;

public sealed class GetTradeSuggestionQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenTooFewBars_ReturnsEmptySuggestion()
    {
        var externalMock = new Mock<IExternalMarketDataService>();
        externalMock.Setup(x => x.GetKlinesAsync(
                "BTCUSDT",
                MarketType.CryptoSpot,
                "Min1",
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new Kline { Time = DateTime.UtcNow.AddMinutes(-1), Open = 30000m, High = 30010m, Low = 29900m, Close = 30005m, Volume = 1m }
            });

        var indicatorMock = new Mock<IIndicatorService>();
        indicatorMock.Setup(x => x.GetMinimumBars(It.IsAny<KlineExecutionContext>())).Returns(5);

        var engineMock = new Mock<IKlineExecutionEngine>();

        var handler = new GetTradeSuggestionQueryHandler(engineMock.Object, externalMock.Object, indicatorMock.Object);
        var result = await handler.Handle(new GetTradeSuggestionQuery
        {
            Symbol = "BTCUSDT",
            MarketType = MarketType.CryptoSpot,
            Timeframe = "Min1",
            UseLongTermTimeframe = false,
            MarketRegime = MarketRegime.RiskOn,
            EventRisk = EventRisk.Low
        }, CancellationToken.None);

        result.Signal.Should().Be("None");
        result.EntryPrice.Should().BeNull();
        result.Confidence.Should().Be(0m);
        engineMock.Verify(x => x.Execute(It.IsAny<KlineExecutionContext>(), It.IsAny<IReadOnlyList<Kline>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEnoughBars_ReturnsEngineResult()
    {
        var externalMock = new Mock<IExternalMarketDataService>();
        externalMock.Setup(x => x.GetKlinesAsync(
                "BTCUSDT",
                MarketType.CryptoSpot,
                "Min1",
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new Kline { Time = DateTime.UtcNow.AddMinutes(-3), Open = 30000m, High = 30010m, Low = 29900m, Close = 30005m, Volume = 1m },
                new Kline { Time = DateTime.UtcNow.AddMinutes(-2), Open = 30005m, High = 30015m, Low = 30000m, Close = 30010m, Volume = 1m },
                new Kline { Time = DateTime.UtcNow.AddMinutes(-1), Open = 30010m, High = 30020m, Low = 30005m, Close = 30015m, Volume = 1m }
            });

        var indicatorMock = new Mock<IIndicatorService>();
        indicatorMock.Setup(x => x.GetMinimumBars(It.IsAny<KlineExecutionContext>())).Returns(3);

        var executionResult = new KlineExecutionResult
        {
            Trend = TrendDirection.Up,
            Momentum = MomentumStrength.Strong,
            Setup = SetupType.Pullback,
            Signal = SignalType.Long,
            Trade = new TradePlan
            {
                Side = SignalType.Long,
                Entry = 30015m,
                StopLoss = 29900m,
                TakeProfit1 = 30100m,
                TakeProfit2 = 30200m
            },
            Confidence = 0.88m
        };

        var engineMock = new Mock<IKlineExecutionEngine>();
        engineMock.Setup(x => x.Execute(It.IsAny<KlineExecutionContext>(), It.IsAny<IReadOnlyList<Kline>>())).Returns(executionResult);

        var handler = new GetTradeSuggestionQueryHandler(engineMock.Object, externalMock.Object, indicatorMock.Object);
        var result = await handler.Handle(new GetTradeSuggestionQuery
        {
            Symbol = "BTCUSDT",
            MarketType = MarketType.CryptoSpot,
            Timeframe = "Min1",
            UseLongTermTimeframe = false,
            MarketRegime = MarketRegime.RiskOn,
            EventRisk = EventRisk.Low
        }, CancellationToken.None);

        result.Signal.Should().Be("Long");
        result.Trend.Should().Be("Up");
        result.EntryPrice.Should().Be(30015m);
        result.Confidence.Should().Be(0.88m);
    }
}
