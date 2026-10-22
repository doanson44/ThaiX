using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.BotCommands.Services;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24Hr;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24HrBySymbol;

namespace ThaiX.Application.UnitTests.Features.BotCommands.Services;

public sealed class BotConversationalResponderTests
{
    [Fact]
    public async Task RespondAsync_WhenCryptoMarketAnalysisIsRequested_ShouldFetchTickerAndSendDataToSecondAiCall()
    {
        // Arrange
        var aiService = new Mock<IAiTextGenerationService>();
        aiService
            .SetupSequence(x => x.GenerateAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiTextGenerationResult(
                "test",
                "test",
                "{\"needsMarketData\":true,\"assetType\":\"crypto\",\"symbol\":\"XRP\"}"))
            .ReturnsAsync(new AiTextGenerationResult("test", "test", "XRP is volatile; manage downside risk."));

        var mediator = new Mock<IMediator>();
        mediator
            .Setup(x => x.Send(
                It.IsAny<IRequest<MexcSpotTicker24HrBySymbolResponse>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MexcSpotTicker24HrBySymbolResponse
            {
                Symbol = "XRPUSDT",
                Success = true,
                Data = new MexcSpotTicker24HrDto
                {
                    Symbol = "XRPUSDT",
                    LastPrice = "0.5200",
                    PriceChangePercent = "2.50",
                    HighPrice = "0.5300",
                    LowPrice = "0.5000",
                    Volume = "1000000",
                    QuoteVolume = "520000",
                    CloseTime = 1_760_000_000_000
                }
            });

        var responder = new BotConversationalResponder(aiService.Object, mediator.Object);

        // Act
        var result = await responder.RespondAsync(
            "Should I buy XRP now?",
            "Be cautious and factual.",
            CancellationToken.None);

        // Assert
        result.Should().Be("XRP is volatile; manage downside risk.");
        mediator.Verify(x => x.Send(
            It.Is<IRequest<MexcSpotTicker24HrBySymbolResponse>>(query =>
                ((GetMexcSpotTicker24HrBySymbolQuery)query).Symbol == "XRPUSDT"),
            It.IsAny<CancellationToken>()), Times.Once);
        aiService.Verify(x => x.GenerateAsync(
            It.Is<string>(prompt => prompt.Contains("Last price: 0.5200", StringComparison.Ordinal)
                && prompt.Contains("24-hour change percent: 2.50%", StringComparison.Ordinal)),
            It.Is<string?>(prompt => prompt != null && prompt.Contains("ground the analysis", StringComparison.Ordinal)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RespondAsync_WhenNoCryptoMarketMentionExists_ShouldUseSingleConversationalAiCall()
    {
        // Arrange
        var aiService = new Mock<IAiTextGenerationService>();
        aiService
            .Setup(x => x.GenerateAsync(
                "Can you summarize the issue?",
                "Answer conversationally.",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiTextGenerationResult("test", "test", "Here is the summary."));

        var mediator = new Mock<IMediator>();
        var responder = new BotConversationalResponder(aiService.Object, mediator.Object);

        // Act
        var result = await responder.RespondAsync(
            "Can you summarize the issue?",
            "Answer conversationally.",
            CancellationToken.None);

        // Assert
        result.Should().Be("Here is the summary.");
        mediator.Verify(x => x.Send(
            It.IsAny<IRequest<MexcSpotTicker24HrBySymbolResponse>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        aiService.Verify(x => x.GenerateAsync(
            "Can you summarize the issue?",
            "Answer conversationally.",
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
