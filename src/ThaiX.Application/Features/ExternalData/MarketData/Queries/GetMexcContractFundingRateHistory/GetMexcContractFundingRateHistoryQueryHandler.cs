using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractFundingRateHistory;

/// <summary>
/// Handler for GetMexcContractFundingRateHistoryQuery.
/// </summary>
public sealed class GetMexcContractFundingRateHistoryQueryHandler
    : IRequestHandler<GetMexcContractFundingRateHistoryQuery, MexcContractFundingRateHistoryResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetMexcContractFundingRateHistoryQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<MexcContractFundingRateHistoryResponse> Handle(
        GetMexcContractFundingRateHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();
        if (symbol.Length is < 3 or > 30 || !symbol.All(c => char.IsLetterOrDigit(c) || c == '_'))
        {
            return Failure(symbol, request.PageNum, request.PageSize,
                "Invalid symbol. Use letters/digits/underscore, length 3-30.");
        }

        if (request.PageNum < 1 || request.PageSize is < 1 or > 1000)
        {
            return Failure(symbol, request.PageNum, request.PageSize,
                "Invalid pagination. PageNum must be at least 1 and PageSize must be between 1 and 1000.");
        }

        var apiResponse = await _externalDataService.GetMexcContractFundingRateHistoryAsync<InternalMexcFundingRateHistoryApiResponse>(
            symbol, request.PageNum, request.PageSize, cancellationToken);

        if (apiResponse is null || !apiResponse.Success || apiResponse.Code != 0 ||
            apiResponse.Data is null || apiResponse.Data.ResultList is null)
        {
            return Failure(symbol, request.PageNum, request.PageSize,
                "Failed to retrieve MEXC contract funding rate history from external API.");
        }

        var mappedData = apiResponse.Data.ResultList
            .Where(item => !string.IsNullOrWhiteSpace(item.Symbol))
            .Select(item => new MexcContractFundingRateHistoryItemDto
            {
                Symbol = item.Symbol!,
                FundingRate = item.FundingRate,
                SettleTime = item.SettleTime,
                CollectCycle = item.CollectCycle
            })
            .ToList();

        return new MexcContractFundingRateHistoryResponse
        {
            Symbol = symbol,
            PageNum = request.PageNum,
            PageSize = apiResponse.Data.PageSize,
            TotalCount = apiResponse.Data.TotalCount,
            TotalPage = apiResponse.Data.TotalPage,
            CurrentPage = apiResponse.Data.CurrentPage,
            Data = mappedData,
            Success = true,
            Message = "Success"
        };
    }

    private static MexcContractFundingRateHistoryResponse Failure(
        string symbol,
        int pageNum,
        int pageSize,
        string message)
        => new()
        {
            Symbol = symbol,
            PageNum = pageNum,
            PageSize = pageSize,
            Data = [],
            Success = false,
            Message = message
        };

    private sealed record InternalMexcFundingRateHistoryApiResponse
    {
        public bool Success { get; init; }
        public int Code { get; init; }
        public InternalMexcFundingRateHistoryData? Data { get; init; }
    }

    private sealed record InternalMexcFundingRateHistoryData
    {
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPage { get; init; }
        public int CurrentPage { get; init; }
        public List<InternalMexcFundingRateHistoryItem>? ResultList { get; init; }
    }

    private sealed record InternalMexcFundingRateHistoryItem
    {
        public string? Symbol { get; init; }
        public decimal FundingRate { get; init; }
        public long SettleTime { get; init; }
        public int CollectCycle { get; init; }
    }
}
