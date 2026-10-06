using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectEvents;

/// <summary>
/// Handler for GetVnDirectEventsQuery.
/// Retrieves VnDirect stock events from external API via proxy.
/// </summary>
public sealed class GetVnDirectEventsQueryHandler
    : IRequestHandler<GetVnDirectEventsQuery, VnDirectEventsResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetVnDirectEventsQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<VnDirectEventsResponse> Handle(
        GetVnDirectEventsQuery request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .GetVnDirectEventsAsync<InternalVnDirectEventsResponse>(cancellationToken);

        if (apiResponse?.Data is null)
        {
            return new VnDirectEventsResponse
            {
                Data = [],
                Success = false,
                Message = "Failed to retrieve VnDirect events from external API."
            };
        }

        var mappedData = apiResponse.Data
            .Select(x => new VnDirectEventDto
            {
                Id = x.Id,
                Code = x.Code,
                Group = x.Group,
                Type = x.Type,
                NewsId = x.NewsId,
                TypeDesc = x.TypeDesc,
                Note = x.Note,
                DisclosureDate = x.DisclosureDate,
                EffectiveDate = x.EffectiveDate,
                Locale = x.Locale
            })
            .ToList();

        return new VnDirectEventsResponse
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

    private sealed record InternalVnDirectEventsResponse
    {
        public int CurrentPage { get; init; }
        public int Size { get; init; }
        public int TotalElements { get; init; }
        public int TotalPages { get; init; }
        public List<InternalVnDirectEventDto> Data { get; init; } = [];
    }

    private sealed record InternalVnDirectEventDto
    {
        public string Id { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public string Group { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public decimal NewsId { get; init; }
        public string TypeDesc { get; init; } = string.Empty;
        public string Note { get; init; } = string.Empty;
        public string DisclosureDate { get; init; } = string.Empty;
        public string EffectiveDate { get; init; } = string.Empty;
        public string Locale { get; init; } = string.Empty;
    }
}
