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
    private HttpResponseMessage HandleContacts(string method, string path, string query, int pageNumber, int pageSize)
    {
        if (path.EndsWith("/export", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            return _store.WithLock(() =>
            {
                var search = GetSearchTerm(query);
                var rows = _store.Contacts.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(search))
                {
                    rows = rows.Where(c =>
                        c.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (c.PrimaryEmail?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                var sb = new StringBuilder();
                sb.AppendLine("FirstName,LastName,Email,Phone,Company,JobTitle");
                foreach (var c in rows)
                {
                    sb.Append(Csv(c.FirstName)).Append(',')
                        .Append(Csv(c.LastName)).Append(',')
                        .Append(Csv(c.PrimaryEmail)).Append(',')
                        .Append(Csv(c.PrimaryPhone)).Append(',')
                        .Append(Csv(c.Company)).Append(',')
                        .Append(Csv(c.JobTitle)).AppendLine();
                }

                return CsvFile(sb.ToString(), "contacts_export_demo.csv");
            });
        }

        if (path.EndsWith("/import/template", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            return CsvFile(
                "FirstName,LastName,Email,Phone,Company,JobTitle\nDemo,User,demo@local,+84000000000,Demo Co,Engineer\n",
                "contacts_import_template.csv");
        }

        if (path.EndsWith("/import-async", StringComparison.OrdinalIgnoreCase) && method == "POST")
        {
            return _store.WithLock(() =>
            {
                var jobId = Guid.NewGuid();
                _store.ContactImportJobs[jobId] = new ContactImportJobDto
                {
                    Id = jobId,
                    Status = "Completed",
                    ProgressPercent = 100,
                    CreatedAtUtc = DateTime.UtcNow.AddMinutes(-1),
                    CompletedAtUtc = DateTime.UtcNow,
                    Result = new ContactImportResultDto
                    {
                        TotalRows = 3,
                        InsertedCount = 2,
                        UpdatedCount = 1,
                        SkippedCount = 0,
                        Errors = []
                    }
                };
                return DemoEnvelope.SuccessData(jobId);
            });
        }

        if (path.Contains("/import/jobs/", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            var jobSegments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var jobIdText = jobSegments.LastOrDefault();
            if (!Guid.TryParse(jobIdText, out var jobId))
                jobId = Guid.NewGuid();

            return _store.WithLock(() =>
            {
                if (!_store.ContactImportJobs.TryGetValue(jobId, out var job))
                {
                    job = new ContactImportJobDto
                    {
                        Id = jobId,
                        Status = "Completed",
                        ProgressPercent = 100,
                        CreatedAtUtc = DateTime.UtcNow.AddMinutes(-2),
                        CompletedAtUtc = DateTime.UtcNow,
                        Result = new ContactImportResultDto
                        {
                            TotalRows = 1,
                            InsertedCount = 1,
                            UpdatedCount = 0,
                            SkippedCount = 0,
                            Errors = []
                        }
                    };
                }

                return DemoEnvelope.SuccessData(job);
            });
        }

        if (path.EndsWith("/import", StringComparison.OrdinalIgnoreCase) && method == "POST")
        {
            return DemoEnvelope.SuccessData(new ContactImportResultDto
            {
                TotalRows = 3,
                InsertedCount = 2,
                UpdatedCount = 1,
                SkippedCount = 0,
                Errors = []
            });
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        Guid contactId = default;
        var hasId = segments.Length >= 3 && Guid.TryParse(segments[2], out contactId);

        if (hasId && segments.Length >= 4 &&
            segments[3].Equals("suggest-users", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() => DemoEnvelope.SuccessData(
                _store.Users.Take(5).Select(u => new SuggestedUserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    PhoneNumber = null
                }).ToList()));
        }

        if (method == "GET" && !hasId)
        {
            return _store.WithLock(() =>
            {
                var search = GetSearchTerm(query);
                IEnumerable<ContactListItemDto> rows = _store.Contacts;
                if (!string.IsNullOrWhiteSpace(search))
                {
                    rows = rows.Where(c =>
                        c.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (c.PrimaryEmail?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (c.Company?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
            });
        }

        if (method == "GET" && hasId)
        {
            return _store.WithLock(() =>
            {
                var contact = _store.Contacts.FirstOrDefault(c => c.Id == contactId) ?? _store.Contacts[0];
                return DemoEnvelope.SuccessData(ToContactDetail(contact));
            });
        }

        if (method == "POST" && !hasId)
        {
            return _store.WithLock(() =>
            {
                var id = Guid.NewGuid();
                _store.Contacts.Insert(0, new ContactListItemDto
                {
                    Id = id,
                    DisplayName = "New Demo Contact",
                    FirstName = "New",
                    LastName = "Contact",
                    PrimaryEmail = $"contact-{id:N}@local",
                    AvatarUrl = DemoImageUrls.Avatar("New Contact"),
                    CreatedAt = DateTime.UtcNow
                });
                return DemoEnvelope.SuccessData(id);
            });
        }

        if (hasId && segments.Length >= 4 &&
            segments[3].Equals("avatar", StringComparison.OrdinalIgnoreCase) &&
            method == "POST")
        {
            var url = DemoImageUrls.Avatar($"contact-{contactId:N}");
            return DemoEnvelope.SuccessData(url);
        }

        if (method is "PUT" or "DELETE" or "PATCH" or "POST")
        {
            if (method == "DELETE" && hasId && segments.Length == 3)
            {
                _store.WithLock(() => _store.Contacts.RemoveAll(c => c.Id == contactId));
            }

            return method == "POST"
                ? DemoEnvelope.SuccessData(Guid.NewGuid())
                : DemoEnvelope.Success();
        }

        return DemoEnvelope.Success();
    }

    private static ContactDetailDto ToContactDetail(ContactListItemDto contact) =>
        new()
        {
            Id = contact.Id,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Company = contact.Company,
            JobTitle = contact.JobTitle,
            AvatarUrl = contact.AvatarUrl,
            Notes = "Demo contact notes",
            IsArchived = contact.IsArchived,
            CreatedAt = contact.CreatedAt,
            UpdatedAt = contact.LastUpdated,
            Emails =
            [
                new ContactEmailDto
                {
                    Id = Guid.NewGuid(),
                    Value = contact.PrimaryEmail ?? "demo@local",
                    IsPrimary = true
                }
            ],
            Phones = string.IsNullOrWhiteSpace(contact.PrimaryPhone)
                ? []
                :
                [
                    new ContactPhoneDto
                    {
                        Id = Guid.NewGuid(),
                        Value = contact.PrimaryPhone,
                        IsPrimary = true
                    }
                ],
            Addresses =
            [
                new ContactAddressDto
                {
                    Id = Guid.NewGuid(),
                    Street = "1 Demo Street",
                    CountryCode = "VN",
                    CityCode = "HCM",
                    DistrictCode = "Q1",
                    PostalCode = "700000",
                    IsPrimary = true
                }
            ],
            SocialLinks = [],
            Tags =
            [
                new ContactTagDto { Id = Guid.NewGuid(), Name = "Demo" }
            ],
            BankAccounts = [],
            IdentityDocuments = []
        };

}
