using MediatR;
using System.Text.Json.Serialization;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCoinGeckoCoinMarket;

/// <summary>
/// Handler for GetCoinGeckoCoinMarketQuery.
/// Retrieves CoinGecko market data by coin id via IExternalDataService.
/// </summary>
public sealed class GetCoinGeckoCoinMarketQueryHandler
    : IRequestHandler<GetCoinGeckoCoinMarketQuery, CoinGeckoCoinMarketResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetCoinGeckoCoinMarketQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<CoinGeckoCoinMarketResponse> Handle(
        GetCoinGeckoCoinMarketQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CoinId) || request.CoinId.Length > 100)
        {
            return new CoinGeckoCoinMarketResponse
            {
                CoinId = request.CoinId,
                Data = null,
                Success = false,
                Message = "Coin id is invalid."
            };
        }

        var normalizedCoinId = request.CoinId.Trim().ToLowerInvariant();

        var apiResponse = await _externalDataService
            .GetCoinGeckoCoinMarketByIdAsync<List<InternalCoinGeckoCoinMarket>>(normalizedCoinId, cancellationToken);

        if (apiResponse is null)
        {
            return new CoinGeckoCoinMarketResponse
            {
                CoinId = normalizedCoinId,
                Data = null,
                Success = false,
                Message = "Failed to retrieve CoinGecko coin market data from external API."
            };
        }

        var market = apiResponse.FirstOrDefault();
        if (market is null)
        {
            return new CoinGeckoCoinMarketResponse
            {
                CoinId = normalizedCoinId,
                Data = null,
                Success = false,
                Message = "Coin id not found on CoinGecko."
            };
        }

        if (string.IsNullOrWhiteSpace(market.Id) ||
            string.IsNullOrWhiteSpace(market.Symbol) ||
            string.IsNullOrWhiteSpace(market.Name))
        {
            return new CoinGeckoCoinMarketResponse
            {
                CoinId = normalizedCoinId,
                Data = null,
                Success = false,
                Message = "CoinGecko returned incomplete market data."
            };
        }

        return new CoinGeckoCoinMarketResponse
        {
            CoinId = normalizedCoinId,
            Data = new CoinGeckoCoinMarketDto
            {
                Id = market.Id,
                Symbol = market.Symbol,
                Name = market.Name,
                Image = market.Image,
                CurrentPrice = market.CurrentPrice,
                MarketCap = market.MarketCap,
                MarketCapRank = market.MarketCapRank,
                High24H = market.High24H,
                Low24H = market.Low24H,
                PriceChangePercentage24H = market.PriceChangePercentage24H
            },
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalCoinGeckoCoinMarket
    {
        public string Id { get; init; } = string.Empty;
        public string Symbol { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Image { get; init; }

        [property: JsonPropertyName("current_price")]
        public decimal CurrentPrice { get; init; }

        [property: JsonPropertyName("market_cap")]
        public decimal? MarketCap { get; init; }

        [property: JsonPropertyName("market_cap_rank")]
        public int? MarketCapRank { get; init; }

        [property: JsonPropertyName("high_24h")]
        public decimal? High24H { get; init; }

        [property: JsonPropertyName("low_24h")]
        public decimal? Low24H { get; init; }

        [property: JsonPropertyName("price_change_percentage_24h")]
        public decimal? PriceChangePercentage24H { get; init; }
    }
}