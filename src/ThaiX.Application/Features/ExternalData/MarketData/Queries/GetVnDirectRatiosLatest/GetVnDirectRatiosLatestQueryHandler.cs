using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectRatiosLatest;

/// <summary>
/// Handler for GetVnDirectRatiosLatestQuery.
/// Retrieves VnDirect latest ratios by stock code via proxy.
/// </summary>
public sealed class GetVnDirectRatiosLatestQueryHandler
    : IRequestHandler<GetVnDirectRatiosLatestQuery, VnDirectRatiosLatestResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetVnDirectRatiosLatestQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<VnDirectRatiosLatestResponse> Handle(
        GetVnDirectRatiosLatestQuery request,
        CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (code.Length is < 3 or > 20 || !code.All(char.IsLetterOrDigit))
        {
            return new VnDirectRatiosLatestResponse
            {
                Code = code,
                Data = [],
                Success = false,
                Message = "Invalid code. Use only letters/digits, length 3-20."
            };
        }

        var apiResponse = await _externalDataService
            .GetVnDirectRatiosLatestAsync<InternalVnDirectRatiosLatestResponse>(code, cancellationToken);

        if (apiResponse?.Data is null)
        {
            return new VnDirectRatiosLatestResponse
            {
                Code = code,
                Data = [],
                Success = false,
                Message = "Failed to retrieve VnDirect latest ratios from external API."
            };
        }

        var mappedData = apiResponse.Data
            .Where(x => !string.IsNullOrWhiteSpace(x.RatioCode))
            .Select(x => new VnDirectRatioItemDto
            {
                RatioCode = x.RatioCode,
                Value = x.Value
            })
            .ToList();

        return new VnDirectRatiosLatestResponse
        {
            Code = code,
            Data = mappedData,
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalVnDirectRatiosLatestResponse
    {
        public List<InternalVnDirectRatioItemDto> Data { get; init; } = [];
    }

    private sealed record InternalVnDirectRatioItemDto
    {
        public string RatioCode { get; init; } = string.Empty;
        public decimal Value { get; init; }
    }
}
