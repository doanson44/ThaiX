using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetTwentyFourHMoneyTransactionHistory;

public sealed class GetTwentyFourHMoneyTransactionHistoryQueryHandler
    : IRequestHandler<GetTwentyFourHMoneyTransactionHistoryQuery, TwentyFourHMoneyTransactionHistoryResponse>
{
    private static readonly Regex SymbolRegex = new(@"^[A-Za-z0-9]{1,10}$", RegexOptions.Compiled);

    // Look-back windows, widest last (used to bound the single fetch query).
    private static readonly (string Label, Func<DateOnly, DateOnly> From)[] Periods =
    [
        ("1D", anchor => anchor),
        ("1W", anchor => anchor.AddDays(-7)),
        ("1M", anchor => anchor.AddMonths(-1)),
        ("3M", anchor => anchor.AddMonths(-3)),
        ("6M", anchor => anchor.AddMonths(-6)),
        ("1Y", anchor => anchor.AddYears(-1))
    ];

    private readonly IApplicationDbContext _dbContext;

    public GetTwentyFourHMoneyTransactionHistoryQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TwentyFourHMoneyTransactionHistoryResponse> Handle(
        GetTwentyFourHMoneyTransactionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Symbol) || !SymbolRegex.IsMatch(request.Symbol))
        {
            return new TwentyFourHMoneyTransactionHistoryResponse
            {
                Success = false,
                Message = "Invalid stock symbol. Must be 1-10 alphanumeric characters."
            };
        }

        var symbol = request.Symbol.Trim().ToUpperInvariant();

        if (request.CurrentPrice is <= 0)
        {
            return new TwentyFourHMoneyTransactionHistoryResponse
            {
                Symbol = symbol,
                Success = false,
                Message = "Current price must be greater than zero."
            };
        }

        var page = Math.Max(request.Page, 1);
        var perPage = Math.Clamp(request.PerPage, 1, 500);

        var paged = await _dbContext.TwentyFourHMoneyTransactions
            .AsNoTracking()
            .Where(t => t.Symbol == symbol)
            .OrderByDescending(t => t.TradeDate)
            .ThenByDescending(t => t.TotalVolume)
            .Select(t => new TwentyFourHMoneyTransactionRecordDto
            {
                TradeDate = t.TradeDate,
                TradeTime = t.TradeTime,
                Price = t.Price,
                Change = t.Change,
                MatchQuantity = t.MatchQuantity,
                TotalVolume = t.TotalVolume,
                Side = t.Side
            })
            .ToPagedListAsync(page, perPage, cancellationToken);

        List<PriceComparisonPeriodDto>? comparison = null;
        string? message = null;

        if (request.CurrentPrice is { } currentPrice)
        {
            comparison = await BuildPriceComparisonAsync(symbol, currentPrice, cancellationToken);
            if (comparison is null)
            {
                message = "No stored transaction history available for this symbol yet.";
            }
        }

        return new TwentyFourHMoneyTransactionHistoryResponse
        {
            Symbol = symbol,
            CurrentPrice = request.CurrentPrice,
            Page = page,
            PerPage = perPage,
            TotalCount = paged.TotalCount,
            Transactions = [.. paged.Items],
            PriceComparison = comparison,
            Success = true,
            Message = message
        };
    }

    private async Task<List<PriceComparisonPeriodDto>?> BuildPriceComparisonAsync(
        string symbol,
        decimal currentPrice,
        CancellationToken cancellationToken)
    {
        var anchor = await _dbContext.TwentyFourHMoneyTransactions
            .Where(t => t.Symbol == symbol)
            .Select(t => (DateOnly?)t.TradeDate)
            .MaxAsync(cancellationToken);

        if (anchor is null)
        {
            return null;
        }

        // Fetch once across the widest (1Y) window, then slice per-period in memory --
        // cheaper than issuing 6 separate aggregate queries against the same rows.
        var oldestFrom = Periods[^1].From(anchor.Value);

        var rows = await _dbContext.TwentyFourHMoneyTransactions
            .AsNoTracking()
            .Where(t => t.Symbol == symbol && t.TradeDate >= oldestFrom && t.TradeDate <= anchor.Value)
            .Select(t => new { t.TradeDate, t.Price, t.MatchQuantity })
            .ToListAsync(cancellationToken);

        var result = new List<PriceComparisonPeriodDto>(Periods.Length);
        foreach (var (label, fromFn) in Periods)
        {
            var fromDate = fromFn(anchor.Value);
            var inWindow = rows.Where(r => r.TradeDate >= fromDate && r.TradeDate <= anchor.Value).ToList();

            var totalVolume = inWindow.Sum(r => r.MatchQuantity);
            var betterVolume = inWindow.Where(r => r.Price > currentPrice).Sum(r => r.MatchQuantity);
            var worseVolume = inWindow.Where(r => r.Price < currentPrice).Sum(r => r.MatchQuantity);
            var equalVolume = totalVolume - betterVolume - worseVolume;
            var weightedSum = inWindow.Sum(r => r.Price * r.MatchQuantity);

            var top3Highest = inWindow
                .Where(r => r.Price > currentPrice)
                .GroupBy(r => r.Price)
                .Select(g => new PriceVolumePointDto { Price = g.Key, Volume = g.Sum(r => r.MatchQuantity) })
                .OrderByDescending(p => p.Price)
                .Take(3)
                .ToList();

            var top3Lowest = inWindow
                .Where(r => r.Price < currentPrice)
                .GroupBy(r => r.Price)
                .Select(g => new PriceVolumePointDto { Price = g.Key, Volume = g.Sum(r => r.MatchQuantity) })
                .OrderBy(p => p.Price)
                .Take(3)
                .ToList();

            result.Add(new PriceComparisonPeriodDto
            {
                Period = label,
                FromDate = fromDate,
                ToDate = anchor.Value,
                TransactionCount = inWindow.Count,
                TotalVolume = totalVolume,
                BetterVolume = betterVolume,
                WorseVolume = worseVolume,
                EqualVolume = equalVolume,
                BetterPercent = totalVolume > 0 ? Math.Round(betterVolume / (decimal)totalVolume * 100, 2) : 0,
                WorsePercent = totalVolume > 0 ? Math.Round(worseVolume / (decimal)totalVolume * 100, 2) : 0,
                AveragePrice = totalVolume > 0 ? Math.Round(weightedSum / totalVolume, 4) : 0,
                Top3Highest = top3Highest,
                Top3Lowest = top3Lowest
            });
        }

        return result;
    }
}
