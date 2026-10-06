using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractKline;

/// <summary>
/// Handler for GetMexcContractKlineQuery.
/// Retrieves MEXC contract kline data by symbol and optional interval.
/// </summary>
public sealed class GetMexcContractKlineQueryHandler
    : IRequestHandler<GetMexcContractKlineQuery, MexcContractKlineResponse>
{
    private static readonly Dictionary<string, string> SupportedIntervals = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Min1"] = "Min1",
        ["Min5"] = "Min5",
        ["Min15"] = "Min15",
        ["Min30"] = "Min30",
        ["Min60"] = "Min60",
        ["Hour4"] = "Hour4",
        ["Hour8"] = "Hour8",
        ["Day1"] = "Day1",
        ["Week1"] = "Week1",
        ["Month1"] = "Month1"
    };

    private readonly IExternalDataService _externalDataService;

    public GetMexcContractKlineQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<MexcContractKlineResponse> Handle(
        GetMexcContractKlineQuery request,
        CancellationToken cancellationToken)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();
        if (symbol.Length is < 3 or > 30 || !symbol.All(c => char.IsLetterOrDigit(c) || c == '_'))
        {
            return new MexcContractKlineResponse
            {
                Symbol = symbol,
                Interval = "Min1",
                Data = null,
                Success = false,
                Message = "Invalid symbol. Use letters/digits/underscore, length 3-30."
            };
        }

        var hasInterval = !string.IsNullOrWhiteSpace(request.Interval);
        var intervalInput = hasInterval ? request.Interval!.Trim() : "Min1";

        if (!SupportedIntervals.TryGetValue(intervalInput, out var normalizedInterval))
        {
            return new MexcContractKlineResponse
            {
                Symbol = symbol,
                Interval = intervalInput,
                Data = null,
                Success = false,
                Message = "Invalid interval. Supported values: Min1, Min5, Min15, Min30, Min60, Hour4, Hour8, Day1, Week1, Month1."
            };
        }

        var apiResponse = await _externalDataService.GetMexcContractKlineAsync<InternalMexcKlineApiResponse>(
            symbol,
            hasInterval ? normalizedInterval : null,
            cancellationToken);

        if (apiResponse is null || !apiResponse.Success || apiResponse.Code != 0 || apiResponse.Data is null)
        {
            return new MexcContractKlineResponse
            {
                Symbol = symbol,
                Interval = normalizedInterval,
                Data = null,
                Success = false,
                Message = "Failed to retrieve MEXC contract kline from external API."
            };
        }

        var data = apiResponse.Data;
        if (data.Time is null || data.Open is null || data.Close is null || data.High is null ||
            data.Low is null || data.Vol is null || data.Amount is null || data.RealOpen is null ||
            data.RealClose is null || data.RealHigh is null || data.RealLow is null)
        {
            return new MexcContractKlineResponse
            {
                Symbol = symbol,
                Interval = normalizedInterval,
                Data = null,
                Success = false,
                Message = "MEXC returned incomplete kline data."
            };
        }

        return new MexcContractKlineResponse
        {
            Symbol = symbol,
            Interval = normalizedInterval,
            Data = new MexcContractKlineDataDto
            {
                Time = data.Time,
                Open = data.Open,
                Close = data.Close,
                High = data.High,
                Low = data.Low,
                Vol = data.Vol,
                Amount = data.Amount,
                RealOpen = data.RealOpen,
                RealClose = data.RealClose,
                RealHigh = data.RealHigh,
                RealLow = data.RealLow
            },
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalMexcKlineApiResponse
    {
        public bool Success { get; init; }
        public int Code { get; init; }
        public InternalMexcKlineData? Data { get; init; }
    }

    private sealed record InternalMexcKlineData
    {
        public List<long>? Time { get; init; }
        public List<decimal>? Open { get; init; }
        public List<decimal>? Close { get; init; }
        public List<decimal>? High { get; init; }
        public List<decimal>? Low { get; init; }
        public List<decimal>? Vol { get; init; }
        public List<decimal>? Amount { get; init; }
        public List<decimal>? RealOpen { get; init; }
        public List<decimal>? RealClose { get; init; }
        public List<decimal>? RealHigh { get; init; }
        public List<decimal>? RealLow { get; init; }
    }
}
