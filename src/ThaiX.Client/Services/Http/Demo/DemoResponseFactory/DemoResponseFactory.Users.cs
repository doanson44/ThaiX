using ThaiX.Client.Models.Users;


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
