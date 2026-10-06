using System.Text.Json.Serialization;
using ThaiX.Application.Common.Enums;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.Banking.Queries.GetVnExpressBankRates;

/// <summary>
/// Query to retrieve bank deposit interest rates of a single channel (online or counter) from VnExpress.
/// Freshness is controlled by the endpoint cache configuration in infrastructure.
/// </summary>
public sealed record GetVnExpressBankRatesQuery : IAppQuery<VnExpressBankRatesResponse>
{
    public BankRateChannel Channel { get; init; } = BankRateChannel.Online;
}

/// <summary>
/// Response containing bank deposit interest rates of the requested channel.
/// </summary>
public sealed record VnExpressBankRatesResponse
{
    public BankRateChannel Channel { get; init; }
    public List<VnExpressBankRateItemDto> Data { get; init; } = [];
    public bool Success { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Deposit interest rates of a single bank. Rates are annual percentages.
/// </summary>
public sealed record VnExpressBankRateItemDto
{
    public required string BankName { get; init; }

    /// <summary>Bank logo published by the source; null when the bank has no logo asset.</summary>
    public string? LogoUrl { get; init; }

    /// <summary>Source note, usually describing rate conditions (e.g. conditional products).</summary>
    public string? Note { get; init; }

    /// <summary>Last update timestamp reported by the source.</summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>Rates for the standard deposit terms, in months. Null means the term is not offered.</summary>
    public decimal? Month1 { get; init; }
    public decimal? Month3 { get; init; }
    public decimal? Month6 { get; init; }
    public decimal? Month9 { get; init; }
    public decimal? Month12 { get; init; }
}

/// <summary>
/// Raw response of the VnExpress bank rate endpoints (https://gw.vnexpress.net/th?types=bank_rate_online|bank_rate_offline).
/// Only one of the two data lists is populated, depending on the requested type.
/// </summary>
public sealed record VnExpressBankRatesApiResponse
{
    public int Code { get; init; }
    public VnExpressBankRatesApiData? Data { get; init; }
}

/// <summary>Channel payload of the VnExpress bank rate response.</summary>
public sealed record VnExpressBankRatesApiData
{
    [JsonPropertyName("bank_rate_online")]
    public List<VnExpressBankRateRow>? BankRateOnline { get; init; }

    [JsonPropertyName("bank_rate_offline")]
    public List<VnExpressBankRateRow>? BankRateOffline { get; init; }
}

/// <summary>
/// One bank row of the VnExpress bank rate response. Rates are annual percentages; 0 means the term is not offered.
/// </summary>
public sealed record VnExpressBankRateRow
{
    [JsonPropertyName("bank")]
    public string? Bank { get; init; }

    [JsonPropertyName("logo_1")]
    public string? Logo1 { get; init; }

    [JsonPropertyName("note")]
    public string? Note { get; init; }

    [JsonPropertyName("rate_1")]
    public decimal Rate1 { get; init; }

    [JsonPropertyName("rate_3")]
    public decimal Rate3 { get; init; }

    [JsonPropertyName("rate_6")]
    public decimal Rate6 { get; init; }

    [JsonPropertyName("rate_9")]
    public decimal Rate9 { get; init; }

    [JsonPropertyName("rate_12")]
    public decimal Rate12 { get; init; }

    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; init; }
}
