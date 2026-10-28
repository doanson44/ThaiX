using MediatR;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.BotCommands.Modules;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetInvestingVnIndexAnalysis;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetYahooFinanceVix;

namespace ThaiX.Application.UnitTests.Features.BotCommands.Modules;

public sealed class MarketSummaryBotCommandModuleTests
{
    [Fact]
    public async Task ExecuteAsync_WhenBothSourcesSucceed_ShouldReturnCombinedVixAndVnIndexData()
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

        var mediator = new Mock<IMediator>();
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
        result.Card.Should().NotBeNull();
        result.Card!.Type.Should().Be(NotificationCardType.Summary);
        result.Card.Metrics.Should().Contain(x => x.Label == "VIX");
        result.Card.Metrics.Should().Contain(x => x.Label == "VNINDEX Close" && x.Value == "1288.5");
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyOneSourceSucceeds_ShouldReturnPartialMarketSummary()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
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
    }
}
