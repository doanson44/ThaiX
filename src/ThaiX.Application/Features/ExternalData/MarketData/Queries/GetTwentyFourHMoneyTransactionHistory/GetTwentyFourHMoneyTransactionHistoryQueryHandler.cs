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

        // Aggregate in the database before transferring data. Grouping by date preserves
        // the boundaries needed to calculate each look-back period without loading every tick.
        var oldestFrom = Periods[^1].From(anchor.Value);
        var rows = await _dbContext.TwentyFourHMoneyTransactions
            .AsNoTracking()
            .Where(t => t.Symbol == symbol && t.TradeDate >= oldestFrom && t.TradeDate <= anchor.Value)
            .GroupBy(t => new { t.TradeDate, t.Price })
            .Select(group => new
            {
                group.Key.TradeDate,
                group.Key.Price,
                TransactionCount = group.Count(),
                Volume = group.Sum(transaction => transaction.MatchQuantity)
            })
            .ToListAsync(cancellationToken);

        var aggregates = rows
            .Select(row => new PriceComparisonAggregate(
                row.TradeDate,
                row.Price,
                row.TransactionCount,
                row.Volume))
            .ToList();

        return BuildPriceComparison(aggregates, anchor.Value, currentPrice);
    }

    internal static List<PriceComparisonPeriodDto> BuildPriceComparison(
        IReadOnlyList<PriceComparisonAggregate> aggregates,
        DateOnly anchor,
        decimal currentPrice)
    {
        var result = new List<PriceComparisonPeriodDto>(Periods.Length);
        foreach (var (label, fromFn) in Periods)
        {
            var fromDate = fromFn(anchor);
            var inWindow = aggregates
                .Where(row => row.TradeDate >= fromDate && row.TradeDate <= anchor)
                .ToList();

            var totalVolume = inWindow.Sum(row => row.Volume);
            var betterVolume = inWindow.Where(row => row.Price > currentPrice).Sum(row => row.Volume);
            var worseVolume = inWindow.Where(row => row.Price < currentPrice).Sum(row => row.Volume);
            var equalVolume = totalVolume - betterVolume - worseVolume;
            var weightedSum = inWindow.Sum(row => row.Price * row.Volume);

            var top3Highest = inWindow
                .Where(row => row.Price > currentPrice)
                .GroupBy(row => row.Price)
                .Select(group => new PriceVolumePointDto
                {
                    Price = group.Key,
                    Volume = group.Sum(row => row.Volume)
                })
                .OrderByDescending(point => point.Price)
                .Take(3)
                .ToList();

            var top3Lowest = inWindow
                .Where(row => row.Price < currentPrice)
                .GroupBy(row => row.Price)
                .Select(group => new PriceVolumePointDto
                {
                    Price = group.Key,
                    Volume = group.Sum(row => row.Volume)
                })
                .OrderBy(point => point.Price)
                .Take(3)
                .ToList();

            result.Add(new PriceComparisonPeriodDto
            {
                Period = label,
                FromDate = fromDate,
                ToDate = anchor,
                TransactionCount = inWindow.Sum(row => (long)row.TransactionCount),
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

internal sealed record PriceComparisonAggregate(
    DateOnly TradeDate,
    decimal Price,
    int TransactionCount,
    long Volume);