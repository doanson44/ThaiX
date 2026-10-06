using ThaiX.Application.Trading;

namespace ThaiX.Application.Common.Interfaces;

public interface IExternalMarketDataService
{
    Task<IReadOnlyList<Kline>> GetKlinesAsync(
        string symbol,
        MarketType marketType,
        string timeframe,
        bool useLongTermTimeframe,
        CancellationToken ct);
}
