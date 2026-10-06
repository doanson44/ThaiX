using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Trading;

namespace ThaiX.Application.Features.Trading.Queries.GetTradeSuggestion;

public sealed class GetTradeSuggestionQueryHandler
    : IRequestHandler<GetTradeSuggestionQuery, TradeSuggestionDto>
{
    private readonly IKlineExecutionEngine _engine;
    private readonly IExternalMarketDataService _external;
    private readonly IIndicatorService _indicatorService;

    public GetTradeSuggestionQueryHandler(IKlineExecutionEngine engine, IExternalMarketDataService external, IIndicatorService indicatorService)
    {
        _engine = engine;
        _external = external;
        _indicatorService = indicatorService;
    }

    public async Task<TradeSuggestionDto> Handle(GetTradeSuggestionQuery request, CancellationToken cancellationToken)
    {
        var klines = await _external.GetKlinesAsync(
            request.Symbol,
            request.MarketType,
            request.Timeframe,
            request.UseLongTermTimeframe,
            cancellationToken);

        var context = new KlineExecutionContext
        {
            Timeframe = request.Timeframe,
            MarketType = request.MarketType,
            MarketRegime = request.MarketRegime,
            EventRisk = request.EventRisk
        };

        if (klines.Count < _indicatorService.GetMinimumBars(context))
        {
            // Skip gracefully for sparse long-term data (for example VN Month1), no exception needed.
            return new TradeSuggestionDto
            {
                Trend = "None",
                Momentum = "None",
                Setup = "None",
                Signal = "None",
                EntryPrice = null,
                StopLoss = null,
                TakeProfit1 = null,
                TakeProfit2 = null,
                Confidence = 0m
            };
        }

        var result = _engine.Execute(context, klines);

        return new TradeSuggestionDto
        {
            Trend = result.Trend.ToString(),
            Momentum = result.Momentum.ToString(),
            Setup = result.Setup.ToString(),
            Signal = result.Signal.ToString(),
            EntryPrice = result.Trade?.Entry,
            StopLoss = result.Trade?.StopLoss,
            TakeProfit1 = result.Trade?.TakeProfit1,
            TakeProfit2 = result.Trade?.TakeProfit2,
            Confidence = result.Confidence
        };
    }
}
