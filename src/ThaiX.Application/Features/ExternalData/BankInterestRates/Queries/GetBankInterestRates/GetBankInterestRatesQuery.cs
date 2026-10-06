using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.BankInterestRates.Queries.GetBankInterestRates;

/// <summary>
/// Query to retrieve bank interest rates from CafeF external API.
/// Data is cached according to endpoint configuration.
/// </summary>
public sealed record GetBankInterestRatesQuery : IAppQuery<BankInterestRatesResponse>;

/// <summary>
/// Response containing bank interest rates data from CafeF.
/// </summary>
public sealed record BankInterestRatesResponse
{
    public required List<BankInterestRateGridItemDto> Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Bank interest rate information.
/// </summary>
public sealed record BankInterestRateDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Symbol { get; init; }
    public required string Icon { get; init; }
    public required List<InterestRateDetailDto> InterestRates { get; init; }
}

/// <summary>
/// Interest rate detail for specific deposit term.
/// </summary>
public sealed record InterestRateDetailDto
{
    /// <summary>Deposit term in months (0 for non-term/demand deposit).</summary>
    public int Deposit { get; init; }

    /// <summary>Interest rate value (percentage). Null if not available for this term.</summary>
    public decimal? Value { get; init; }
}

/// <summary>
/// External API response containing raw bank interest rates data from CafeF.
/// This is the actual structure returned by the external API.
/// </summary>
public sealed record ExternalBankInterestRatesResponse
{
    public required List<BankInterestRateDto> Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Bank interest rate grid item for UI display.
/// </summary>
public sealed record BankInterestRateGridItemDto
{
    public required string BankName { get; init; }
    public required string Symbol { get; init; }
    public required string IconUrl { get; init; }
    public decimal? Month1 { get; init; }
    public decimal? Month3 { get; init; }
    public decimal? Month6 { get; init; }
    public decimal? Month9 { get; init; }
    public decimal? Month12 { get; init; }
    public decimal? Month18 { get; init; }
    public decimal? Month24 { get; init; }
}
