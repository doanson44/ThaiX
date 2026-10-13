using ThaiX.Client.Models.Notes;


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
