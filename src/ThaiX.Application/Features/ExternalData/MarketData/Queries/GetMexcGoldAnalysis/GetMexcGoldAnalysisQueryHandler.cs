using MediatR;
using System.Globalization;
using System.Text.Json;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcGoldAnalysis;

public sealed class GetMexcGoldAnalysisQueryHandler(IExternalDataService externalDataService)
    : IRequestHandler<GetMexcGoldAnalysisQuery, MexcGoldAnalysisResponse>
{
    private const string GoldSymbol = "GOLD(XAUT)USDT";
    private const string DailyInterval = "1d";

    public async Task<MexcGoldAnalysisResponse> Handle(
        GetMexcGoldAnalysisQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await externalDataService
            .GetMexcSpotKlinesAsync<List<List<JsonElement>>>(GoldSymbol, DailyInterval, cancellationToken);

        var candles = rows?
            .Where(row => row.Count >= 8)
            .Select(ParseCandle)
            .Where(candle => candle is not null)
            .Select(candle => candle!)
            .OrderBy(candle => candle.OpenTime)
            .ToList();

        if (candles is not { Count: > 0 })
        {
            return new MexcGoldAnalysisResponse
            {
                Symbol = GoldSymbol,
                Message = "No valid daily gold candles were returned by MEXC.",
                Success = false
            };
        }

        var latest = candles[^1];
        var change = latest.Close - latest.Open;

        return new MexcGoldAnalysisResponse
        {
            Symbol = GoldSymbol,
            Price = latest.Close,
            BopPrice = latest.Open,
            Change = change,
            ChangePct = latest.Open == 0 ? 0 : change / latest.Open * 100,
            High = latest.High,
            Low = latest.Low,
            Volume = latest.Volume,
            QuoteVolume = latest.QuoteVolume,
            LastUpdated = DateTimeOffset.FromUnixTimeMilliseconds(latest.OpenTime)
                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Message = "Success",
            Success = true
        };
    }

    private static GoldCandle? ParseCandle(List<JsonElement> row)
    {
        if (!TryReadInt64(row[0], out var openTime) ||
            !TryReadDecimal(row[1], out var open) ||
            !TryReadDecimal(row[2], out var high) ||
            !TryReadDecimal(row[3], out var low) ||
            !TryReadDecimal(row[4], out var close) ||
            !TryReadDecimal(row[5], out var volume) ||
            !TryReadInt64(row[6], out var closeTime) ||
            !TryReadDecimal(row[7], out var quoteVolume))
        {
            return null;
        }

        return new GoldCandle(openTime, open, high, low, close, volume, closeTime, quoteVolume);
    }

    private static bool TryReadInt64(JsonElement element, out long value)
    {
        if (element.ValueKind == JsonValueKind.Number)
            return element.TryGetInt64(out value);

        return long.TryParse(element.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryReadDecimal(JsonElement element, out decimal value) =>
        decimal.TryParse(element.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out value);

    private sealed record GoldCandle(
        long OpenTime,
        decimal Open,
        decimal High,
        decimal Low,
        decimal Close,
        decimal Volume,
        long CloseTime,
        decimal QuoteVolume);
}
