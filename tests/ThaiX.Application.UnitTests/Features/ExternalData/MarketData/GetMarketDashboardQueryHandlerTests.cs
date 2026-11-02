using MediatR;
using Moq;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetInvestingVnIndexAnalysis;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMarketDashboard;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcBitcoinAnalysis;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcGoldAnalysis;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetYahooFinanceVix;

namespace ThaiX.Application.UnitTests.Features.ExternalData.MarketData;

public sealed class GetMarketDashboardQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenAllMarketQueriesReturnData_ReturnsAllFourSnapshots()
    {
        // Arrange
        var vix = new YahooFinanceVixResponse { Symbol = "^VIX", Success = true };
        var vnIndex = new InvestingVnIndexAnalysisResponse { Symbol = "VNINDEX", Success = true };
        var bitcoin = new MexcBitcoinAnalysisResponse { Symbol = "BTCUSDT", Success = true };
        var gold = new MexcGoldAnalysisResponse { Symbol = "GOLD(XAUT)USDT", Success = true };
        var mediator = new Mock<IMediator>();

        mediator.Setup(x => x.Send(It.IsAny<GetYahooFinanceVixQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(vix);
        mediator.Setup(x => x.Send(It.IsAny<GetInvestingVnIndexAnalysisQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(vnIndex);
        mediator.Setup(x => x.Send(It.IsAny<GetMexcBitcoinAnalysisQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(bitcoin);
        mediator.Setup(x => x.Send(It.IsAny<GetMexcGoldAnalysisQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(gold);

        var handler = new GetMarketDashboardQueryHandler(mediator.Object);

        // Act
        var result = await handler.Handle(new GetMarketDashboardQuery(), CancellationToken.None);

        // Assert
        Assert.Same(vix, result.Vix);
        Assert.Same(vnIndex, result.VnIndex);
        Assert.Same(bitcoin, result.Bitcoin);
        Assert.Same(gold, result.Gold);
        mediator.Verify(x => x.Send(It.IsAny<GetYahooFinanceVixQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        mediator.Verify(x => x.Send(It.IsAny<GetInvestingVnIndexAnalysisQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        mediator.Verify(x => x.Send(It.IsAny<GetMexcBitcoinAnalysisQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        mediator.Verify(x => x.Send(It.IsAny<GetMexcGoldAnalysisQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
