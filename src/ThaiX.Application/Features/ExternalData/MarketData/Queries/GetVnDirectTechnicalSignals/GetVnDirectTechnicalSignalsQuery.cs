using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectTechnicalSignals;

/// <summary>
/// Query to retrieve VnDirect technical signals.
/// Default strategy is cipLong when not provided.
/// </summary>
public sealed record GetVnDirectTechnicalSignalsQuery : IAppQuery<VnDirectTechnicalSignalsResponse>
{
    public string? Strategy { get; init; }
}

/// <summary>
/// Response containing VnDirect technical signals data.
/// </summary>
public sealed record VnDirectTechnicalSignalsResponse
{
    public int CurrentPage { get; init; }
    public int Size { get; init; }
    public int TotalElements { get; init; }
    public int TotalPages { get; init; }
    public List<VnDirectTechnicalSignalDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// VnDirect technical signal item.
/// </summary>
public sealed record VnDirectTechnicalSignalDto
{
    public string Code { get; init; } = string.Empty;
    public string TradingDate { get; init; } = string.Empty;
    public string Time { get; init; } = string.Empty;
    public string Strategy { get; init; } = string.Empty;
    public string TotalSignal { get; init; } = string.Empty;
    public List<VnDirectTechnicalIndicatorDto> Indicators { get; init; } = [];
}

/// <summary>
/// VnDirect technical indicator details.
/// </summary>
public sealed record VnDirectTechnicalIndicatorDto
{
    public string Indicator { get; init; } = string.Empty;
    public string Period { get; init; } = string.Empty;
    public string IndicatorName { get; init; } = string.Empty;
    public string Signal { get; init; } = string.Empty;
    public decimal LatestValue { get; init; }
}
