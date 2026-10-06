using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectEvents;

/// <summary>
/// Query to retrieve VnDirect stock events list.
/// </summary>
public sealed record GetVnDirectEventsQuery : IAppQuery<VnDirectEventsResponse>;

/// <summary>
/// Response containing VnDirect stock events data.
/// </summary>
public sealed record VnDirectEventsResponse
{
    public int CurrentPage { get; init; }
    public int Size { get; init; }
    public int TotalElements { get; init; }
    public int TotalPages { get; init; }
    public List<VnDirectEventDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// VnDirect event item.
/// </summary>
public sealed record VnDirectEventDto
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
