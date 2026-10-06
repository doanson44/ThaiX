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
    private async Task<HttpResponseMessage> HandlePortfoliosAsync(
        string method,
        string path,
        string query,
        int pageNumber,
        int pageSize,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (path.EndsWith("/export", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            return _store.WithLock(() =>
            {
                var search = GetSearchTerm(query);
                var rows = _store.Portfolios.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(search))
                {
                    rows = rows.Where(p =>
                        p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (p.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        p.PortfolioType.Contains(search, StringComparison.OrdinalIgnoreCase));
                }

                var sb = new StringBuilder();
                sb.AppendLine("Name,PortfolioType,Description,CreatedAt");
                foreach (var p in rows)
                {
                    sb.Append(Csv(p.Name)).Append(',')
                        .Append(Csv(p.PortfolioType)).Append(',')
                        .Append(Csv(p.Description)).Append(',')
                        .Append(p.CreatedAt.ToString("O")).AppendLine();
                }

                return CsvFile(sb.ToString(), "portfolios_export_demo.csv");
            });
        }

        if (path.EndsWith("/import/template", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            return CsvFile(
                "Name,PortfolioType,Description\nDemo Trading,Trading,Sample portfolio\n",
                "portfolios_import_template.csv");
        }

        if (path.EndsWith("/import", StringComparison.OrdinalIgnoreCase) && method == "POST")
        {
            return DemoEnvelope.SuccessData(new PortfolioImportResultDto
            {
                TotalRows = 3,
                InsertedCount = 2,
                UpdatedCount = 1,
                SkippedCount = 0,
                Errors = []
            });
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        Guid entityId = default;
        var hasId = segments.Length >= 3 && Guid.TryParse(segments[2], out entityId);

        if (method == "GET" && !hasId)
        {
            return _store.WithLock(() =>
            {
                var search = GetSearchTerm(query);
                IEnumerable<PortfolioListItemDto> rows = _store.Portfolios;
                if (!string.IsNullOrWhiteSpace(search))
                {
                    rows = rows.Where(p =>
                        p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (p.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        p.PortfolioType.Contains(search, StringComparison.OrdinalIgnoreCase));
                }

                return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
            });
        }

        if (method == "GET" && hasId)
        {
            return _store.WithLock(() =>
            {
                var item = _store.Portfolios.FirstOrDefault(x => x.Id == entityId) ?? _store.Portfolios[0];
                return DemoEnvelope.SuccessData(new PortfolioDetailDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    PortfolioType = item.PortfolioType,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt
                });
            });
        }

        if (method == "POST")
        {
            var body = await ReadJsonAsync<CreatePortfolioRequest>(request, cancellationToken)
                       ?? new CreatePortfolioRequest { Name = "Demo Portfolio", PortfolioType = "Trading" };
            return _store.WithLock(() =>
            {
                var id = Guid.NewGuid();
                _store.Portfolios.Insert(0, new PortfolioListItemDto
                {
                    Id = id,
                    Name = body.Name,
                    Description = body.Description,
                    PortfolioType = string.IsNullOrWhiteSpace(body.PortfolioType) ? "Trading" : body.PortfolioType,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                return DemoEnvelope.SuccessData(id);
            });
        }

        if (method == "PUT" && hasId)
        {
            var body = await ReadJsonAsync<UpdatePortfolioRequest>(request, cancellationToken);
            if (body is not null)
            {
                _store.WithLock(() =>
                {
                    var idx = _store.Portfolios.FindIndex(x => x.Id == entityId);
                    if (idx < 0)
                    {
                        return;
                    }

                    var existing = _store.Portfolios[idx];
                    _store.Portfolios[idx] = new PortfolioListItemDto
                    {
                        Id = existing.Id,
                        Name = body.Name,
                        Description = body.Description,
                        PortfolioType = string.IsNullOrWhiteSpace(body.PortfolioType) ? existing.PortfolioType : body.PortfolioType,
                        CreatedAt = existing.CreatedAt,
                        UpdatedAt = DateTime.UtcNow
                    };
                });
            }

            return DemoEnvelope.Success();
        }

        if (method == "DELETE" && hasId)
        {
            _store.WithLock(() => _store.Portfolios.RemoveAll(x => x.Id == entityId));
            return DemoEnvelope.Success();
        }

        return DemoEnvelope.Success();
    }

    private HttpResponseMessage HandleFiles(string method, string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        Guid fileId = default;
        var hasId = segments.Length >= 3 && Guid.TryParse(segments[2], out fileId);

        if (method == "POST" && path.EndsWith("/upload", StringComparison.OrdinalIgnoreCase))
        {
            // BlogService / FileService upload expects a raw UploadedFileDto (not ApiResponse envelope).
            return _store.WithLock(() =>
            {
                var dto = new UploadedFileDto
                {
                    Id = Guid.NewGuid(),
                    FileName = "demo.png",
                    ContentType = "image/svg+xml",
                    Size = 1024,
                    Url = DemoImageUrls.Featured($"upload-{Guid.NewGuid():N}")
                };
                _store.Files[dto.Id] = dto;
                return DemoEnvelope.Json(dto);
            });
        }

        if (hasId && method == "GET")
        {
            var id = fileId;
            return _store.WithLock(() =>
            {
                if (!_store.Files.TryGetValue(id, out var dto))
                {
                    dto = new UploadedFileDto
                    {
                        Id = id,
                        FileName = "demo-file.png",
                        ContentType = "image/svg+xml",
                        Size = 2048,
                        Url = DemoImageUrls.Featured($"file-{id:N}")
                    };
                    _store.Files[id] = dto;
                }

                return DemoEnvelope.SuccessData(dto);
            });
        }

        if (hasId && method == "DELETE")
        {
            var id = fileId;
            return _store.WithLock(() =>
            {
                _store.Files.Remove(id);
                return DemoEnvelope.Success();
            });
        }

        return DemoEnvelope.Success();
    }

}
