using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetStockPriceHistory;

/// <summary>
/// Handler for GetStockPriceHistoryQuery.
/// Retrieves stock price history from CafeF via IExternalDataService.
/// </summary>
public sealed class GetStockPriceHistoryQueryHandler
    : IRequestHandler<GetStockPriceHistoryQuery, StockPriceHistoryResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetStockPriceHistoryQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<StockPriceHistoryResponse> Handle(
        GetStockPriceHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();

        if (symbol.Length is < 1 or > 10 || !symbol.All(char.IsLetterOrDigit))
        {
            return new StockPriceHistoryResponse
            {
                Symbol = symbol,
                Data = [],
                Success = false,
                Message = "Invalid symbol. Use only letters/digits, max length 10."
            };
        }

        var apiResult = await _externalDataService
            .GetStockPriceHistoryAsync<List<InternalStockPriceHistoryItem>?>(symbol, cancellationToken);

        if (apiResult is null || apiResult.Count == 0)
        {
            return new StockPriceHistoryResponse
            {
                Symbol = symbol,
                Data = [],
                Success = false,
                Message = "Failed to retrieve stock price history from external API."
            };
        }

        var mappedData = apiResult
            .Select(item => new StockPriceHistoryItemDto
            {
                Symbol = item.Symbol ?? symbol,
                TradeDate = item.TradeDate,
                BasicPrice = item.BasicPrice,
                OpenPrice = item.OpenPrice,
                HighPrice = item.HighPrice,
                LowPrice = item.LowPrice,
                ClosePrice = item.ClosePrice,
                Volume = item.Volume,
                TotalValue = item.TotalValue
            })
            .ToList();

        return new StockPriceHistoryResponse
        {
            Symbol = symbol,
            Data = mappedData,
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalStockPriceHistoryItem
    {
        public string? Symbol { get; init; }
        public DateTime TradeDate { get; init; }
        public decimal BasicPrice { get; init; }
        public decimal OpenPrice { get; init; }
        public decimal HighPrice { get; init; }
        public decimal LowPrice { get; init; }
        public decimal ClosePrice { get; init; }
        public decimal Volume { get; init; }
        public decimal TotalValue { get; init; }
    }
}
