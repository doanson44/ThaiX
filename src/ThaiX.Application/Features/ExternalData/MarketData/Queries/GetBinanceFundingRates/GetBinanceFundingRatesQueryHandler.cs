using MediatR;
using System.Globalization;
using System.Text.Json;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceFundingRates;

/// <summary>
/// Handler for GetBinanceFundingRatesQuery.
/// Retrieves Binance futures funding rate history.
/// </summary>
public sealed class GetBinanceFundingRatesQueryHandler
    : IRequestHandler<GetBinanceFundingRatesQuery, BinanceFundingRatesResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetBinanceFundingRatesQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<BinanceFundingRatesResponse> Handle(
        GetBinanceFundingRatesQuery request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .GetBinanceFundingRatesAsync<JsonElement>(cancellationToken);

        if (apiResponse.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return new BinanceFundingRatesResponse
            {
                Data = new List<BinanceFundingRateDto>(),
                Success = false,
                Message = "Failed to retrieve Binance funding rates from external API."
            };
        }

        var items = new List<BinanceFundingRateDto>();

        if (apiResponse.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in apiResponse.EnumerateArray())
            {
                if (TryMapFundingRate(element, out var dto))
                {
                    items.Add(dto);
                }
            }
        }
        else if (apiResponse.ValueKind == JsonValueKind.Object)
        {
            if (TryMapFundingRate(apiResponse, out var dto))
            {
                items.Add(dto);
            }
        }

        if (items.Count == 0)
        {
            return new BinanceFundingRatesResponse
            {
                Data = new List<BinanceFundingRateDto>(),
                Success = false,
                Message = "No funding rate data returned from Binance."
            };
        }

        var usdtItems = items
            .Where(x => x.Symbol.EndsWith("USDT", StringComparison.OrdinalIgnoreCase))
            .ToList();

        return new BinanceFundingRatesResponse
        {
            Data = usdtItems,
            Success = true,
            Message = "Success"
        };
    }

    private static bool TryMapFundingRate(JsonElement element, out BinanceFundingRateDto dto)
    {
        dto = default!;

        if (element.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!TryReadString(element, "symbol", out var symbol) ||
            !TryReadDecimal(element, "fundingRate", out var fundingRate) ||
            !TryReadInt64(element, "fundingTime", out var fundingTime))
        {
            return false;
        }

        dto = new BinanceFundingRateDto
        {
            Symbol = symbol!,
            FundingRate = fundingRate,
            FundingTime = fundingTime,
            MarkPrice = ReadDecimalOptional(element, "markPrice")
        };

        return true;
    }

    private static bool TryReadString(JsonElement element, string propertyName, out string? value)
    {
        value = null;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        value = property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.ToString(),
            _ => null
        };

        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool TryReadDecimal(JsonElement element, string propertyName, out decimal value)
    {
        value = 0;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number)
        {
            return property.TryGetDecimal(out value);
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            var text = property.GetString();
            return decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        return false;
    }

    private static decimal? ReadDecimalOptional(JsonElement element, string propertyName)
    {
        return TryReadDecimal(element, propertyName, out var value) ? value : null;
    }

    private static bool TryReadInt64(JsonElement element, string propertyName, out long value)
    {
        value = 0;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number)
        {
            return property.TryGetInt64(out value);
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            var text = property.GetString();
            return long.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        return false;
    }
}
