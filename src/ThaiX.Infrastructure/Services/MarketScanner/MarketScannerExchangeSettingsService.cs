using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Infrastructure.Services.MarketScanner;

public sealed class MarketScannerExchangeSettingsService(
    IApplicationDbContext context,
    IMemoryCache cache) : IMarketScannerExchangeSettingsService
{
    private const string JsonBinCode = "MARKET_SCANNER_EXCHANGE_SETTINGS";
    private const string CacheKey = "MarketScanner:ExchangeSettings";

    public async Task<MarketScannerExchangeSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKey, out MarketScannerExchangeSettings? cached) && cached is not null)
            return cached;

        var jsonBin = await context.JsonBins
            .AsNoTracking()
            .Where(item => item.Code == JsonBinCode)
            .Select(item => item.Content)
            .FirstOrDefaultAsync(cancellationToken);

        var settings = jsonBin is null
            ? new MarketScannerExchangeSettings()
            : JsonSerializer.Deserialize<MarketScannerExchangeSettings>(jsonBin)
                ?? throw new InvalidDataException("Market scanner exchange settings JsonBin is invalid.");

        cache.Set(CacheKey, settings);
        return settings;
    }

    public async Task UpdateAsync(
        MarketScannerExchangeSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var json = JsonSerializer.SerializeToUtf8Bytes(settings);
        var entity = await context.JsonBins
            .FirstOrDefaultAsync(item => item.Code == JsonBinCode, cancellationToken);

        if (entity is null)
        {
            entity = JsonBin.Create(
                code: JsonBinCode,
                name: "Market Scanner Exchange Settings",
                category: JsonBinCategories.Config,
                content: json);

            context.JsonBins.Add(entity);
        }
        else
        {
            entity.ReplaceContent(json, "application/json", isCompressed: false);
        }

        await context.SaveChangesAsync(cancellationToken);
        cache.Set(CacheKey, settings);
    }
}
