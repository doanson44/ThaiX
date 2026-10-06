using System.Text.Json;
using Microsoft.JSInterop;
using ThaiX.Client.Models.MarketData;

namespace ThaiX.Client.Services.MarketData;

/// <summary>Persists MEXC socket price-move alert settings in browser localStorage.</summary>
public sealed class MexcSocketPriceMoveAlertStore
{
    public const string SpotStorageKey = "ThaiX.mexc.spot.priceMoveAlert";
    public const string ContractStorageKey = "ThaiX.mexc.contract.priceMoveAlert";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly IJSRuntime _jsRuntime;

    public MexcSocketPriceMoveAlertStore(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<MexcSocketPriceMoveAlertState> LoadAsync(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return new MexcSocketPriceMoveAlertState();
        }

        try
        {
            var raw = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", storageKey);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new MexcSocketPriceMoveAlertState();
            }

            var state = JsonSerializer.Deserialize<MexcSocketPriceMoveAlertState>(raw, JsonOptions);
            return state ?? new MexcSocketPriceMoveAlertState();
        }
        catch
        {
            return new MexcSocketPriceMoveAlertState();
        }
    }

    public async Task SaveAsync(string storageKey, MexcSocketPriceMoveAlertState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return;
        }

        var json = JsonSerializer.Serialize(state, JsonOptions);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", storageKey, json);
    }
}
