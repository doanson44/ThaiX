using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractFundingRates;

/// <summary>
/// Handler for GetMexcContractFundingRatesQuery.
/// Retrieves MEXC contract funding rates for all symbols.
/// </summary>
public sealed class GetMexcContractFundingRatesQueryHandler
    : IRequestHandler<GetMexcContractFundingRatesQuery, MexcContractFundingRatesResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetMexcContractFundingRatesQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<MexcContractFundingRatesResponse> Handle(
        GetMexcContractFundingRatesQuery request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .GetMexcContractFundingRatesAsync<InternalMexcFundingRateApiResponse>(cancellationToken);

        if (apiResponse is null || !apiResponse.Success || apiResponse.Code != 0 || apiResponse.Data is null)
        {
            return new MexcContractFundingRatesResponse
            {
                Data = new List<MexcContractFundingRateDto>(),
                Success = false,
                Message = "Failed to retrieve MEXC contract funding rates from external API."
            };
        }

        var mappedData = apiResponse.Data
            .Where(item => !string.IsNullOrWhiteSpace(item.Symbol)
                && item.Symbol!.EndsWith("_USDT", StringComparison.OrdinalIgnoreCase))
            .Select(item => new MexcContractFundingRateDto
            {
                Symbol = item.Symbol!,
                FundingRate = item.FundingRate,
                MaxFundingRate = item.MaxFundingRate,
                MinFundingRate = item.MinFundingRate,
                CollectCycle = item.CollectCycle,
                NextSettleTime = item.NextSettleTime,
                Timestamp = item.Timestamp
            })
            .ToList();

        return new MexcContractFundingRatesResponse
        {
            Data = mappedData,
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalMexcFundingRateApiResponse
    {
        public bool Success { get; init; }
        public int Code { get; init; }
        public List<InternalMexcFundingRateItem>? Data { get; init; }
    }

    private sealed record InternalMexcFundingRateItem
    {
        public string? Symbol { get; init; }
        public decimal FundingRate { get; init; }
        public decimal MaxFundingRate { get; init; }
        public decimal MinFundingRate { get; init; }
        public int CollectCycle { get; init; }
        public long NextSettleTime { get; init; }
        public long Timestamp { get; init; }
    }
}
