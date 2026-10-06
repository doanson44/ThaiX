using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectRatiosLatestByItemCode;

/// <summary>
/// Handler for GetVnDirectRatiosLatestByItemCodeQuery.
/// Retrieves VnDirect latest market ratios by item codes via proxy.
/// </summary>
public sealed class GetVnDirectRatiosLatestByItemCodeQueryHandler
    : IRequestHandler<GetVnDirectRatiosLatestByItemCodeQuery, VnDirectRatiosLatestByItemCodeResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetVnDirectRatiosLatestByItemCodeQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<VnDirectRatiosLatestByItemCodeResponse> Handle(
        GetVnDirectRatiosLatestByItemCodeQuery request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .GetVnDirectRatiosLatestByItemCodeAsync<InternalVnDirectRatiosLatestByItemCodeResponse>(cancellationToken);

        if (apiResponse?.Data is null)
        {
            return new VnDirectRatiosLatestByItemCodeResponse
            {
                Data = [],
                Success = false,
                Message = "Failed to retrieve VnDirect latest ratios by item codes from external API."
            };
        }

        var mappedData = apiResponse.Data
            .Where(x => !string.IsNullOrWhiteSpace(x.ItemCode))
            .Select(x => new VnDirectRatioByItemCodeDto
            {
                Code = x.Code,
                Group = x.Group,
                ReportDate = x.ReportDate,
                ItemCode = x.ItemCode,
                RatioCode = x.RatioCode,
                ItemName = x.ItemName,
                Value = x.Value
            })
            .ToList();

        return new VnDirectRatiosLatestByItemCodeResponse
        {
            Data = mappedData,
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalVnDirectRatiosLatestByItemCodeResponse
    {
        public List<InternalVnDirectRatioByItemCodeDto> Data { get; init; } = [];
    }

    private sealed record InternalVnDirectRatioByItemCodeDto
    {
        public string Code { get; init; } = string.Empty;
        public string Group { get; init; } = string.Empty;
        public string ReportDate { get; init; } = string.Empty;
        public string ItemCode { get; init; } = string.Empty;
        public string RatioCode { get; init; } = string.Empty;
        public string ItemName { get; init; } = string.Empty;
        public decimal Value { get; init; }
    }
}
