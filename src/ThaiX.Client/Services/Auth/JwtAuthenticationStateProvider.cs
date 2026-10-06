using Microsoft.AspNetCore.Components.Authorization;
using System.Globalization;
using System.Security.Claims;

namespace ThaiX.Client.Services.Auth;

public sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());
    private readonly AuthTokenStore _tokenStore;

    public JwtAuthenticationStateProvider(AuthTokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenStore.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(Anonymous);
        }

        var claims = JwtClaimsParser.ParseClaims(token);
        if (claims.Count == 0 || IsExpired(claims))
        {
            await _tokenStore.ClearTokenAsync();
            return new AuthenticationState(Anonymous);
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task MarkUserAsAuthenticatedAsync(string token)
    {
        await _tokenStore.SetTokenAsync(token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        await _tokenStore.ClearTokenAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }

    public void NotifyStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static bool IsExpired(IReadOnlyCollection<Claim> claims)
    {
        var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (!long.TryParse(exp, NumberStyles.Integer, CultureInfo.InvariantCulture, out var epochSeconds))
        {
            return false;
        }

        var expiry = DateTimeOffset.FromUnixTimeSeconds(epochSeconds);
        return expiry <= DateTimeOffset.UtcNow;
    }
}
