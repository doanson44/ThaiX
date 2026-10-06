using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Contacts.Models;

namespace ThaiX.Application.Features.Contacts.Queries.ExportContacts;

/// <summary>
/// Handler for ExportContactsQuery.
/// Exports contacts to CSV using Google Contacts format with ThaiX extension columns.
/// Sensitive fields (document number, bank account number) are exported as last-4-digits only.
/// </summary>
public sealed class ExportContactsQueryHandler : IRequestHandler<ExportContactsQuery, ExportContactsResult>
{
    private readonly IApplicationDbContext _dbContext;

    public ExportContactsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExportContactsResult> Handle(ExportContactsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Contacts
            .AsNoTracking()
            .Include(c => c.Emails)
            .Include(c => c.Phones)
            .Include(c => c.Tags)
            .Include(c => c.Addresses)
            .Include(c => c.SocialLinks)
            .Include(c => c.BankAccounts)
            .Include(c => c.IdentityDocument)
            .AsQueryable();

        if (request.IsArchived.HasValue)
            query = query.Where(c => c.IsArchived == request.IsArchived.Value);

        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            var tag = request.Tag.Trim().ToLower();
            query = query.Where(c => c.Tags.Any(t => t.Name.ToLower() == tag));
        }

        var pattern = MasterDataSearchHelper.BuildContainsPattern(request.SearchTerm);
        if (pattern is not null)
        {
            var collation = MasterDataSearchHelper.Latin1GeneralCiAi;
            query = query.Where(c =>
                EF.Functions.Like(EF.Functions.Collate(c.FullName.FirstName, collation), pattern, "\\") ||
                EF.Functions.Like(EF.Functions.Collate(c.FullName.LastName, collation), pattern, "\\") ||
                (c.Company != null && EF.Functions.Like(EF.Functions.Collate(c.Company, collation), pattern, "\\")) ||
                c.Emails.Any(e => EF.Functions.Like(EF.Functions.Collate(e.Value, collation), pattern, "\\")) ||
                c.Phones.Any(p => EF.Functions.Like(EF.Functions.Collate(p.Value, collation), pattern, "\\")));
        }

        var contacts = await query
            .OrderBy(c => c.FullName.LastName)
            .ThenBy(c => c.FullName.FirstName)
            .ToListAsync(cancellationToken);

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };

        using var memoryStream = new MemoryStream();
        // Write UTF-8 BOM for Excel compatibility
        memoryStream.Write([0xEF, 0xBB, 0xBF]);

        await using var writer = new StreamWriter(memoryStream, leaveOpen: true);
        await using var csv = new CsvWriter(writer, csvConfig);

        // Write Google Contacts export headers
        foreach (var header in ContactImportCsvSpec.GoogleExportHeaders)
            csv.WriteField(header);
        await csv.NextRecordAsync();

        foreach (var contact in contacts)
        {
            var emails = contact.Emails.OrderByDescending(e => e.IsPrimary).ToList();
            var phones = contact.Phones.OrderByDescending(p => p.IsPrimary).ToList();
            var address = contact.Addresses.OrderByDescending(a => a.IsPrimary).FirstOrDefault();
            var socialLinks = contact.SocialLinks.ToList();
            var bank = contact.BankAccounts.OrderByDescending(b => b.IsPrimary).FirstOrDefault();
            var idDoc = contact.IdentityDocument;

            var groupMembership = string.Join(
                ContactImportCsvSpec.GoogleListSeparator,
                contact.Tags.Select(t => t.Name));

            // Standard Google Contacts columns
            csv.WriteField(contact.FullName.FirstName);
            csv.WriteField(contact.FullName.LastName);
            csv.WriteField(contact.Birthday?.ToString(ContactImportCsvSpec.DateFormat, CultureInfo.InvariantCulture));
            csv.WriteField(contact.Notes);
            csv.WriteField(groupMembership);

            csv.WriteField(emails.ElementAtOrDefault(0)?.Value);
            csv.WriteField(emails.ElementAtOrDefault(1)?.Value);
            csv.WriteField(emails.ElementAtOrDefault(2)?.Value);

            csv.WriteField(phones.ElementAtOrDefault(0)?.Value);
            csv.WriteField(phones.ElementAtOrDefault(1)?.Value);
            csv.WriteField(phones.ElementAtOrDefault(2)?.Value);

            csv.WriteField(contact.Company);
            csv.WriteField(contact.JobTitle);

            csv.WriteField(address?.Street);
            csv.WriteField(address?.CountryCode);
            csv.WriteField(address?.CityCode);
            csv.WriteField(null); // Address 1 - Region (not mapped; district stored in Extended Address)
            csv.WriteField(address?.PostalCode);
            csv.WriteField(address?.DistrictCode); // Address 1 - Extended Address

            csv.WriteField(socialLinks.ElementAtOrDefault(0)?.Platform);
            csv.WriteField(socialLinks.ElementAtOrDefault(0)?.Url);
            csv.WriteField(socialLinks.ElementAtOrDefault(1)?.Platform);
            csv.WriteField(socialLinks.ElementAtOrDefault(1)?.Url);

            // ThaiX extension columns (identity document — sensitive number masked to last 4)
            csv.WriteField(idDoc?.DocumentType);
            csv.WriteField(idDoc?.DocumentNumberLast4);
            csv.WriteField(idDoc?.IssuedBy);
            csv.WriteField(idDoc?.IssuedPlace);
            csv.WriteField(idDoc?.IssuedDate.ToString(ContactImportCsvSpec.DateFormat, CultureInfo.InvariantCulture));
            csv.WriteField(idDoc?.ExpiryDate?.ToString(ContactImportCsvSpec.DateFormat, CultureInfo.InvariantCulture));

            // ThaiX extension columns (bank account — sensitive number masked to last 4)
            csv.WriteField(bank?.BankCode);
            csv.WriteField(bank?.AccountNumberLast4);
            csv.WriteField(bank?.AccountName);
            csv.WriteField(bank?.CurrencyCode);

            await csv.NextRecordAsync();
        }

        await writer.FlushAsync(cancellationToken);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);

        return new ExportContactsResult
        {
            FileContent = memoryStream.ToArray(),
            FileName = $"contacts_export_{timestamp}.csv"
        };
    }
}

