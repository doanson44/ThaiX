using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.TradingSuggestions;

namespace ThaiX.Application.Features.Trading.Queries.EvaluateWeeklySuggestionPerformance;

public sealed record EvaluateWeeklySuggestionPerformanceQuery : IAppQuery<EvaluateWeeklySuggestionPerformanceResult>
{
    public required IReadOnlyList<EvaluateWeeklySuggestionSymbolInput> Symbols { get; init; }
    public SuggestionAssetClass? AssetClass { get; init; }
    public int LookbackReports { get; init; } = 12;
}

public sealed record EvaluateWeeklySuggestionSymbolInput
{
    public required string Symbol { get; init; }
    public decimal? CurrentPrice { get; init; }
}

public sealed record EvaluateWeeklySuggestionPerformanceResult
{
    public required int MatchedCount { get; init; }
    public required IReadOnlyList<EvaluatedWeeklySuggestionDto> Items { get; init; }
}

public sealed record EvaluatedWeeklySuggestionDto
{
    public required string Symbol { get; init; }
    public required DateTime RunAtUtc { get; init; }
    public required string Timeframe { get; init; }
    public required int Rank { get; init; }
    public required decimal EntryPrice { get; init; }
    public decimal? CurrentPrice { get; init; }
    public decimal? ChangePercentFromEntry { get; init; }
    public required string ReportKey { get; init; }
    public required SuggestionAssetClass AssetClass { get; init; }
}

public sealed class EvaluateWeeklySuggestionPerformanceQueryHandler
    : IRequestHandler<EvaluateWeeklySuggestionPerformanceQuery, EvaluateWeeklySuggestionPerformanceResult>
{
    private readonly IApplicationDbContext _dbContext;

    public EvaluateWeeklySuggestionPerformanceQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EvaluateWeeklySuggestionPerformanceResult> Handle(
        EvaluateWeeklySuggestionPerformanceQuery request,
        CancellationToken cancellationToken)
    {
        var symbols = request.Symbols
            .Where(x => !string.IsNullOrWhiteSpace(x.Symbol))
            .GroupBy(x => x.Symbol.Trim().ToUpperInvariant())
            .Select(g => g.Last())
            .ToDictionary(x => x.Symbol.Trim().ToUpperInvariant(), x => x.CurrentPrice, StringComparer.OrdinalIgnoreCase);

        if (symbols.Count == 0)
        {
            return new EvaluateWeeklySuggestionPerformanceResult
            {
                MatchedCount = 0,
                Items = []
            };
        }

        var lookback = request.LookbackReports switch
        {
            < 1 => 12,
            > 52 => 52,
            _ => request.LookbackReports
        };

        var reportQuery = _dbContext.WeeklySuggestionReports
            .AsNoTracking()
            .AsQueryable();

        if (request.AssetClass.HasValue)
        {
            reportQuery = reportQuery.Where(x => x.AssetClass == request.AssetClass.Value);
        }

        var recentReportIds = await reportQuery
            .OrderByDescending(x => x.RunAtUtc)
            .Take(lookback)
            .Select(x => new { x.Id, x.ReportKey, x.RunAtUtc, x.AssetClass })
            .ToListAsync(cancellationToken);

        if (recentReportIds.Count == 0)
        {
            return new EvaluateWeeklySuggestionPerformanceResult
            {
                MatchedCount = 0,
                Items = []
            };
        }

        var reportMap = recentReportIds.ToDictionary(x => x.Id);
        var reportIdSet = recentReportIds.Select(x => x.Id).ToHashSet();

        var items = await _dbContext.WeeklySuggestionReportItems
            .AsNoTracking()
            .Where(x => reportIdSet.Contains(x.ReportId) && symbols.Keys.Contains(x.Symbol))
            .OrderByDescending(x => x.Report.RunAtUtc)
            .ThenBy(x => x.Timeframe)
            .ThenBy(x => x.Rank)
            .Select(x => new
            {
                x.Symbol,
                x.EntryPrice,
                x.Timeframe,
                x.Rank,
                x.ReportId
            })
            .ToListAsync(cancellationToken);

        var evaluated = items.Select(x =>
        {
            var report = reportMap[x.ReportId];
            symbols.TryGetValue(x.Symbol, out var currentPrice);
            decimal? changePercent = null;

            if (currentPrice.HasValue && x.EntryPrice > 0)
            {
                changePercent = ((currentPrice.Value - x.EntryPrice) / x.EntryPrice) * 100m;
            }

            return new EvaluatedWeeklySuggestionDto
            {
                Symbol = x.Symbol,
                RunAtUtc = report.RunAtUtc,
                Timeframe = x.Timeframe,
                Rank = x.Rank,
                EntryPrice = x.EntryPrice,
                CurrentPrice = currentPrice,
                ChangePercentFromEntry = changePercent,
                ReportKey = report.ReportKey,
                AssetClass = report.AssetClass
            };
        }).ToList();

        return new EvaluateWeeklySuggestionPerformanceResult
        {
            MatchedCount = evaluated.Count,
            Items = evaluated
        };
    }
}
