using MediatR;

namespace ThaiX.Application.Features.MarketScanner.Commands.UpdateMarketScannerExchangeSettings;

public sealed record UpdateMarketScannerExchangeSettingsCommand : IRequest
{
    public bool RequiredBybit { get; init; }
    public bool RequiredBinance { get; init; }
}
