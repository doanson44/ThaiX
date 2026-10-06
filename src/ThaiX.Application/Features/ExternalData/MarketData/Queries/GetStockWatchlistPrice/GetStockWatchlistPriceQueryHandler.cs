using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetStockWatchlistPrice;

/// <summary>
/// Handler for GetStockWatchlistPriceQuery.
/// Retrieves current symbol snapshot from CafeF watchlist endpoint.
/// </summary>
public sealed class GetStockWatchlistPriceQueryHandler
    : IRequestHandler<GetStockWatchlistPriceQuery, StockWatchlistPriceResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetStockWatchlistPriceQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<StockWatchlistPriceResponse> Handle(
        GetStockWatchlistPriceQuery request,
        CancellationToken cancellationToken)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();

        if (symbol.Length is < 1 or > 10 || !symbol.All(char.IsLetterOrDigit))
        {
            return new StockWatchlistPriceResponse
            {
                Symbol = symbol,
                Success = false,
                Message = "Invalid symbol. Use only letters/digits, max length 10."
            };
        }

        var apiResult = await _externalDataService
            .GetStockWatchlistPriceAsync<InternalApiResponse?>(symbol, cancellationToken);

        var value = apiResult?.Data?.Value;
        if (apiResult is null || !apiResult.Succeeded || value is null)
        {
            return new StockWatchlistPriceResponse
            {
                Symbol = symbol,
                Success = false,
                Message = "Failed to retrieve stock watchlist price from external API."
            };
        }

        return new StockWatchlistPriceResponse
        {
            Symbol = value.Symbol ?? symbol,
            LastTradeDate = value.LastTradeDate,
            Price = value.Price,
            RefPrice = value.RefPrice,
            FloorPrice = value.FloorPrice,
            CeilingPrice = value.CeilingPrice,
            Volume = value.Volume,
            HighPrice = value.HighPrice,
            LowPrice = value.LowPrice,
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalApiResponse
    {
        public bool Succeeded { get; init; }
        public InternalData? Data { get; init; }
    }

    private sealed record InternalData
    {
        public InternalValue? Value { get; init; }
    }

    private sealed record InternalValue
    {
        public string? Symbol { get; init; }
        public DateTimeOffset? LastTradeDate { get; init; }
        public decimal Price { get; init; }
        public decimal RefPrice { get; init; }
        public decimal FloorPrice { get; init; }
        public decimal CeilingPrice { get; init; }
        public decimal Volume { get; init; }
        public decimal HighPrice { get; init; }
        public decimal LowPrice { get; init; }
    }
}
