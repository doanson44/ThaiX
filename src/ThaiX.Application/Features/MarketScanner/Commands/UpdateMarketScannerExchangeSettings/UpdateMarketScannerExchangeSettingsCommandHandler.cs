using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.MarketScanner.Commands.UpdateMarketScannerExchangeSettings;

public sealed class UpdateMarketScannerExchangeSettingsCommandHandler(
    IMarketScannerExchangeSettingsService settingsService)
    : IRequestHandler<UpdateMarketScannerExchangeSettingsCommand>
{
    public Task Handle(
        UpdateMarketScannerExchangeSettingsCommand request,
        CancellationToken cancellationToken) =>
        settingsService.UpdateAsync(
            new MarketScannerExchangeSettings
            {
                RequiredBybit = request.RequiredBybit,
                RequiredBinance = request.RequiredBinance
            },
            cancellationToken);
}
