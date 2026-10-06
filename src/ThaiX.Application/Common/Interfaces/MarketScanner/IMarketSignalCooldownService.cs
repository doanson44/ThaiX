using ThaiX.Domain.Aggregates.MarketScanner;

namespace ThaiX.Application.Common.Interfaces.MarketScanner;

public interface IMarketSignalCooldownService
{
    bool IsOnCooldown(string symbol, MarketSignalType signalType);

    void MarkTriggered(string symbol, MarketSignalType signalType);
}
