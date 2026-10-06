using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ThaiX.Client.Models.Ai;
using ThaiX.Client.Models.ApiClients;
using ThaiX.Client.Models.AssetPositions;
using ThaiX.Client.Models.Auth;
using ThaiX.Client.Models.Blog;
using ThaiX.Client.Models.Contacts;
using ThaiX.Client.Models.CredentialAccounts;
using ThaiX.Client.Models.ExpenseTracker;
using ThaiX.Client.Models.ExternalData;
using ThaiX.Client.Models.JsonBins;
using ThaiX.Client.Models.Lottery;
using ThaiX.Client.Models.MasterData;
using ThaiX.Client.Models.MarketScanner;
using ThaiX.Client.Models.Notes;
using ThaiX.Client.Models.Notifications;
using ThaiX.Client.Models.Portfolios;
using ThaiX.Client.Models.PriceAlerts;
using ThaiX.Client.Models.Resumes;
using ThaiX.Client.Models.Slack;
using ThaiX.Client.Models.Trading;
using ThaiX.Client.Models.Users;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Notifications;


namespace ThaiX.Client.Services.Http.Demo;

public sealed partial class DemoResponseFactory
{
    private async Task<HttpResponseMessage> HandleApiClientsAsync(
        string method,
        string path,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (path.Contains("available-scopes", StringComparison.OrdinalIgnoreCase))
            return DemoEnvelope.SuccessData(new List<string> { "api.read", "api.write", "api.admin" });

        Guid entityId = default;
        var hasId = segments.Length >= 3 && Guid.TryParse(segments[2], out entityId);
        var action = segments.Length >= 4 ? segments[3].ToLowerInvariant() : string.Empty;

        if (method == "GET" && !hasId)
            return _store.WithLock(() => DemoEnvelope.SuccessData(_store.ApiClients.ToList()));

        if (method == "GET" && hasId)
        {
            return _store.WithLock(() =>
            {
                var item = _store.ApiClients.FirstOrDefault(x => x.Id == entityId) ?? _store.ApiClients[0];
                return DemoEnvelope.SuccessData(item);
            });
        }

        if (method == "POST" && !hasId)
        {
            var body = await ReadJsonAsync<CreateApiClientRequest>(request, cancellationToken);
            return _store.WithLock(() =>
            {
                var id = Guid.NewGuid();
                var created = new CreateApiClientResponse
                {
                    Id = id,
                    ClientId = body?.ClientId ?? $"demo-{id:N}"[..16],
                    Name = body?.Name ?? "Demo Client",
                    Description = body?.Description,
                    IsActive = true,
                    Scopes = body?.Scopes?.ToList() ?? ["api.read"],
                    CreatedAt = DateTime.UtcNow,
                    ClientSecret = "demo-client-secret-" + Guid.NewGuid().ToString("N")[..16]
                };
                _store.ApiClients.Insert(0, created);
                return DemoEnvelope.SuccessData(created);
            });
        }

        if (hasId && action.Equals("regenerate-secret", StringComparison.OrdinalIgnoreCase) && method == "POST")
        {
            return _store.WithLock(() =>
            {
                var item = _store.ApiClients.FirstOrDefault(x => x.Id == entityId) ?? _store.ApiClients[0];
                return DemoEnvelope.SuccessData(new RegenerateApiClientSecretResponse
                {
                    Id = item.Id,
                    ClientId = item.ClientId,
                    ClientSecret = "demo-rotated-secret-" + Guid.NewGuid().ToString("N")[..16]
                });
            });
        }

        if (hasId && method == "POST" && action is "activate" or "deactivate")
        {
            return _store.WithLock(() =>
            {
                var idx = _store.ApiClients.FindIndex(x => x.Id == entityId);
                if (idx < 0) idx = 0;
                var existing = _store.ApiClients[idx];
                var updated = existing with { IsActive = action == "activate", UpdatedAt = DateTime.UtcNow };
                _store.ApiClients[idx] = updated;
                return DemoEnvelope.SuccessData(updated);
            });
        }

        if (hasId && (action.Equals("scopes", StringComparison.OrdinalIgnoreCase) || method == "PUT"))
        {
            return _store.WithLock(() =>
            {
                var item = _store.ApiClients.FirstOrDefault(x => x.Id == entityId) ?? _store.ApiClients[0];
                return DemoEnvelope.SuccessData(item);
            });
        }

        if (method == "DELETE" && hasId)
        {
            _store.WithLock(() => _store.ApiClients.RemoveAll(x => x.Id == entityId));
            return DemoEnvelope.Success();
        }

        return DemoEnvelope.Success();
    }

}
