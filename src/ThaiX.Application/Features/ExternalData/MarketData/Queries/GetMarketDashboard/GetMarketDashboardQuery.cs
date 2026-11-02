using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetInvestingVnIndexAnalysis;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcBitcoinAnalysis;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcGoldAnalysis;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetYahooFinanceVix;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMarketDashboard;

public sealed record GetMarketDashboardQuery : IAppQuery<MarketDashboardResponse>;

public sealed record MarketDashboardResponse
{
    public required YahooFinanceVixResponse Vix { get; init; }
    public required InvestingVnIndexAnalysisResponse VnIndex { get; init; }
    public required MexcBitcoinAnalysisResponse Bitcoin { get; init; }
    public required MexcGoldAnalysisResponse Gold { get; init; }
}
