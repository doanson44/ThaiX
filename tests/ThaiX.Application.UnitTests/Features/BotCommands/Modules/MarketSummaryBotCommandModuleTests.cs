using MediatR;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.BotCommands.Modules;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetInvestingVnIndexAnalysis;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcGoldAnalysis;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24Hr;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24HrBySymbol;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetYahooFinanceVix;

namespace ThaiX.Application.UnitTests.Features.BotCommands.Modules;

public sealed class MarketSummaryBotCommandModuleTests
{
    [Fact]
    public async Task ExecuteAsync_WhenAllSourcesSucceed_ShouldReturnCombinedMarketData()
    {
        // Arrange
        var vix = new YahooFinanceVixResponse
        {
            Symbol = "^VIX",
            CurrentValue = 18.5m,
            PreviousClose = 17.9m,
            Percentile = 0.72,
            IsSpike = false,
            Regime = "EarlyRisk",
            Ema10 = 17.8m,
            Ema20 = 16.9m,
            Momentum3D = 0.04m,
            Momentum5D = 0.06m,
            Success = true
        };
        var vnIndex = new InvestingVnIndexAnalysisResponse
        {
            Success = true,
            Close = 1288.5m,
            Trend = "Bullish",
            Momentum = "Positive",
            Signal = "Neutral",
            Rsi14 = 58.2m,
            Ema9 = 1280m,
            Ema21 = 1270m,
            Ema50 = 1250m,
            HighestHigh20 = 1300m,
            LowestLow20 = 1210m
        };

        var gold = new MexcGoldAnalysisResponse
        {
            Symbol = "GOLD(XAUT)USDT",
            Price = 2350m,
            BopPrice = 2300m,
            Change = 50m,
            ChangePct = 2.17m,
            High = 2360m,
            Low = 2290m,
            Volume = 12m,
            QuoteVolume = 28200m,
            LastUpdated = "2026-11-02",
            Success = true
        };

        var mediator = new Mock<IMediator>();
        mediator
            .Setup(x => x.Send(
                It.Is<IRequest<MexcGoldAnalysisResponse>>(query => query is GetMexcGoldAnalysisQuery),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(gold);
        mediator
            .Setup(x => x.Send(
                It.Is<IRequest<YahooFinanceVixResponse>>(query => query is GetYahooFinanceVixQuery),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vix);
        mediator
            .Setup(x => x.Send(
                It.Is<IRequest<InvestingVnIndexAnalysisResponse>>(query => query is GetInvestingVnIndexAnalysisQuery),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vnIndex);

        mediator
            .Setup(x => x.Send(
                It.Is<IRequest<MexcSpotTicker24HrBySymbolResponse>>(query =>
                    query is GetMexcSpotTicker24HrBySymbolQuery { Symbol: "BTCUSDT" }),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MexcSpotTicker24HrBySymbolResponse
            {
                Symbol = "BTCUSDT",
                Success = true,
                Data = new MexcSpotTicker24HrDto
                {
                    Symbol = "BTCUSDT",
                    LastPrice = "68000.50",
                    PriceChange = "1200.50",
                    PriceChangePercent = "1.80",
                    HighPrice = "69000",
                    LowPrice = "66500",
                    QuoteVolume = "1250000000"
                }
            });

        var module = new MarketSummaryBotCommandModule(mediator.Object);
        var context = new BotCommandContext
        {
            Channel = "Telegram",
            ExternalChannelId = "test-channel",
            ParsedCommand = new BotParsedCommand
            {
                Name = "market-summary",
                RawText = "/market-summary",
                Arguments = [],
                Flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            }
        };

        // Act
        var result = await module.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Status.Should().Be(BotCommandExecutionStatus.Completed);
        result.PlainText.Should().Contain("VIX (^VIX): value=18.5");
        result.PlainText.Should().Contain("regime=EarlyRisk");
        result.PlainText.Should().Contain("VNINDEX (daily):");
        result.PlainText.Should().Contain("close=1288.5");
        result.PlainText.Should().Contain("MEXC gold (GOLD(XAUT)USDT): price=2350");
        result.PlainText.Should().Contain("Bitcoin spot (BTCUSDT): last=68000.50");
        result.PlainText.Should().Contain("change24h=1200.50 (1.80%)");
        result.Card.Should().NotBeNull();
        result.Card!.Type.Should().Be(NotificationCardType.Summary);
        result.Card.Metrics.Should().Contain(x => x.Label == "VIX");
        result.Card.Metrics.Should().Contain(x => x.Label == "VNINDEX Close" && x.Value == "1288.5");
        result.Card.Metrics.Should().Contain(x => x.Label == "Gold Price" && x.Value == "2350");
        result.Card.Metrics.Should().Contain(x => x.Label == "BTC Price (USDT)" && x.Value == "68000.50");
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyOneSourceSucceeds_ShouldReturnPartialMarketSummary()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(x => x.Send(
                It.Is<IRequest<MexcGoldAnalysisResponse>>(query => query is GetMexcGoldAnalysisQuery),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Gold source unavailable"));

        mediator
            .Setup(x => x.Send(
                It.Is<IRequest<MexcSpotTicker24HrBySymbolResponse>>(query => query.GetType() == typeof(GetMexcSpotTicker24HrBySymbolQuery)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MexcSpotTicker24HrBySymbolResponse
            {
                Symbol = "BTCUSDT",
                Success = false,
                Message = "BTC source unavailable"
            });
        mediator
            .Setup(x => x.Send(
                It.Is<IRequest<YahooFinanceVixResponse>>(query => query is GetYahooFinanceVixQuery),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new YahooFinanceVixResponse
            {
                Symbol = "^VIX",
                CurrentValue = 21m,
                Success = true,
                Regime = "Panic"
            });
        mediator
            .Setup(x => x.Send(
                It.Is<IRequest<InvestingVnIndexAnalysisResponse>>(query => query is GetInvestingVnIndexAnalysisQuery),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InvestingVnIndexAnalysisResponse
            {
                Success = false,
                Message = "Source unavailable"
            });

        var module = new MarketSummaryBotCommandModule(mediator.Object);
        var context = new BotCommandContext
        {
            Channel = "Slack",
            ExternalChannelId = "test-channel",
            ParsedCommand = new BotParsedCommand
            {
                Name = "market-summary",
                RawText = "/market-summary",
                Arguments = [],
                Flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            }
        };

        // Act
        var result = await module.ExecuteAsync(context, CancellationToken.None);

        // Assert
        result.Status.Should().Be(BotCommandExecutionStatus.Completed);
        result.PlainText.Should().Contain("value=21");
        result.PlainText.Should().Contain("VNINDEX: unavailable");
        result.PlainText.Should().Contain("MEXC gold: unavailable");
    }
}
