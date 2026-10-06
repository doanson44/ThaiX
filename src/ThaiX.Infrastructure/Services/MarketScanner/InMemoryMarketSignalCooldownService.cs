using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using ThaiX.Application.Common.Interfaces.MarketScanner;
using ThaiX.Domain.Aggregates.MarketScanner;
using ThaiX.Infrastructure.Configuration;

namespace ThaiX.Infrastructure.Services.MarketScanner;

internal sealed class InMemoryMarketSignalCooldownService(
    IOptions<MarketScannerOptions> options) : IMarketSignalCooldownService
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _cooldowns = new();
    private readonly int _cooldownMinutes = options.Value.CooldownMinutes;

    public bool IsOnCooldown(string symbol, MarketSignalType signalType)
    {
        var key = BuildKey(symbol, signalType);
        if (!_cooldowns.TryGetValue(key, out var expiry))
            return false;

        if (DateTimeOffset.UtcNow < expiry)
            return true;

        _cooldowns.TryRemove(key, out _);
        return false;
    }

    public void MarkTriggered(string symbol, MarketSignalType signalType)
    {
        var key = BuildKey(symbol, signalType);
        var expiry = DateTimeOffset.UtcNow.AddMinutes(_cooldownMinutes);
        _cooldowns[key] = expiry;
    }

    private static string BuildKey(string symbol, MarketSignalType signalType) =>
        $"{symbol}:{signalType}";
}
