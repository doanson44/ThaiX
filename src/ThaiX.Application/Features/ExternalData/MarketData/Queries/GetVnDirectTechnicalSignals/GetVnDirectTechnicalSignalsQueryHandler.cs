using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectTechnicalSignals;

/// <summary>
/// Handler for GetVnDirectTechnicalSignalsQuery.
/// Retrieves VnDirect technical signals from external API via proxy.
/// </summary>
public sealed class GetVnDirectTechnicalSignalsQueryHandler
    : IRequestHandler<GetVnDirectTechnicalSignalsQuery, VnDirectTechnicalSignalsResponse>
{
    private readonly IExternalDataService _externalDataService;
    private static readonly string[] ValidStrategies = { "cipLong", "cipShort" };

    public GetVnDirectTechnicalSignalsQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<VnDirectTechnicalSignalsResponse> Handle(
        GetVnDirectTechnicalSignalsQuery request,
        CancellationToken cancellationToken)
    {
        var strategy = string.IsNullOrWhiteSpace(request.Strategy)
            ? "cipLong"
            : request.Strategy.Trim();

        if (!ValidStrategies.Contains(strategy, StringComparer.OrdinalIgnoreCase))
        {
            return new VnDirectTechnicalSignalsResponse
            {
                Data = [],
                Success = false,
                Message = $"Invalid strategy. Supported strategies: {string.Join(", ", ValidStrategies)}"
            };
        }

        var normalizedStrategy = strategy.Equals("cipShort", StringComparison.OrdinalIgnoreCase)
            ? "cipShort"
            : "cipLong";

        var apiResponse = await _externalDataService
            .GetVnDirectTechnicalSignalsAsync<InternalVnDirectTechnicalSignalsResponse>(normalizedStrategy, cancellationToken);

        if (apiResponse?.Data is null)
        {
            return new VnDirectTechnicalSignalsResponse
            {
                Data = [],
                Success = false,
                Message = "Failed to retrieve VnDirect technical signals from external API."
            };
        }

        var mappedData = apiResponse.Data
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .Select(x => new VnDirectTechnicalSignalDto
            {
                Code = x.Code,
                TradingDate = x.TradingDate,
                Time = x.Time,
                Strategy = x.Strategy,
                TotalSignal = x.TotalSignal,
                Indicators = x.Indicators
                    .Select(i => new VnDirectTechnicalIndicatorDto
                    {
                        Indicator = i.Indicator,
                        Period = i.Period,
                        IndicatorName = i.IndicatorName,
                        Signal = i.Signal,
                        LatestValue = i.LatestValue
                    })
                    .ToList()
            })
            .ToList();

        return new VnDirectTechnicalSignalsResponse
        {
            CurrentPage = apiResponse.CurrentPage,
            Size = apiResponse.Size,
            TotalElements = apiResponse.TotalElements,
            TotalPages = apiResponse.TotalPages,
            Data = mappedData,
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalVnDirectTechnicalSignalsResponse
    {
        public int CurrentPage { get; init; }
        public int Size { get; init; }
        public int TotalElements { get; init; }
        public int TotalPages { get; init; }
        public List<InternalVnDirectTechnicalSignalDto> Data { get; init; } = [];
    }

    private sealed record InternalVnDirectTechnicalSignalDto
    {
        public string Code { get; init; } = string.Empty;
        public string TradingDate { get; init; } = string.Empty;
        public string Time { get; init; } = string.Empty;
        public string Strategy { get; init; } = string.Empty;
        public string TotalSignal { get; init; } = string.Empty;
        public List<InternalVnDirectTechnicalIndicatorDto> Indicators { get; init; } = [];
    }

    private sealed record InternalVnDirectTechnicalIndicatorDto
    {
        public string Indicator { get; init; } = string.Empty;
        public string Period { get; init; } = string.Empty;
        public string IndicatorName { get; init; } = string.Empty;
        public string Signal { get; init; } = string.Empty;
        public decimal LatestValue { get; init; }
    }
}
