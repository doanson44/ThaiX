using MediatR;
using System.Text.Json;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotKlines;

/// <summary>
/// Handler for GetMexcSpotKlinesQuery.
/// Retrieves MEXC spot klines by symbol and interval.
/// </summary>
public sealed class GetMexcSpotKlinesQueryHandler
    : IRequestHandler<GetMexcSpotKlinesQuery, MexcSpotKlinesResponse>
{
    private static readonly HashSet<string> SupportedIntervals = new(StringComparer.Ordinal)
    {
        "1m",
        "5m",
        "15m",
        "30m",
        "60m",
        "4h",
        "1d",
        "1W",
        "1M"
    };

    private readonly IExternalDataService _externalDataService;

    public GetMexcSpotKlinesQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<MexcSpotKlinesResponse> Handle(
        GetMexcSpotKlinesQuery request,
        CancellationToken cancellationToken)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();
        if (symbol.Length is < 3 or > 20 || !symbol.All(char.IsLetterOrDigit))
        {
            return new MexcSpotKlinesResponse
            {
                Symbol = symbol,
                Interval = request.Interval?.Trim() ?? string.Empty,
                Data = new List<MexcSpotKlineDto>(),
                Success = false,
                Message = "Invalid symbol. Use only letters/digits, length 3-20."
            };
        }

        var interval = request.Interval?.Trim() ?? string.Empty;
        if (!SupportedIntervals.Contains(interval))
        {
            return new MexcSpotKlinesResponse
            {
                Symbol = symbol,
                Interval = interval,
                Data = new List<MexcSpotKlineDto>(),
                Success = false,
                Message = "Invalid interval. Supported values: 1m, 5m, 15m, 30m, 60m, 4h, 1d, 1W, 1M."
            };
        }

        var apiRows = await _externalDataService
            .GetMexcSpotKlinesAsync<List<List<JsonElement>>>(symbol, interval, cancellationToken);

        if (apiRows is null)
        {
            return new MexcSpotKlinesResponse
            {
                Symbol = symbol,
                Interval = interval,
                Data = new List<MexcSpotKlineDto>(),
                Success = false,
                Message = "Failed to retrieve MEXC spot klines from external API."
            };
        }

        var mappedData = new List<MexcSpotKlineDto>();
        foreach (var row in apiRows)
        {
            if (row.Count < 8 || !TryReadInt64(row[0], out var openTime) || !TryReadInt64(row[6], out var closeTime))
            {
                continue;
            }

            mappedData.Add(new MexcSpotKlineDto
            {
                OpenTime = openTime,
                OpenPrice = ReadAsString(row[1]),
                HighPrice = ReadAsString(row[2]),
                LowPrice = ReadAsString(row[3]),
                ClosePrice = ReadAsString(row[4]),
                Volume = ReadAsString(row[5]),
                CloseTime = closeTime,
                QuoteVolume = ReadAsString(row[7])
            });
        }

        return new MexcSpotKlinesResponse
        {
            Symbol = symbol,
            Interval = interval,
            Data = mappedData,
            Success = true,
            Message = "Success"
        };
    }

    private static bool TryReadInt64(JsonElement element, out long value)
    {
        if (element.ValueKind == JsonValueKind.Number)
        {
            return element.TryGetInt64(out value);
        }

        if (element.ValueKind == JsonValueKind.String &&
            long.TryParse(element.GetString(), out value))
        {
            return true;
        }

        value = 0;
        return false;
    }

    private static string ReadAsString(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.ToString(),
            _ => string.Empty
        };
    }
}
