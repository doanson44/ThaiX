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
    private HttpResponseMessage HandleJsonBins(string method, string path, string query, int pageNumber, int pageSize)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // api/public/json-bins/share/{token}
        if (segments.Length >= 2 && segments[1].Equals("public", StringComparison.OrdinalIgnoreCase))
        {
            var token = segments.Length >= 5 ? segments[4] : string.Empty;
            if (method == "GET" && !string.IsNullOrWhiteSpace(token))
            {
                return DemoEnvelope.SuccessData(new JsonBinSharedModel
                {
                    Code = "DEMO_SHARED",
                    Name = "Shared demo bin",
                    ContentType = "application/json",
                    ContentJson = "{\"demo\":true,\"shared\":true}",
                    ShareExpiresAtUtc = DateTime.UtcNow.AddDays(7)
                });
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        // api/json-bins/...
        Guid entityId = default;
        var hasId = segments.Length >= 3 && Guid.TryParse(segments[2], out entityId);
        var segment2 = segments.Length >= 3 ? segments[2].ToLowerInvariant() : string.Empty;
        var action = hasId && segments.Length >= 4
            ? segments[3].ToLowerInvariant()
            : segment2;

        if (method == "GET" && action is "code-exists")
        {
            var code = GetQueryValue(query, "code") ?? string.Empty;
            return DemoEnvelope.SuccessData(new ValidateJsonBinCodeModel
            {
                Code = code,
                Exists = false,
                IsAvailable = true
            });
        }

        if (method == "GET" && action is "by-code")
        {
            var byCode = segments.Length >= 4 ? segments[3] : string.Empty;
            return DemoEnvelope.SuccessData(new JsonBinDetailModel
            {
                Id = Guid.NewGuid(),
                Code = byCode,
                Name = $"Lookup: {byCode}",
                Category = JsonBinCategories.Unknown,
                ContentType = "application/json",
                SizeBytes = 512,
                IsCompressed = false,
                ContentJson = "{\"demo\":true}",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (method == "GET" && !hasId)
        {
            var items = Enumerable.Range(1, 15).Select(i => new JsonBinListItemModel
            {
                Id = new Guid($"00000000-0000-0000-0000-{i:D12}"),
                Code = $"DEMO_{i:D3}",
                Name = $"Demo JsonBin {i}",
                Category = (JsonBinCategories)(i % 5),
                ContentType = "application/json",
                SizeBytes = 1024 * i,
                IsCompressed = i % 3 == 0,
                Tags = i % 2 == 0 ? "demo,sample" : null,
                ExpiredAtUtc = i % 4 == 0 ? DateTime.UtcNow.AddDays(30) : null,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-i),
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            }).ToList();

            if (!string.IsNullOrWhiteSpace(GetSearchTerm(query)))
            {
                var search = GetSearchTerm(query)!.ToLowerInvariant();
                items = items
                    .Where(x => x.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                x.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return DemoEnvelope.Paged(items, pageNumber, pageSize);
        }

        if (method == "GET" && hasId && action is "content")
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent("{\"demo\":true}"u8.ToArray())
                {
                    Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
                }
            };
        }

        if (method == "GET" && hasId)
        {
            return DemoEnvelope.SuccessData(new JsonBinDetailModel
            {
                Id = entityId,
                Code = $"DEMO_{entityId.ToString("N")[..3].ToUpperInvariant()}",
                Name = $"Demo Detail {entityId.ToString("N")[..6]}",
                Category = JsonBinCategories.ExternalData,
                ContentType = "application/json",
                SizeBytes = 2048,
                IsCompressed = false,
                ContentJson = "{\"demo\":true,\"message\":\"This is demo content\"}",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                IsShared = false
            });
        }

        if (method == "POST" && hasId && action is "expire")
            return DemoEnvelope.Success();

        if (method == "POST" && hasId && action is "share")
        {
            var token = Convert.ToHexString(Guid.NewGuid().ToByteArray())[..16].ToLowerInvariant();
            return DemoEnvelope.SuccessData(new GenerateShareLinkResultModel
            {
                Id = entityId,
                Code = $"DEMO_{entityId.ToString("N")[..3].ToUpperInvariant()}",
                Token = token,
                RelativePath = $"/api/public/json-bins/share/{token}",
                ShareExpiresAtUtc = DateTime.UtcNow.AddDays(7)
            });
        }

        if (method == "DELETE" && hasId && action is "share")
            return DemoEnvelope.Success();

        if (method == "POST" && !hasId)
            return DemoEnvelope.SuccessData("DEMO_AUTO_GENERATED");

        if (method == "PUT" && hasId)
            return DemoEnvelope.SuccessData("DEMO_UPDATED");

        if (method == "DELETE" && hasId)
            return DemoEnvelope.Success();

        return DemoEnvelope.Success();
    }

}
