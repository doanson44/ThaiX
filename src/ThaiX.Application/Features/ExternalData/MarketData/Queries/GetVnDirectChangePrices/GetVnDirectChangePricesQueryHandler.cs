using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectChangePrices;

/// <summary>
/// Handler for GetVnDirectChangePricesQuery.
/// Retrieves VnDirect change prices for market indices from external API via proxy.
/// </summary>
public sealed class GetVnDirectChangePricesQueryHandler
    : IRequestHandler<GetVnDirectChangePricesQuery, VnDirectChangePricesResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetVnDirectChangePricesQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<VnDirectChangePricesResponse> Handle(
        GetVnDirectChangePricesQuery request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .GetVnDirectChangePricesAsync<InternalChangePricesResponse>(cancellationToken);

        if (apiResponse?.Data is null)
        {
            return new VnDirectChangePricesResponse
            {
                Data = [],
                Success = false,
                Message = "Failed to retrieve VnDirect change prices from external API."
            };
        }

        var mappedData = apiResponse.Data
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .Select(x => new VnDirectChangePriceDto
            {
                Code = x.Code,
                Name = x.Name,
                Type = x.Type,
                Period = x.Period,
                Price = x.Price,
                BopPrice = x.BopPrice,
                Change = x.Change,
                ChangePct = x.ChangePct,
                LastUpdated = x.LastUpdated
            })
            .ToList();

        return new VnDirectChangePricesResponse
        {
            CurrentPage = apiResponse.CurrentPage,
            Size = apiResponse.Size,
            TotalElements = apiResponse.TotalElements,
            TotalPages = apiResponse.TotalPages,
            Data = mappedData,
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalChangePricesResponse
    {
        public int CurrentPage { get; init; }
        public int Size { get; init; }
        public int TotalElements { get; init; }
        public int TotalPages { get; init; }
        public List<InternalChangePriceDto> Data { get; init; } = [];
    }

    private sealed record InternalChangePriceDto
    {
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string Period { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public decimal BopPrice { get; init; }
        public decimal Change { get; init; }
        public decimal ChangePct { get; init; }
        public string LastUpdated { get; init; } = string.Empty;
    }
}
