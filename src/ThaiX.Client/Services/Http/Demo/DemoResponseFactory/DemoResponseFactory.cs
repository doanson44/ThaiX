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

/// <summary>
/// Maps HTTP method + path to demo <see cref="ApiResponse{T}"/> / <see cref="PagedApiResponse{T}"/> envelopes.
/// Keep <c>DemoEndpointCatalog.md</c> in sync when adding Services REST paths
/// (re-scan: <c>rg -o '"api/[^"]+"' src/ThaiX.Client/Services --glob '*.cs'</c>).
/// </summary>
public sealed partial class DemoResponseFactory
{
    private readonly DemoSessionStore _store;
    private readonly string _adminToken;

    public DemoResponseFactory(DemoSessionStore store)
    {
        _store = store;
        _adminToken = DemoJwtFactory.CreateAdminToken();
    }

    public async Task<HttpResponseMessage> CreateAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        var method = request.Method.Method.ToUpperInvariant();
        var path = GetRelativePath(request.RequestUri);
        var query = request.RequestUri?.Query ?? string.Empty;
        ParsePaging(query, out var pageNumber, out var pageSize);
        EnsureDemoDataForPath(path);

        if (path.Equals("health", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("api/health", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.Text("Healthy (demo)");
        }

        if (path.StartsWith("api/auth", StringComparison.OrdinalIgnoreCase))
        {
            return HandleAuth(method, path);
        }

        if (path.StartsWith("api/account", StringComparison.OrdinalIgnoreCase))
        {
            return HandleAccount(method, path);
        }

        if (path.StartsWith("api/master-data", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleMasterDataAsync(method, path, query, pageNumber, pageSize, request, cancellationToken);
        }

        if (path.StartsWith("api/users", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleUsersAsync(method, path, pageNumber, pageSize, request, cancellationToken);
        }

        if (path.StartsWith("api/contacts", StringComparison.OrdinalIgnoreCase))
        {
            return HandleContacts(method, path, query, pageNumber, pageSize);
        }

        if (path.StartsWith("api/portfolios", StringComparison.OrdinalIgnoreCase))
        {
            return await HandlePortfoliosAsync(method, path, query, pageNumber, pageSize, request, cancellationToken);
        }

        if (path.StartsWith("api/files", StringComparison.OrdinalIgnoreCase))
        {
            return HandleFiles(method, path);
        }

        if (path.StartsWith("api/blog", StringComparison.OrdinalIgnoreCase) &&
            !path.StartsWith("api/blog/ai", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleBlogAsync(method, path, query, pageNumber, pageSize, request, cancellationToken);
        }

        if (path.StartsWith("api/credential-accounts", StringComparison.OrdinalIgnoreCase))
        {
            return HandleCredentialAccounts(method, path, query, pageNumber, pageSize);
        }

        if (path.StartsWith("api/notification-schedules", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleNotificationSchedulesAsync(method, path, query, pageNumber, pageSize, request, cancellationToken);
        }

        if (path.StartsWith("api/api-clients", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleApiClientsAsync(method, path, request, cancellationToken);
        }

        if (path.StartsWith("api/notes", StringComparison.OrdinalIgnoreCase))
        {
            return HandleNotes(method, path, query, pageNumber, pageSize);
        }

        if (path.StartsWith("api/json-bins", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("api/public/json-bins", StringComparison.OrdinalIgnoreCase))
        {
            return HandleJsonBins(method, path, query, pageNumber, pageSize);
        }

        if (path.StartsWith("api/external-data", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("chainbroker", StringComparison.OrdinalIgnoreCase))
        {
            return HandleExternalData(method, path, query, pageNumber, pageSize);
        }

        if (path.StartsWith("api/trading", StringComparison.OrdinalIgnoreCase))
        {
            return HandleTrading(method, path, pageNumber, pageSize);
        }

        if (path.StartsWith("api/lottery", StringComparison.OrdinalIgnoreCase))
        {
            return HandleLottery(method, path);
        }

        if (path.StartsWith("api/resumes", StringComparison.OrdinalIgnoreCase))
        {
            return HandleResumes(method, path);
        }

        if (path.StartsWith("api/ai", StringComparison.OrdinalIgnoreCase))
        {
            return HandleAi(method);
        }

        if (path.StartsWith("api/bot-commands", StringComparison.OrdinalIgnoreCase))
        {
            return HandleBotCommands(method);
        }

        if (path.StartsWith("api/blog/ai", StringComparison.OrdinalIgnoreCase))
        {
            return HandleBlogAi(method, path);
        }

        if (path.StartsWith("api/notifications", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleNotificationsAsync(method, path, request, cancellationToken);
        }

        if (path.StartsWith("api/slack", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("api/telegram", StringComparison.OrdinalIgnoreCase))
        {
            return HandleSlackTelegram(method, path);
        }

        if (path.StartsWith("api/expense-tracker", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleExpenseTrackerAsync(method, path, pageNumber, pageSize, request, cancellationToken);
        }

        if (IsListArea(path))
        {
            return HandleTypedListResource(method, path, query, pageNumber, pageSize);
        }

        if (method is "POST" or "PUT" or "PATCH" or "DELETE")
        {
            return method == "POST"
                ? DemoEnvelope.SuccessData(Guid.NewGuid())
                : DemoEnvelope.Success();
        }

        return DemoEnvelope.SuccessData(new Dictionary<string, object>
        {
            ["demo"] = true,
            ["path"] = path,
            ["message"] = "Demo placeholder"
        });
    }

    private void EnsureDemoDataForPath(string path)
    {
        if (path.Equals("health", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("api/health", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("api/auth", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("api/account", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("api/resumes", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("api/ai", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("api/blog/ai", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("api/bot-commands", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("api/slack", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("api/telegram", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (path.StartsWith("api/external-data", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("chainbroker", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("api/trading", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("api/lottery", StringComparison.OrdinalIgnoreCase))
        {
            _store.EnsureMarketSeeded();
            return;
        }

        _store.EnsureCoreSeeded();
    }

    private static string GetRelativePath(Uri? uri)
    {
        if (uri is null)
        {
            return string.Empty;
        }

        string relative;
        if (uri.IsAbsoluteUri)
        {
            relative = uri.AbsolutePath;
        }
        else
        {
            relative = uri.OriginalString;
            var q = relative.IndexOf('?', StringComparison.Ordinal);
            if (q >= 0)
            {
                relative = relative[..q];
            }
        }

        return relative.Trim('/');
    }

    private static string? GetQueryValue(string query, string key)
    {
        if (string.IsNullOrEmpty(query))
        {
            return null;
        }

        var q = query.TrimStart('?');
        foreach (var part in q.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2)
            {
                continue;
            }

            if (Uri.UnescapeDataString(kv[0]).Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(kv[1]);
            }
        }

        return null;
    }

    private static string? GetSearchTerm(string query) =>
        GetQueryValue(query, "searchTerm") ?? GetQueryValue(query, "search");

    private static bool MatchesSearch(string search, params string?[] values) =>
        values.Any(value =>
            !string.IsNullOrEmpty(value) &&
            value.Contains(search, StringComparison.OrdinalIgnoreCase));

    private static string Csv(string? value)
    {
        var text = value ?? string.Empty;
        if (text.Contains('"') || text.Contains(',') || text.Contains('\n') || text.Contains('\r'))
        {
            return $"\"{text.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return text;
    }

    private static HttpResponseMessage CsvFile(string csv, string fileName)
    {
        var bytes = Encoding.UTF8.GetBytes(csv);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bytes)
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileName = fileName
        };
        return response;
    }

    private static void ParsePaging(string query, out int pageNumber, out int pageSize)
    {
        pageNumber = 1;
        pageSize = 10;
        if (string.IsNullOrEmpty(query))
        {
            return;
        }

        var q = query.TrimStart('?');
        foreach (var part in q.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2)
            {
                continue;
            }

            var key = Uri.UnescapeDataString(kv[0]);
            var value = Uri.UnescapeDataString(kv[1]);
            if (key.Equals("pageNumber", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("pageIndex", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("PageNumber", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("page", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(value, out var p) && p > 0)
                {
                    pageNumber = p;
                }
            }
            else if (key.Equals("pageSize", StringComparison.OrdinalIgnoreCase) ||
                     key.Equals("PageSize", StringComparison.OrdinalIgnoreCase) ||
                     key.Equals("perPage", StringComparison.OrdinalIgnoreCase))
            {
                // Allow -1 (return all) used by external-data clients
                if (int.TryParse(value, out var s))
                {
                    pageSize = s;
                }
            }
        }
    }

    private static async Task<T?> ReadJsonAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content is null)
        {
            return default;
        }

        try
        {
            var stream = await request.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<T>(stream, ApiJsonOptions.Default, cancellationToken);
        }
        catch
        {
            return default;
        }
    }
}
