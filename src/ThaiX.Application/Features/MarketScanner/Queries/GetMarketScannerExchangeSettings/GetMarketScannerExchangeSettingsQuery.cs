using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.MarketScanner.Queries.GetMarketScannerExchangeSettings;

public sealed record GetMarketScannerExchangeSettingsQuery : IAppQuery<MarketScannerExchangeSettings>;