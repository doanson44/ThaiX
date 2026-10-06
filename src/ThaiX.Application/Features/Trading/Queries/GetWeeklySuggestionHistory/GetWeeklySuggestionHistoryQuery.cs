using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.TradingSuggestions;

namespace ThaiX.Application.Features.Trading.Queries.GetWeeklySuggestionHistory;

public sealed record GetWeeklySuggestionHistoryQuery : IAppQuery<WeeklySuggestionHistoryResult>
{
    public SuggestionAssetClass? AssetClass { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed record WeeklySuggestionHistoryResult
{
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required IReadOnlyList<WeeklySuggestionHistoryReportDto> Items { get; init; }
}

public sealed record WeeklySuggestionHistoryReportDto
{
    public required Guid ReportId { get; init; }
    public required string ReportKey { get; init; }
    public required DateTime RunAtUtc { get; init; }
    public required SuggestionAssetClass AssetClass { get; init; }
    public required SuggestionReportType ReportType { get; init; }
    public required WeeklySuggestionReportStatus Status { get; init; }
    public required int PickedCount { get; init; }
    public required IReadOnlyList<WeeklySuggestionHistoryItemDto> Picks { get; init; }
}

public sealed record WeeklySuggestionHistoryItemDto
{
    public required string Timeframe { get; init; }
    public required int Rank { get; init; }
    public required string Symbol { get; init; }
    public required SuggestionAssetClass MarketType { get; init; }
    public required decimal EntryPrice { get; init; }
    public required string Signal { get; init; }
    public required decimal Confidence { get; init; }
}

public sealed class GetWeeklySuggestionHistoryQueryHandler
    : IRequestHandler<GetWeeklySuggestionHistoryQuery, WeeklySuggestionHistoryResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetWeeklySuggestionHistoryQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WeeklySuggestionHistoryResult> Handle(
        GetWeeklySuggestionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => request.PageSize
        };

        var query = _dbContext.WeeklySuggestionReports
            .AsNoTracking()
            .AsQueryable();

        if (request.AssetClass.HasValue)
        {
            query = query.Where(x => x.AssetClass == request.AssetClass.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var reports = await query
            .OrderByDescending(x => x.RunAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new WeeklySuggestionHistoryReportDto
            {
                ReportId = x.Id,
                ReportKey = x.ReportKey,
                RunAtUtc = x.RunAtUtc,
                AssetClass = x.AssetClass,
                ReportType = x.ReportType,
                Status = x.Status,
                PickedCount = x.Items.Count,
                Picks = x.Items
                    .OrderBy(i => i.Timeframe)
                    .ThenBy(i => i.Rank)
                    .Select(i => new WeeklySuggestionHistoryItemDto
                    {
                        Timeframe = i.Timeframe,
                        Rank = i.Rank,
                        Symbol = i.Symbol,
                        MarketType = i.MarketType,
                        EntryPrice = i.EntryPrice,
                        Signal = i.Signal,
                        Confidence = i.Confidence
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return new WeeklySuggestionHistoryResult
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = reports
        };
    }
}
