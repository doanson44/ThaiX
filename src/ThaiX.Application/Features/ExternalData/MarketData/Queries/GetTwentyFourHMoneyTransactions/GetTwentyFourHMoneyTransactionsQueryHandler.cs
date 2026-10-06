using MediatR;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetTwentyFourHMoneyTransactions;

/// <summary>
/// Handler for GetTwentyFourHMoneyTransactionsQuery.
/// </summary>
public sealed class GetTwentyFourHMoneyTransactionsQueryHandler
    : IRequestHandler<GetTwentyFourHMoneyTransactionsQuery, TwentyFourHMoneyTransactionsResponse>
{
    private static readonly Regex SymbolRegex = new(@"^[A-Za-z0-9]{1,10}$", RegexOptions.Compiled);

    private readonly IExternalDataService _externalDataService;

    public GetTwentyFourHMoneyTransactionsQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<TwentyFourHMoneyTransactionsResponse> Handle(
        GetTwentyFourHMoneyTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Symbol) || !SymbolRegex.IsMatch(request.Symbol))
        {
            return new TwentyFourHMoneyTransactionsResponse
            {
                Data = [],
                Success = false,
                Message = "Invalid stock symbol. Must be 1-10 alphanumeric characters."
            };
        }

        var page = Math.Max(request.Page, 1);
        var perPage = Math.Clamp(request.PerPage, 1, 1000);
        var normalizedSymbol = request.Symbol.Trim().ToUpperInvariant();

        var apiResponse = await _externalDataService
            .GetTwentyFourHMoneyTransactionListSsiAsync<InternalTwentyFourHMoneyTransactionsResponse>(
                normalizedSymbol,
                page,
                perPage,
                cancellationToken);

        if (apiResponse?.Data is null)
        {
            return new TwentyFourHMoneyTransactionsResponse
            {
                Symbol = normalizedSymbol,
                Page = page,
                PerPage = perPage,
                Data = [],
                Success = false,
                Message = "Failed to retrieve 24HMoney transactions from external API."
            };
        }

        return new TwentyFourHMoneyTransactionsResponse
        {
            Symbol = normalizedSymbol,
            Page = page,
            PerPage = perPage,
            Status = apiResponse.Status,
            ExecuteTimeMs = apiResponse.ExecuteTimeMs,
            Data = apiResponse.Data
                .Select(x => new TwentyFourHMoneyTransactionDto
                {
                    Price = x.Price,
                    Change = x.Change,
                    MatchQuantity = x.MatchQuantity,
                    TotalVolume = x.TotalVolume,
                    Time = x.Time,
                    Side = x.Side
                })
                .ToList(),
            Success = apiResponse.Status == 200,
            Message = apiResponse.Message
        };
    }

    private sealed record InternalTwentyFourHMoneyTransactionsResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; init; }

        [JsonPropertyName("status")]
        public int Status { get; init; }

        [JsonPropertyName("data")]
        public List<InternalTwentyFourHMoneyTransactionDto> Data { get; init; } = [];

        [JsonPropertyName("execute_time_ms")]
        public int ExecuteTimeMs { get; init; }
    }

    private sealed record InternalTwentyFourHMoneyTransactionDto
    {
        [JsonPropertyName("price")]
        public decimal Price { get; init; }

        [JsonPropertyName("change")]
        public decimal Change { get; init; }

        [JsonPropertyName("match_qtty")]
        public long MatchQuantity { get; init; }

        [JsonPropertyName("total_vol")]
        public long TotalVolume { get; init; }

        [JsonPropertyName("time")]
        public string Time { get; init; } = string.Empty;

        [JsonPropertyName("side")]
        public string Side { get; init; } = string.Empty;
    }
}
