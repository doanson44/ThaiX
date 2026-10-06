using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectStockPrices;

/// <summary>
/// Handler for GetVnDirectStockPricesQuery.
/// Retrieves VnDirect stock price history from external API via proxy.
/// </summary>
public sealed class GetVnDirectStockPricesQueryHandler
    : IRequestHandler<GetVnDirectStockPricesQuery, VnDirectStockPricesResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetVnDirectStockPricesQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<VnDirectStockPricesResponse> Handle(
        GetVnDirectStockPricesQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code) ||
            !System.Text.RegularExpressions.Regex.IsMatch(request.Code, @"^[A-Za-z0-9]{1,10}$"))
        {
            return new VnDirectStockPricesResponse
            {
                Data = [],
                Success = false,
                Message = "Invalid stock code. Must be 1-10 alphanumeric characters."
            };
        }

        var apiResponse = await _externalDataService
            .GetVnDirectStockPricesAsync<InternalVnDirectStockPricesResponse>(request.Code, cancellationToken);

        if (apiResponse?.Data is null)
        {
            return new VnDirectStockPricesResponse
            {
                Code = request.Code.Trim().ToUpperInvariant(),
                Data = [],
                Success = false,
                Message = "Failed to retrieve VnDirect stock prices from external API."
            };
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        var mappedData = apiResponse.Data
            .Select(x => new VnDirectStockPriceDto
            {
                Code = x.Code,
                Date = x.Date,
                Time = x.Time,
                Floor = x.Floor,
                Type = x.Type,
                BasicPrice = x.BasicPrice,
                CeilingPrice = x.CeilingPrice,
                FloorPrice = x.FloorPrice,
                Open = x.Open,
                High = x.High,
                Low = x.Low,
                Close = x.Close,
                Average = x.Average,
                AdOpen = x.AdOpen,
                AdHigh = x.AdHigh,
                AdLow = x.AdLow,
                AdClose = x.AdClose,
                AdAverage = x.AdAverage,
                NmVolume = x.NmVolume,
                NmValue = x.NmValue,
                PtVolume = x.PtVolume,
                PtValue = x.PtValue,
                Change = x.Change,
                AdChange = x.AdChange,
                PctChange = x.PctChange
            })
            .ToList();

        return new VnDirectStockPricesResponse
        {
            Code = normalizedCode,
            CurrentPage = apiResponse.CurrentPage,
            Size = apiResponse.Size,
            TotalElements = apiResponse.TotalElements,
            TotalPages = apiResponse.TotalPages,
            Data = mappedData,
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalVnDirectStockPricesResponse
    {
        public int CurrentPage { get; init; }
        public int Size { get; init; }
        public int TotalElements { get; init; }
        public int TotalPages { get; init; }
        public List<InternalVnDirectStockPriceDto> Data { get; init; } = [];
    }

    private sealed record InternalVnDirectStockPriceDto
    {
        public string Code { get; init; } = string.Empty;
        public string Date { get; init; } = string.Empty;
        public string Time { get; init; } = string.Empty;
        public string Floor { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public decimal BasicPrice { get; init; }
        public decimal CeilingPrice { get; init; }
        public decimal FloorPrice { get; init; }
        public decimal Open { get; init; }
        public decimal High { get; init; }
        public decimal Low { get; init; }
        public decimal Close { get; init; }
        public decimal Average { get; init; }
        public decimal AdOpen { get; init; }
        public decimal AdHigh { get; init; }
        public decimal AdLow { get; init; }
        public decimal AdClose { get; init; }
        public decimal AdAverage { get; init; }
        public decimal NmVolume { get; init; }
        public decimal NmValue { get; init; }
        public decimal PtVolume { get; init; }
        public decimal PtValue { get; init; }
        public decimal Change { get; init; }
        public decimal AdChange { get; init; }
        public decimal PctChange { get; init; }
    }
}
