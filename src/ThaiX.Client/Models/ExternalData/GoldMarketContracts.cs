namespace ThaiX.Client.Models.ExternalData;

public sealed record MexcGoldAnalysisResponseDto
{
    public string Symbol { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal BopPrice { get; init; }
    public decimal Change { get; init; }
    public decimal ChangePct { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Volume { get; init; }
    public decimal QuoteVolume { get; init; }
    public string LastUpdated { get; init; } = string.Empty;
    public string? Message { get; init; }
    public bool Success { get; init; }
}
