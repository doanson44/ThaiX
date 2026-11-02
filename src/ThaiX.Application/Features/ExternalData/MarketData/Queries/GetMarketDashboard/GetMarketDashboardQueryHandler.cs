using MediatR;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetInvestingVnIndexAnalysis;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcBitcoinAnalysis;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcGoldAnalysis;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetYahooFinanceVix;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMarketDashboard;

public sealed class GetMarketDashboardQueryHandler(IMediator mediator)
    : IRequestHandler<GetMarketDashboardQuery, MarketDashboardResponse>
{
    public async Task<MarketDashboardResponse> Handle(
        GetMarketDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var vixTask = SendOrDefaultAsync(
            new GetYahooFinanceVixQuery(),
            () => new YahooFinanceVixResponse { Symbol = "^VIX", Success = false, Message = "Unable to retrieve VIX data." },
            cancellationToken);
        var vnIndexTask = SendOrDefaultAsync(
            new GetInvestingVnIndexAnalysisQuery(),
            () => new InvestingVnIndexAnalysisResponse { Success = false, Message = "Unable to retrieve VNINDEX data." },
            cancellationToken);
        var bitcoinTask = SendOrDefaultAsync(
            new GetMexcBitcoinAnalysisQuery(),
            () => new MexcBitcoinAnalysisResponse { Success = false, Message = "Unable to retrieve Bitcoin data." },
            cancellationToken);
        var goldTask = SendOrDefaultAsync(
            new GetMexcGoldAnalysisQuery(),
            () => new MexcGoldAnalysisResponse { Success = false, Message = "Unable to retrieve gold data." },
            cancellationToken);

        await Task.WhenAll(vixTask, vnIndexTask, bitcoinTask, goldTask);

        return new MarketDashboardResponse
        {
            Vix = await vixTask,
            VnIndex = await vnIndexTask,
            Bitcoin = await bitcoinTask,
            Gold = await goldTask
        };
    }

    private async Task<TResponse> SendOrDefaultAsync<TResponse>(
        IRequest<TResponse> query,
        Func<TResponse> fallback,
        CancellationToken cancellationToken)
    {
        try
        {
            return await mediator.Send(query, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return fallback();
        }
    }
}
