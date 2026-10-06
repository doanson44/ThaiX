using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotDepth;

/// <summary>
/// Handler for GetMexcSpotDepthQuery.
/// Retrieves MEXC spot depth by symbol with fixed limit=5000.
/// </summary>
public sealed class GetMexcSpotDepthQueryHandler
    : IRequestHandler<GetMexcSpotDepthQuery, MexcSpotDepthResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetMexcSpotDepthQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<MexcSpotDepthResponse> Handle(
        GetMexcSpotDepthQuery request,
        CancellationToken cancellationToken)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();
        if (symbol.Length is < 3 or > 20 || !symbol.All(char.IsLetterOrDigit))
        {
            return new MexcSpotDepthResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Invalid symbol. Use only letters/digits, length 3-20."
            };
        }

        var apiResponse = await _externalDataService
            .GetMexcSpotDepthAsync<InternalMexcSpotDepthResponse>(symbol, cancellationToken);

        if (apiResponse is null || apiResponse.Bids is null || apiResponse.Asks is null)
        {
            return new MexcSpotDepthResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Failed to retrieve MEXC spot depth from external API."
            };
        }

        return new MexcSpotDepthResponse
        {
            Symbol = symbol,
            Data = new MexcSpotDepthDataDto
            {
                LastUpdateId = apiResponse.LastUpdateId,
                Timestamp = apiResponse.Timestamp,
                Bids = apiResponse.Bids,
                Asks = apiResponse.Asks
            },
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalMexcSpotDepthResponse
    {
        public long LastUpdateId { get; init; }
        public long? Timestamp { get; init; }
        public List<List<string>>? Bids { get; init; }
        public List<List<string>>? Asks { get; init; }
    }
}
