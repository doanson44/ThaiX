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
    private async Task<HttpResponseMessage> HandleUsersAsync(
        string method, string path, int pageNumber, int pageSize,
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var query = request.RequestUri?.Query ?? string.Empty;

        if (path.Equals("api/users/me/profile", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(new UserProfileDto
            {
                FullName = "Demo Admin",
                AvatarUrl = DemoImageUrls.Avatar("Demo Admin"),
                ResumeSlug = "demo-admin"
            });
        }

        if (path.Equals("api/users/permissions", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(DemoSessionStore.AllPermissions);
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        Guid userId = default;
        var hasUserId = segments.Length >= 3 && Guid.TryParse(segments[2], out userId);
        var sub = segments.Length >= 4 ? segments[3].ToLowerInvariant() : string.Empty;

        if (method == "GET" && !hasUserId)
        {
            return _store.WithLock(() =>
            {
                var search = GetSearchTerm(query);
                IEnumerable<UserListItemDto> rows = _store.Users;
                if (!string.IsNullOrWhiteSpace(search))
                    rows = rows.Where(u => u.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
                return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
            });
        }

        if (method == "GET" && hasUserId && string.IsNullOrEmpty(sub))
        {
            return _store.WithLock(() =>
            {
                var user = _store.Users.FirstOrDefault(u => u.Id == userId)
                           ?? _store.Users[0];
                return DemoEnvelope.SuccessData(user);
            });
        }

        if (sub == "permissions")
        {
            if (method == "GET")
            {
                return DemoEnvelope.SuccessData(DemoSessionStore.AllPermissions);
            }

            return DemoEnvelope.Success();
        }

        if (sub is "linked-contact" or "link-contact")
        {
            if (method == "GET")
            {
                return DemoEnvelope.SuccessData(new LinkedContactDto
                {
                    ContactId = _store.Contacts[0].Id,
                    DisplayName = _store.Contacts[0].DisplayName
                });
            }

            return DemoEnvelope.Success();
        }

        if (sub == "reset-password")
        {
            return DemoEnvelope.SuccessData(new ResetPasswordResult
            {
                TemporaryPassword = "Demo@12345",
                RequirePasswordChange = true
            });
        }

        if (sub == "lockout")
        {
            return DemoEnvelope.Success();
        }

        if (method == "POST" && !hasUserId)
        {
            var body = await ReadJsonAsync<CreateUserRequest>(request, cancellationToken);
            return _store.WithLock(() =>
            {
                var id = Guid.NewGuid();
                _store.Users.Add(new UserListItemDto
                {
                    Id = id,
                    Email = body?.Email ?? $"user{id:N}@local",
                    EmailConfirmed = true,
                    LockoutEnabled = false
                });
                return DemoEnvelope.SuccessData(id);
            });
        }

        if (method is "PUT" or "DELETE")
        {
            if (method == "DELETE" && hasUserId)
            {
                _store.WithLock(() => _store.Users.RemoveAll(u => u.Id == userId));
            }

            return DemoEnvelope.Success();
        }

        return DemoEnvelope.SuccessData(new { demo = true });
    }

}
