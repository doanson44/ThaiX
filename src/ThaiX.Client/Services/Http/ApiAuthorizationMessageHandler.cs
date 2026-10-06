using System.Globalization;
using System.Net.Http.Headers;
using ThaiX.Client.Services.Auth;

namespace ThaiX.Client.Services.Http;

/// <summary>
/// Adds Authorization and Accept-Language headers to API requests.
/// </summary>
public sealed class ApiAuthorizationMessageHandler : DelegatingHandler
{
    private readonly AuthTokenStore _authTokenStore;

    public ApiAuthorizationMessageHandler(AuthTokenStore authTokenStore)
    {
        _authTokenStore = authTokenStore;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _authTokenStore.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var culture = CultureInfo.CurrentUICulture.Name;
        request.Headers.AcceptLanguage.Clear();
        request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(culture));

        return await base.SendAsync(request, cancellationToken);
    }
}
