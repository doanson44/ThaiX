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

        var hasTimeRange = request.StartTime.HasValue || request.EndTime.HasValue;
        if (hasTimeRange &&
            (!request.StartTime.HasValue || !request.EndTime.HasValue ||
             request.StartTime.Value <= 0 || request.EndTime.Value <= 0 ||
             request.StartTime.Value > request.EndTime.Value))
        {
            return Failure(symbol, request.PageNum, request.PageSize,
                "Invalid time range. StartTime and EndTime must both be positive Unix timestamps in milliseconds, and StartTime must not exceed EndTime.");
        }

        if (request.PageNum < 1 || request.PageSize is < 1 or > 1000)
        {
            return Failure(symbol, request.PageNum, request.PageSize,
                "Invalid pagination. PageNum must be at least 1 and PageSize must be between 1 and 1000.");
        }

        var apiPageSize = hasTimeRange ? 1000 : request.PageSize;
        var apiPageNum = hasTimeRange ? 1 : request.PageNum;
        var apiResponse = await _externalDataService.GetMexcContractFundingRateHistoryAsync<InternalMexcFundingRateHistoryApiResponse>(
            symbol, apiPageNum, apiPageSize, cancellationToken);

        if (apiResponse is null || !apiResponse.Success || apiResponse.Code != 0 ||
            apiResponse.Data is null || apiResponse.Data.ResultList is null)
        {
            return Failure(symbol, request.PageNum, request.PageSize,
                "Failed to retrieve MEXC contract funding rate history from external API.");
        }

        var history = new List<InternalMexcFundingRateHistoryItem>();
        var currentPage = apiResponse.Data;
        var pageNumber = apiPageNum;

        while (true)
        {
            var pageItems = currentPage.ResultList ?? [];
            history.AddRange(pageItems.Where(item => !string.IsNullOrWhiteSpace(item.Symbol)));

            if (!hasTimeRange || pageNumber >= currentPage.TotalPage ||
                (pageItems.Count > 0 && pageItems.Max(item => item.SettleTime) < request.StartTime!.Value))
            {
                break;
            }

            pageNumber++;
            var nextPage = await _externalDataService.GetMexcContractFundingRateHistoryAsync<InternalMexcFundingRateHistoryApiResponse>(
                symbol, pageNumber, apiPageSize, cancellationToken);

            if (nextPage is null || !nextPage.Success || nextPage.Code != 0 ||
                nextPage.Data is null || nextPage.Data.ResultList is null)
            {
                return Failure(symbol, request.PageNum, request.PageSize,
                    "Failed to retrieve MEXC contract funding rate history from external API.");
            }

            currentPage = nextPage.Data;
        }

        var mappedData = history
            .Where(item => !hasTimeRange ||
                (item.SettleTime >= request.StartTime!.Value && item.SettleTime <= request.EndTime!.Value))
            .OrderBy(item => item.SettleTime)
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
            PageNum = hasTimeRange ? 1 : request.PageNum,
            PageSize = apiPageSize,
            TotalCount = hasTimeRange ? mappedData.Count : currentPage.TotalCount,
            TotalPage = hasTimeRange ? 1 : currentPage.TotalPage,
            CurrentPage = hasTimeRange ? 1 : currentPage.CurrentPage,
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
