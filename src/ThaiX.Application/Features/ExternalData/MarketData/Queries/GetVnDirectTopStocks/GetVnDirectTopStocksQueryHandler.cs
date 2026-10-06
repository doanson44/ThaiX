using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Services.Scoring;
using ThaiX.Application.Features.ExternalData.Funds.Queries.GetDragonCapitalFundPortfolio;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectEvents;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectTechnicalSignals;
using ThaiX.Application.Features.TcbsTop10.Queries.GetTcbsTop10Portfolios;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectTopStocks;

/// <summary>
/// Handler for GetVnDirectTopStocksQuery.
/// Retrieves VnDirect top stocks from external API via proxy and enriches them with technical, portfolio, and event metrics.
/// </summary>
public sealed class GetVnDirectTopStocksQueryHandler
    : IRequestHandler<GetVnDirectTopStocksQuery, VnDirectTopStocksResponse>
{
    private static readonly string[] DragonFundCodes =
        { "VF1", "VF4", "VFMVN30", "VFMVND", "VFMMID" };

    private readonly IExternalDataService _externalDataService;
    private readonly IMediator _mediator;
    private readonly IStockScoringPipeline _stockScoringPipeline;

    public GetVnDirectTopStocksQueryHandler(
        IExternalDataService externalDataService,
        IMediator mediator,
        IStockScoringPipeline stockScoringPipeline)
    {
        _externalDataService = externalDataService;
        _mediator = mediator;
        _stockScoringPipeline = stockScoringPipeline;
    }

    public async Task<VnDirectTopStocksResponse> Handle(
        GetVnDirectTopStocksQuery request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .GetVnDirectTopStocksAsync<InternalVnDirectTopStocksResponse>(cancellationToken);

        if (apiResponse?.Data is null)
        {
            return new VnDirectTopStocksResponse
            {
                Data = [],
                Success = false,
                Message = "Failed to retrieve VnDirect top stocks from external API."
            };
        }

        var baseStocks = apiResponse.Data
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .Select(x => new VnDirectTopStockDto
            {
                Code = x.Code,
                Index = x.Index,
                LastPrice = x.LastPrice,
                LastUpdated = x.LastUpdated,
                PriceChgCr1D = x.PriceChgCr1D,
                PriceChgPctCr1D = x.PriceChgPctCr1D,
                AccumulatedVal = x.AccumulatedVal,
                NmVolumeAvgCr20D = x.NmVolumeAvgCr20D,
                NmVolNmVolAvg20DPctCr = x.NmVolNmVolAvg20DPctCr,
                TotalVolumeAvgCr20D = x.TotalVolumeAvgCr20D,
                PtVolTotalVolAvg20DPctCr = x.PtVolTotalVolAvg20DPctCr,
                PtVolAvg5DTotalVolAvg20DPctCr = x.PtVolAvg5DTotalVolAvg20DPctCr,
                PtVolSumCr5D = x.PtVolSumCr5D,
                PtValAvgCr5D = x.PtValAvgCr5D,
                PtVolAvgCr5D = x.PtVolAvgCr5D
            })
            .ToList();

        if (!request.IncludeEnrichment)
        {
            return new VnDirectTopStocksResponse
            {
                CurrentPage = apiResponse.CurrentPage,
                Size = apiResponse.Size,
                TotalElements = apiResponse.TotalElements,
                TotalPages = apiResponse.TotalPages,
                Data = baseStocks,
                Success = true,
                Message = "Success"
            };
        }

        var cipLongTask = _mediator.Send(
            new GetVnDirectTechnicalSignalsQuery { Strategy = "cipLong" },
            cancellationToken);

        var cipShortTask = _mediator.Send(
            new GetVnDirectTechnicalSignalsQuery { Strategy = "cipShort" },
            cancellationToken);

        var eventsTask = _mediator.Send(new GetVnDirectEventsQuery(), cancellationToken);
        var tcbsTask = _mediator.Send(new GetTcbsTop10PortfoliosQuery(), cancellationToken);
        var dragonTasks = DragonFundCodes
            .Select(code => _mediator.Send(new GetDragonCapitalFundPortfolioQuery { FundCode = code }, cancellationToken))
            .ToList();

        await Task.WhenAll(cipLongTask, cipShortTask, eventsTask, tcbsTask);
        await Task.WhenAll(dragonTasks);

        var longSignals = BuildTechnicalLookup(cipLongTask.Result);
        var shortSignals = BuildTechnicalLookup(cipShortTask.Result);
        var eventLookup = BuildEventLookup(eventsTask.Result?.Data ?? []);
        var tcbsHoldings = new HashSet<string>(tcbsTask.Result.CurrentHoldings, StringComparer.OrdinalIgnoreCase);
        var tcbsAllTimeHoldings = new HashSet<string>(tcbsTask.Result.AllTimeHoldings, StringComparer.OrdinalIgnoreCase);
        var dragonMetrics = BuildDragonHoldings(dragonTasks.Select(t => t.Result));

        var enrichedStocks = baseStocks
            .Select(stock =>
            {
                var codeKey = NormalizeCode(stock.Code);
                var scoringContext = new StockScoringContext
                {
                    Code = codeKey,
                    TotalVolumeAvgCr20D = stock.TotalVolumeAvgCr20D,
                    NmVolumeAvgCr20D = stock.NmVolumeAvgCr20D,
                    NmVolNmVolAvg20DPctCr = stock.NmVolNmVolAvg20DPctCr,
                    LongSignal = longSignals.GetValueOrDefault(codeKey),
                    ShortSignal = shortSignals.GetValueOrDefault(codeKey),
                    IsInTcbsHoldings = tcbsHoldings.Contains(codeKey),
                    DragonMetrics = dragonMetrics.GetValueOrDefault(codeKey)
                        ?? new DragonHoldingMetrics(new HashSet<string>(StringComparer.OrdinalIgnoreCase), 0m),
                    Events = eventLookup.GetValueOrDefault(codeKey) ?? [],
                    EventLookbackDays = request.MaxEventsLookbackDays
                };

                var scoring = _stockScoringPipeline.Execute(scoringContext);

                return stock with
                {
                    LongSignal = scoring.LongSignal,
                    ShortSignal = scoring.ShortSignal,
                    LongBuyCount = scoring.LongBuyCount,
                    LongSellCount = scoring.LongSellCount,
                    ShortBuyCount = scoring.ShortBuyCount,
                    ShortSellCount = scoring.ShortSellCount,
                    InTcbsCurrentHoldings = scoringContext.IsInTcbsHoldings,
                    InTcbsAllTimeHoldings = tcbsAllTimeHoldings.Contains(codeKey),
                    DragonFundCount = scoringContext.DragonMetrics.Funds.Count,
                    DragonFunds = scoringContext.DragonMetrics.Funds.OrderBy(f => f).ToList(),
                    DragonTotalWeight = scoringContext.DragonMetrics.TotalWeight,
                    EventRiskLevel = scoring.EventRiskLevel,
                    RecentEventCount = scoring.RecentEventCount,
                    MostSevereEventType = scoring.MostSevereEventType,
                    LatestEventEffectiveDate = scoring.LatestEventEffectiveDate,
                    TechnicalScore = scoring.TechnicalScore,
                    PortfolioScore = scoring.PortfolioScore,
                    EventPenalty = scoring.EventPenalty,
                    LiquidityPenalty = scoring.LiquidityPenalty,
                    CompositeScore = scoring.CompositeScore,
                    ScoreBreakdown = scoring.ScoreBreakdown.ToList(),
                    DataCompleteness = scoring.DataCompleteness == 1m
                };
            })
            .ToList();

        return new VnDirectTopStocksResponse
        {
            CurrentPage = apiResponse.CurrentPage,
            Size = apiResponse.Size,
            TotalElements = apiResponse.TotalElements,
            TotalPages = apiResponse.TotalPages,
            Data = enrichedStocks,
            Success = true,
            Message = "Success"
        };
    }

    private static IReadOnlyDictionary<string, VnDirectTechnicalSignalDto> BuildTechnicalLookup(VnDirectTechnicalSignalsResponse response)
    {
        return response.Data
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .GroupBy(x => NormalizeCode(x.Code), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<VnDirectEventDto>> BuildEventLookup(IEnumerable<VnDirectEventDto> events)
    {
        return events
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .GroupBy(x => NormalizeCode(x.Code), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<VnDirectEventDto>)g.Select(item => item).ToList(), StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyDictionary<string, DragonHoldingMetrics> BuildDragonHoldings(
        IEnumerable<DragonCapitalFundPortfolioResponse> responses)
    {
        var result = new Dictionary<string, DragonHoldingMetrics>(StringComparer.OrdinalIgnoreCase);

        foreach (var response in responses)
        {
            if (response is null || !response.Success)
            {
                continue;
            }

            var fundCode = string.IsNullOrWhiteSpace(response.FundCode)
                ? string.Empty
                : response.FundCode.ToUpperInvariant();

            foreach (var holding in response.Top10Holdings)
            {
                if (string.IsNullOrWhiteSpace(holding.AssetId))
                {
                    continue;
                }

                var codeKey = NormalizeCode(holding.AssetId);
                if (!result.TryGetValue(codeKey, out var current))
                {
                    current = new DragonHoldingMetrics(new HashSet<string>(StringComparer.OrdinalIgnoreCase), 0m);
                    result[codeKey] = current;
                }

                if (!string.IsNullOrWhiteSpace(fundCode))
                {
                    current.Funds.Add(fundCode);
                }

                result[codeKey] = current with { TotalWeight = current.TotalWeight + holding.Weight };
            }
        }

        return result;
    }

    private static string NormalizeCode(string? code)
        => string.IsNullOrWhiteSpace(code) ? string.Empty : code.Trim().ToUpperInvariant();

    private sealed record InternalVnDirectTopStocksResponse
    {
        public int CurrentPage { get; init; }
        public int Size { get; init; }
        public int TotalElements { get; init; }
        public int TotalPages { get; init; }
        public List<InternalVnDirectTopStockDto> Data { get; init; } = [];
    }

    private sealed record InternalVnDirectTopStockDto
    {
        public string Code { get; init; } = string.Empty;
        public string Index { get; init; } = string.Empty;
        public decimal LastPrice { get; init; }
        public string LastUpdated { get; init; } = string.Empty;
        public decimal PriceChgCr1D { get; init; }
        public decimal PriceChgPctCr1D { get; init; }
        public decimal AccumulatedVal { get; init; }
        public decimal NmVolumeAvgCr20D { get; init; }
        public decimal NmVolNmVolAvg20DPctCr { get; init; }
        public decimal TotalVolumeAvgCr20D { get; init; }
        public decimal PtVolTotalVolAvg20DPctCr { get; init; }
        public decimal PtVolAvg5DTotalVolAvg20DPctCr { get; init; }
        public decimal PtVolSumCr5D { get; init; }
        public decimal PtValAvgCr5D { get; init; }
        public decimal PtVolAvgCr5D { get; init; }
    }
}
