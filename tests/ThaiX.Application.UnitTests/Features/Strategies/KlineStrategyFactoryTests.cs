using ThaiX.Application.Strategies;
using ThaiX.Application.Trading;

namespace ThaiX.Application.UnitTests.Features.Strategies;

public sealed class KlineStrategyFactoryTests
{
    [Fact]
    public void Create_WhenMarketTypeStock_ShouldReturnStockStrategy()
    {
        var factory = new KlineStrategyFactory();
        var strategy = factory.Create(new KlineExecutionContext { MarketType = MarketType.Stock, Timeframe = "1h" });

        strategy.Should().BeOfType<StockStrategy>();
    }

    [Fact]
    public void Create_WhenMarketTypeCryptoSpot_ShouldReturnCryptoStrategy()
    {
        var factory = new KlineStrategyFactory();
        var strategy = factory.Create(new KlineExecutionContext { MarketType = MarketType.CryptoSpot, Timeframe = "1h" });

        strategy.Should().BeOfType<CryptoStrategy>();
    }

    [Fact]
    public void Create_WhenMarketTypeUnsupported_ShouldThrowArgumentOutOfRangeException()
    {
        var factory = new KlineStrategyFactory();
        var context = new KlineExecutionContext { MarketType = (MarketType)999, Timeframe = "1h" };

        Action act = () => factory.Create(context);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
