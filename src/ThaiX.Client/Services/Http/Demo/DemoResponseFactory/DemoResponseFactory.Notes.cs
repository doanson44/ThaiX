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
    private HttpResponseMessage HandleNotes(string method, string path, string query, int pageNumber, int pageSize)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        Guid entityId = default;
        var hasId = segments.Length >= 3 && Guid.TryParse(segments[2], out entityId);
        var action = segments.Length >= 4 ? segments[3].ToLowerInvariant() : string.Empty;

        if (hasId && method == "POST" && action is "pin" or "archive")
        {
            _store.WithLock(() =>
            {
                var idx = _store.Notes.FindIndex(n => n.Id == entityId);
                if (idx < 0) return;
                var n = _store.Notes[idx];
                _store.Notes[idx] = new NoteDto
                {
                    Id = n.Id,
                    OwnerId = n.OwnerId,
                    Title = n.Title,
                    Content = n.Content,
                    Color = n.Color,
                    IsPinned = action == "pin" ? !n.IsPinned : n.IsPinned,
                    IsArchived = action == "archive" ? !n.IsArchived : n.IsArchived,
                    CreatedAt = n.CreatedAt,
                    UpdatedAt = DateTime.UtcNow
                };
            });
            return DemoEnvelope.Success();
        }

        if (method == "GET" && !hasId)
        {
            return _store.WithLock(() =>
            {
                var search = GetSearchTerm(query);
                var pinned = GetQueryValue(query, "isPinned");
                var archived = GetQueryValue(query, "isArchived");
                IEnumerable<NoteDto> rows = _store.Notes;
                if (!string.IsNullOrWhiteSpace(search))
                    rows = rows.Where(n =>
                        n.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        n.Content.Contains(search, StringComparison.OrdinalIgnoreCase));
                if (bool.TryParse(pinned, out var isPinned))
                    rows = rows.Where(n => n.IsPinned == isPinned);
                if (bool.TryParse(archived, out var isArchived))
                    rows = rows.Where(n => n.IsArchived == isArchived);
                return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
            });
        }

        if (method == "GET" && hasId)
        {
            return _store.WithLock(() =>
            {
                var item = _store.Notes.FirstOrDefault(x => x.Id == entityId) ?? _store.Notes[0];
                return DemoEnvelope.SuccessData(item);
            });
        }

        if (method == "POST" && !hasId)
            return DemoEnvelope.SuccessData(Guid.NewGuid());

        if (method == "DELETE" && hasId)
        {
            _store.WithLock(() => _store.Notes.RemoveAll(n => n.Id == entityId));
            return DemoEnvelope.Success();
        }

        return DemoEnvelope.Success();
    }

}
