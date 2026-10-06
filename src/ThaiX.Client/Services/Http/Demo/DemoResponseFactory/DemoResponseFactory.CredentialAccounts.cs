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
    private HttpResponseMessage HandleCredentialAccounts(
        string method, string path, string query, int pageNumber, int pageSize)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        Guid entityId = default;
        var hasId = segments.Length >= 3 && Guid.TryParse(segments[2], out entityId);
        var action = segments.Length >= 4 ? segments[3].ToLowerInvariant() : string.Empty;

        if (hasId && action is "password" && segments.Length >= 5 &&
            (segments[4].Equals("view", StringComparison.OrdinalIgnoreCase) ||
             segments[4].Equals("copy", StringComparison.OrdinalIgnoreCase)) &&
            method == "POST")
        {
            return _store.WithLock(() =>
            {
                var item = _store.CredentialAccounts.FirstOrDefault(x => x.Id == entityId)
                           ?? _store.CredentialAccounts[0];
                return DemoEnvelope.SuccessData(new CredentialAccountPasswordDto
                {
                    Id = item.Id,
                    Username = item.Username,
                    Password = "DemoP@ssw0rd!"
                });
            });
        }

        if (hasId && action.Equals("audit-logs", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            return _store.WithLock(() =>
            {
                var rows = _store.CredentialAudits.Where(a => a.CredentialAccountId == entityId).ToList();
                if (rows.Count == 0 && _store.CredentialAccounts.Count > 0)
                {
                    rows = _store.CredentialAudits
                        .Where(a => a.CredentialAccountId == _store.CredentialAccounts[0].Id)
                        .ToList();
                }

                return DemoEnvelope.Paged(rows, pageNumber, pageSize);
            });
        }

        if (method == "GET" && !hasId)
        {
            return _store.WithLock(() =>
            {
                var search = GetSearchTerm(query);
                IEnumerable<CredentialAccountDto> rows = _store.CredentialAccounts;
                if (!string.IsNullOrWhiteSpace(search))
                    rows = rows.Where(x =>
                        x.Username.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (x.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
                return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
            });
        }

        if (method == "GET" && hasId)
        {
            return _store.WithLock(() =>
            {
                var item = _store.CredentialAccounts.FirstOrDefault(x => x.Id == entityId)
                           ?? _store.CredentialAccounts[0];
                return DemoEnvelope.SuccessData(new CredentialAccountDetailDto
                {
                    Id = item.Id,
                    Username = item.Username,
                    Description = item.Description,
                    IsUsed = item.IsUsed,
                    UsedAt = item.UsedAt,
                    UsedBy = item.UsedBy,
                    UsageCount = item.UsageCount,
                    LastUsedAgo = item.LastUsedAgo,
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = item.UsedAt
                });
            });
        }

        if (method == "POST" && !hasId)
            return DemoEnvelope.SuccessData(Guid.NewGuid());

        return DemoEnvelope.Success();
    }

}
