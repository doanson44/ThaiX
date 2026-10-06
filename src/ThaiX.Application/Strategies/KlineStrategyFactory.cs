using ThaiX.Application.Trading;

namespace ThaiX.Application.Strategies;

public sealed class KlineStrategyFactory : IKlineStrategyFactory
{
    private readonly StockStrategy _stockStrategy = new();
    private readonly CryptoStrategy _cryptoStrategy = new();

    public IKlineStrategy Create(KlineExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.MarketType switch
        {
            MarketType.Stock => _stockStrategy,
            MarketType.Crypto => _cryptoStrategy,
            MarketType.CryptoSpot => _cryptoStrategy,
            _ => throw new ArgumentOutOfRangeException(nameof(context.MarketType), context.MarketType, "Unsupported market type.")
        };
    }
}
