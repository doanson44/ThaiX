using Microsoft.JSInterop;

namespace ThaiX.Client.Services.Auth;

/// <summary>
/// Stores JWT token in localStorage with in-memory cache.
/// Registered as Scoped — in WASM there is one instance per app lifetime,
/// in SSR/Blazor Server there is one instance per circuit (per user).
/// </summary>
public sealed class AuthTokenStore
{
    private const string TokenStorageKey = "AuthToken";
    private readonly IJSRuntime _jsRuntime;

    private string? _cachedToken;
    private bool _initialized;

    public AuthTokenStore(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async ValueTask<string?> GetTokenAsync()
    {
        if (_initialized)
        {
            return _cachedToken;
        }

        _cachedToken = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenStorageKey);
        _initialized = true;
        return _cachedToken;
    }

    public async ValueTask SetTokenAsync(string token)
    {
        _cachedToken = token;
        _initialized = true;
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenStorageKey, token);
    }

    public async ValueTask ClearTokenAsync()
    {
        _cachedToken = null;
        _initialized = true;
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenStorageKey);
    }
}
