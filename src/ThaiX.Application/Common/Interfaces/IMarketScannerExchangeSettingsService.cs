namespace ThaiX.Application.Common.Interfaces;

public sealed record MarketScannerExchangeSettings
{
    public bool RequiredBybit { get; init; }
    public bool RequiredBinance { get; init; }
}

public interface IMarketScannerExchangeSettingsService
{
    Task<MarketScannerExchangeSettings> GetAsync(CancellationToken cancellationToken = default);

    Task UpdateAsync(MarketScannerExchangeSettings settings, CancellationToken cancellationToken = default);
}