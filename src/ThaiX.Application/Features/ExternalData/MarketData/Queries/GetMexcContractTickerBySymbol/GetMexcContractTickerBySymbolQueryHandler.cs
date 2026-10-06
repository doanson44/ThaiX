using MediatR;
using System.Text.Json;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractTickers;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractTickerBySymbol;

/// <summary>
/// Handler for GetMexcContractTickerBySymbolQuery.
/// Retrieves MEXC contract ticker data by symbol.
/// </summary>
public sealed class GetMexcContractTickerBySymbolQueryHandler
    : IRequestHandler<GetMexcContractTickerBySymbolQuery, MexcContractTickerBySymbolResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetMexcContractTickerBySymbolQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<MexcContractTickerBySymbolResponse> Handle(
        GetMexcContractTickerBySymbolQuery request,
        CancellationToken cancellationToken)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();

        if (symbol.Length is < 3 or > 30 || !symbol.All(c => char.IsLetterOrDigit(c) || c == '_'))
        {
            return new MexcContractTickerBySymbolResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Invalid symbol. Use letters/digits/underscore, length 3-30."
            };
        }

        var apiResponse = await _externalDataService
            .GetMexcContractTickerBySymbolAsync<InternalMexcApiObjectResponse>(symbol, cancellationToken);

        if (apiResponse is null || !apiResponse.Success || apiResponse.Code != 0 || apiResponse.Data is null)
        {
            return new MexcContractTickerBySymbolResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Failed to retrieve MEXC contract ticker from external API."
            };
        }

        if (string.IsNullOrWhiteSpace(apiResponse.Data.Symbol))
        {
            return new MexcContractTickerBySymbolResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "MEXC returned incomplete ticker data."
            };
        }

        return new MexcContractTickerBySymbolResponse
        {
            Symbol = symbol,
            Data = new MexcContractTickerDto
            {
                ContractId = apiResponse.Data.ContractId,
                Symbol = apiResponse.Data.Symbol,
                LastPrice = apiResponse.Data.LastPrice,
                Bid1 = apiResponse.Data.Bid1,
                Ask1 = apiResponse.Data.Ask1,
                High24Price = apiResponse.Data.High24Price,
                Low24Price = apiResponse.Data.Low24Price,
                Volume24 = apiResponse.Data.Volume24,
                Amount24 = apiResponse.Data.Amount24,
                HoldVol = apiResponse.Data.HoldVol,
                RiseFallRate = apiResponse.Data.RiseFallRate,
                RiseFallValue = apiResponse.Data.RiseFallValue,
                IndexPrice = apiResponse.Data.IndexPrice,
                FairPrice = apiResponse.Data.FairPrice,
                FundingRate = apiResponse.Data.FundingRate,
                MaxBidPrice = apiResponse.Data.MaxBidPrice,
                MinAskPrice = apiResponse.Data.MinAskPrice,
                RiseFallRates = apiResponse.Data.RiseFallRates,
                RiseFallRatesOfTimezone = apiResponse.Data.RiseFallRatesOfTimezone,
                Timestamp = apiResponse.Data.Timestamp
            },
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalMexcApiObjectResponse
    {
        public bool Success { get; init; }
        public int Code { get; init; }
        public InternalMexcTicker? Data { get; init; }
    }

    private sealed record InternalMexcTicker
    {
        public int? ContractId { get; init; }
        public string Symbol { get; init; } = string.Empty;
        public decimal? LastPrice { get; init; }
        public decimal? Bid1 { get; init; }
        public decimal? Ask1 { get; init; }
        public decimal? High24Price { get; init; }
        public decimal? Low24Price { get; init; }
        public decimal? Volume24 { get; init; }
        public decimal? Amount24 { get; init; }
        public decimal? HoldVol { get; init; }
        public decimal? RiseFallRate { get; init; }
        public decimal? RiseFallValue { get; init; }
        public decimal? IndexPrice { get; init; }
        public decimal? FairPrice { get; init; }
        public decimal? FundingRate { get; init; }
        public decimal? MaxBidPrice { get; init; }
        public decimal? MinAskPrice { get; init; }
        public JsonElement? RiseFallRates { get; init; }
        public List<decimal>? RiseFallRatesOfTimezone { get; init; }
        public long? Timestamp { get; init; }
    }
}
