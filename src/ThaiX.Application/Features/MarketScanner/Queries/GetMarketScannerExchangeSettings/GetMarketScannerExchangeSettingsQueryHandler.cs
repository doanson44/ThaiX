using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.MarketScanner.Queries.GetMarketScannerExchangeSettings;

public sealed class GetMarketScannerExchangeSettingsQueryHandler(
    IMarketScannerExchangeSettingsService settingsService)
    : IRequestHandler<GetMarketScannerExchangeSettingsQuery, MarketScannerExchangeSettings>
{
    public Task<MarketScannerExchangeSettings> Handle(
        GetMarketScannerExchangeSettingsQuery request,
        CancellationToken cancellationToken) =>
        settingsService.GetAsync(cancellationToken);
}
