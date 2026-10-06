using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractDepth;

/// <summary>
/// Handler for GetMexcContractDepthQuery.
/// Retrieves MEXC contract depth for a symbol.
/// </summary>
public sealed class GetMexcContractDepthQueryHandler
    : IRequestHandler<GetMexcContractDepthQuery, MexcContractDepthResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetMexcContractDepthQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<MexcContractDepthResponse> Handle(
        GetMexcContractDepthQuery request,
        CancellationToken cancellationToken)
    {
        var symbol = request.Symbol.Trim().ToUpperInvariant();
        if (symbol.Length is < 3 or > 30 || !symbol.All(c => char.IsLetterOrDigit(c) || c == '_'))
        {
            return new MexcContractDepthResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Invalid symbol. Use letters/digits/underscore, length 3-30."
            };
        }

        var apiResponse = await _externalDataService
            .GetMexcContractDepthAsync<InternalMexcDepthApiResponse>(symbol, cancellationToken);

        if (apiResponse is null || !apiResponse.Success || apiResponse.Code != 0 || apiResponse.Data is null)
        {
            return new MexcContractDepthResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "Failed to retrieve MEXC contract depth from external API."
            };
        }

        var data = apiResponse.Data;
        if (data.Asks is null || data.Bids is null)
        {
            return new MexcContractDepthResponse
            {
                Symbol = symbol,
                Data = null,
                Success = false,
                Message = "MEXC returned incomplete depth data."
            };
        }

        return new MexcContractDepthResponse
        {
            Symbol = symbol,
            Data = new MexcContractDepthDataDto
            {
                Asks = data.Asks,
                Bids = data.Bids,
                Version = data.Version,
                Timestamp = data.Timestamp
            },
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalMexcDepthApiResponse
    {
        public bool Success { get; init; }
        public int Code { get; init; }
        public InternalMexcDepthData? Data { get; init; }
    }

    private sealed record InternalMexcDepthData
    {
        public List<List<decimal>>? Asks { get; init; }
        public List<List<decimal>>? Bids { get; init; }
        public long Version { get; init; }
        public long Timestamp { get; init; }
    }
}
