using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Runtime.CompilerServices;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Contacts.Models;

namespace ThaiX.Infrastructure.Import.Csv;

/// <summary>
/// Parses contact CSV streams in either Google Contacts format or the legacy ThaiX format.
/// Google Contacts format is detected automatically from the header row.
/// </summary>
public sealed class ContactCsvParser : IContactCsvParser
{
    private const int PhoneValueMaxLength = 20;
    private static readonly string[] BirthdayFormats = { "yyyy-MM-dd", "M/d/yyyy", "MM/dd/yyyy", "d/M/yyyy", "--MM-dd" };

    public async IAsyncEnumerable<ParsedImportRow> ParseAsync(
        Stream csvStream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, config);
        await csv.ReadAsync();
        csv.ReadHeader();

        var isGoogle = IsGoogleContactsFormat(csv.HeaderRecord);
        if (isGoogle)
            csv.Context.RegisterClassMap<GoogleCsvRowMap>();
        else
            csv.Context.RegisterClassMap<StandardCsvRowMap>();

        ValidateRequiredHeaders(csv.HeaderRecord, isGoogle);

        var rowNumber = 1;
        while (await csv.ReadAsync())
        {
            rowNumber++;
            cancellationToken.ThrowIfCancellationRequested();
            var raw = csv.GetRecord<StandardCsvRow>();
            if (raw is not { } r)
                continue;

            var emails = CollectEmails(r.Email1, r.Email2, r.Email3);
            var phones = CollectPhones(r.Phone1, r.Phone2, r.Phone3);
            var tags = ParseTags(r.Tags);
            var (firstName, lastName) = NormalizeName(r.FirstName, r.LastName);

            var addresses = BuildAddresses(r);
            var identityDoc = BuildIdentityDocument(r);
            var socialLinks = BuildSocialLinks(r);
            var bankAccounts = BuildBankAccounts(r);

            yield return new ParsedImportRow
            {
                RowNumber = rowNumber,
                Data = new ContactImportRow
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Emails = emails,
                    Phones = phones,
                    Company = r.Company?.Trim(),
                    JobTitle = r.JobTitle?.Trim(),
                    Birthday = TryParseBirthday(r.Birthday),
                    Notes = r.Notes?.Trim(),
                    Tags = tags,
                    Addresses = addresses,
                    IdentityDocument = identityDoc,
                    SocialLinks = socialLinks,
                    BankAccounts = bankAccounts
                }
            };
        }
    }

    private static bool IsGoogleContactsFormat(string[]? headers)
    {
        if (headers == null) return false;
        var set = new HashSet<string>(headers.Select(h => h?.Trim() ?? string.Empty), StringComparer.OrdinalIgnoreCase);
        return set.Contains(ContactImportCsvSpec.G_GivenName)
            || set.Contains(ContactImportCsvSpec.G_FamilyName)
            || set.Contains(ContactImportCsvSpec.G_Email1Value);
    }

    private static void ValidateRequiredHeaders(string[]? headerRecord, bool isGoogle)
    {
        if (headerRecord == null || headerRecord.Length == 0)
            throw new OperationFailedException(ErrorCodes.CONTACT_IMPORT_INVALID_CSV_TEMPLATE,
                "CSV must have a header row. Download the import template to get the correct format.");

        var set = new HashSet<string>(headerRecord.Select(h => h?.Trim() ?? string.Empty), StringComparer.OrdinalIgnoreCase);
        var required = isGoogle ? ContactImportCsvSpec.GoogleRequiredHeaders : ContactImportCsvSpec.RequiredHeaders;
        foreach (var col in required)
        {
            if (!set.Contains(col))
                throw new OperationFailedException(ErrorCodes.CONTACT_IMPORT_INVALID_CSV_TEMPLATE,
                    $"CSV is missing required column: '{col}'. Download the import template to get the correct format.");
        }
    }

    private static (string FirstName, string LastName) NormalizeName(string? first, string? last)
    {
        var f = first?.Trim() ?? string.Empty;
        var l = last?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(f) && string.IsNullOrWhiteSpace(l))
            return (string.Empty, string.Empty);
        if (string.IsNullOrWhiteSpace(l))
            return (f, f);
        if (string.IsNullOrWhiteSpace(f))
            return (l, l);
        return (f, l);
    }

    private static IReadOnlyList<string> CollectEmails(string? e1, string? e2, string? e3)
    {
        var list = new List<string>();
        foreach (var e in new[] { e1, e2, e3 })
        {
            var v = e?.Trim();
            if (string.IsNullOrWhiteSpace(v)) continue;
            var lower = v.ToLowerInvariant();
            if (!list.Contains(lower))
                list.Add(lower);
        }
        return list;
    }

    private static IReadOnlyList<string> CollectPhones(string? p1, string? p2, string? p3)
    {
        var list = new List<string>();
        foreach (var p in new[] { p1, p2, p3 })
        {
            var v = SanitizePhone(p);
            if (string.IsNullOrWhiteSpace(v)) continue;
            if (!list.Contains(v))
                list.Add(v);
        }
        return list;
    }

    private static string? SanitizePhone(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        var s = raw.Trim();
        var idx = s.IndexOf(" ::: ", StringComparison.Ordinal);
        if (idx >= 0)
            s = s[..idx].Trim();
        if (string.IsNullOrWhiteSpace(s))
            return null;
        return s.Length > PhoneValueMaxLength ? s[..PhoneValueMaxLength] : s;
    }

    private static IReadOnlyList<string> ParseTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
            return [];
        // Support both ThaiX format (";") and Google Contacts format (" ::: ")
        var separators = new[] { ContactImportCsvSpec.GoogleListSeparator, ContactImportCsvSpec.ListSeparator };
        return tags.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.TrimStart('*').Trim()) // strip Google's leading "* " prefix from group names
            .Where(t => t.Length > 0)
            .Distinct()
            .ToList();
    }

    private static DateOnly? TryParseBirthday(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var v = value.Trim();
        if (DateOnly.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return d;
        if (DateTime.TryParseExact(v, BirthdayFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return DateOnly.FromDateTime(dt);
        if (DateTime.TryParse(v, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out dt))
            return DateOnly.FromDateTime(dt);
        return null;
    }

    private static IReadOnlyList<ContactImportAddress> BuildAddresses(StandardCsvRow r)
    {
        var street = r.Street?.Trim() ?? string.Empty;
        var country = r.CountryCode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(street) && string.IsNullOrWhiteSpace(country))
            return [];
        return new List<ContactImportAddress>
        {
            new ContactImportAddress
            {
                Street = street,
                CountryCode = country,
                CityCode = r.CityCode?.Trim() ?? string.Empty,
                DistrictCode = r.DistrictCode?.Trim() ?? string.Empty,
                PostalCode = r.PostalCode?.Trim() ?? string.Empty,
                IsPrimary = true
            }
        };
    }

    private static ContactImportIdentityDocument? BuildIdentityDocument(StandardCsvRow r)
    {
        if (string.IsNullOrWhiteSpace(r.DocumentType) || string.IsNullOrWhiteSpace(r.DocumentNumber) ||
            string.IsNullOrWhiteSpace(r.IssuedBy) || string.IsNullOrWhiteSpace(r.IssuedPlace) ||
            string.IsNullOrWhiteSpace(r.IssuedDate))
            return null;
        if (!DateOnly.TryParse(r.IssuedDate.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var issued))
            return null;
        DateOnly? expiry = null;
        if (!string.IsNullOrWhiteSpace(r.ExpiryDate) && DateOnly.TryParse(r.ExpiryDate.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var ex))
            expiry = ex;
        return new ContactImportIdentityDocument
        {
            DocumentType = r.DocumentType.Trim(),
            DocumentNumber = r.DocumentNumber.Trim(),
            IssuedBy = r.IssuedBy.Trim(),
            IssuedPlace = r.IssuedPlace.Trim(),
            IssuedDate = issued,
            ExpiryDate = expiry
        };
    }

    private static IReadOnlyList<ContactImportSocialLink> BuildSocialLinks(StandardCsvRow r)
    {
        var list = new List<ContactImportSocialLink>();
        if (!string.IsNullOrWhiteSpace(r.SocialPlatform1) && !string.IsNullOrWhiteSpace(r.SocialUrl1))
            list.Add(new ContactImportSocialLink { Platform = r.SocialPlatform1.Trim(), Url = r.SocialUrl1.Trim() });
        if (!string.IsNullOrWhiteSpace(r.SocialPlatform2) && !string.IsNullOrWhiteSpace(r.SocialUrl2))
            list.Add(new ContactImportSocialLink { Platform = r.SocialPlatform2.Trim(), Url = r.SocialUrl2.Trim() });
        return list;
    }

    private static IReadOnlyList<ContactImportBankAccount> BuildBankAccounts(StandardCsvRow r)
    {
        if (string.IsNullOrWhiteSpace(r.BankCode) || string.IsNullOrWhiteSpace(r.BankAccountNumber) ||
            string.IsNullOrWhiteSpace(r.BankAccountName) || string.IsNullOrWhiteSpace(r.BankCurrencyCode))
            return [];
        return new List<ContactImportBankAccount>
        {
            new ContactImportBankAccount
            {
                BankCode = r.BankCode.Trim(),
                BranchName = null,
                AccountNumber = r.BankAccountNumber.Trim(),
                AccountName = r.BankAccountName.Trim(),
                CurrencyCode = r.BankCurrencyCode.Trim(),
                IsPrimary = true
            }
        };
    }

    /// <summary>Row DTO with exact standard template column names (no aliases).</summary>
    private sealed class StandardCsvRow
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email1 { get; set; }
        public string? Email2 { get; set; }
        public string? Email3 { get; set; }
        public string? Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? Phone3 { get; set; }
        public string? Company { get; set; }
        public string? JobTitle { get; set; }
        public string? Birthday { get; set; }
        public string? Notes { get; set; }
        public string? Tags { get; set; }
        public string? Street { get; set; }
        public string? CountryCode { get; set; }
        public string? CityCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? PostalCode { get; set; }
        public string? DocumentType { get; set; }
        public string? DocumentNumber { get; set; }
        public string? IssuedBy { get; set; }
        public string? IssuedPlace { get; set; }
        public string? IssuedDate { get; set; }
        public string? ExpiryDate { get; set; }
        public string? SocialPlatform1 { get; set; }
        public string? SocialUrl1 { get; set; }
        public string? SocialPlatform2 { get; set; }
        public string? SocialUrl2 { get; set; }
        public string? BankCode { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankCurrencyCode { get; set; }
    }

    /// <summary>Map standard column names only (no aliases).</summary>
    private sealed class StandardCsvRowMap : ClassMap<StandardCsvRow>
    {
        public StandardCsvRowMap()
        {
            Map(m => m.FirstName).Name(ContactImportCsvSpec.FirstName);
            Map(m => m.LastName).Name(ContactImportCsvSpec.LastName);
            Map(m => m.Email1).Name(ContactImportCsvSpec.Email1).Optional();
            Map(m => m.Email2).Name(ContactImportCsvSpec.Email2).Optional();
            Map(m => m.Email3).Name(ContactImportCsvSpec.Email3).Optional();
            Map(m => m.Phone1).Name(ContactImportCsvSpec.Phone1).Optional();
            Map(m => m.Phone2).Name(ContactImportCsvSpec.Phone2).Optional();
            Map(m => m.Phone3).Name(ContactImportCsvSpec.Phone3).Optional();
            Map(m => m.Company).Name(ContactImportCsvSpec.Company).Optional();
            Map(m => m.JobTitle).Name(ContactImportCsvSpec.JobTitle).Optional();
            Map(m => m.Birthday).Name(ContactImportCsvSpec.Birthday).Optional();
            Map(m => m.Notes).Name(ContactImportCsvSpec.Notes).Optional();
            Map(m => m.Tags).Name(ContactImportCsvSpec.Tags).Optional();
            Map(m => m.Street).Name(ContactImportCsvSpec.Street).Optional();
            Map(m => m.CountryCode).Name(ContactImportCsvSpec.CountryCode).Optional();
            Map(m => m.CityCode).Name(ContactImportCsvSpec.CityCode).Optional();
            Map(m => m.DistrictCode).Name(ContactImportCsvSpec.DistrictCode).Optional();
            Map(m => m.PostalCode).Name(ContactImportCsvSpec.PostalCode).Optional();
            Map(m => m.DocumentType).Name(ContactImportCsvSpec.DocumentType).Optional();
            Map(m => m.DocumentNumber).Name(ContactImportCsvSpec.DocumentNumber).Optional();
            Map(m => m.IssuedBy).Name(ContactImportCsvSpec.IssuedBy).Optional();
            Map(m => m.IssuedPlace).Name(ContactImportCsvSpec.IssuedPlace).Optional();
            Map(m => m.IssuedDate).Name(ContactImportCsvSpec.IssuedDate).Optional();
            Map(m => m.ExpiryDate).Name(ContactImportCsvSpec.ExpiryDate).Optional();
            Map(m => m.SocialPlatform1).Name(ContactImportCsvSpec.SocialPlatform1).Optional();
            Map(m => m.SocialUrl1).Name(ContactImportCsvSpec.SocialUrl1).Optional();
            Map(m => m.SocialPlatform2).Name(ContactImportCsvSpec.SocialPlatform2).Optional();
            Map(m => m.SocialUrl2).Name(ContactImportCsvSpec.SocialUrl2).Optional();
            Map(m => m.BankCode).Name(ContactImportCsvSpec.BankCode).Optional();
            Map(m => m.BankAccountNumber).Name(ContactImportCsvSpec.BankAccountNumber).Optional();
            Map(m => m.BankAccountName).Name(ContactImportCsvSpec.BankAccountName).Optional();
            Map(m => m.BankCurrencyCode).Name(ContactImportCsvSpec.BankCurrencyCode).Optional();
        }
    }

    /// <summary>
    /// Map Google Contacts CSV column names to StandardCsvRow.
    /// ThaiX extension columns (DocumentType, BankCode, etc.) keep their own names and are optional.
    /// The DocumentNumberLast4 / BankAccountLast4 columns (from ThaiX export) are intentionally
    /// NOT mapped to DocumentNumber / BankAccountNumber — masked values cannot be re-imported.
    /// </summary>
    private sealed class GoogleCsvRowMap : ClassMap<StandardCsvRow>
    {
        public GoogleCsvRowMap()
        {
            Map(m => m.FirstName).Name(ContactImportCsvSpec.G_GivenName);
            Map(m => m.LastName).Name(ContactImportCsvSpec.G_FamilyName);
            Map(m => m.Email1).Name(ContactImportCsvSpec.G_Email1Value).Optional();
            Map(m => m.Email2).Name(ContactImportCsvSpec.G_Email2Value).Optional();
            Map(m => m.Email3).Name(ContactImportCsvSpec.G_Email3Value).Optional();
            Map(m => m.Phone1).Name(ContactImportCsvSpec.G_Phone1Value).Optional();
            Map(m => m.Phone2).Name(ContactImportCsvSpec.G_Phone2Value).Optional();
            Map(m => m.Phone3).Name(ContactImportCsvSpec.G_Phone3Value).Optional();
            Map(m => m.Company).Name(ContactImportCsvSpec.G_Org1Name).Optional();
            Map(m => m.JobTitle).Name(ContactImportCsvSpec.G_Org1Title).Optional();
            Map(m => m.Birthday).Name(ContactImportCsvSpec.G_Birthday).Optional();
            Map(m => m.Notes).Name(ContactImportCsvSpec.G_Notes).Optional();
            Map(m => m.Tags).Name(ContactImportCsvSpec.G_GroupMembership).Optional();
            Map(m => m.Street).Name(ContactImportCsvSpec.G_Addr1Street).Optional();
            Map(m => m.CountryCode).Name(ContactImportCsvSpec.G_Addr1Country).Optional();
            Map(m => m.CityCode).Name(ContactImportCsvSpec.G_Addr1City).Optional();
            Map(m => m.DistrictCode).Name(ContactImportCsvSpec.G_Addr1ExtendedAddress).Optional();
            Map(m => m.PostalCode).Name(ContactImportCsvSpec.G_Addr1PostalCode).Optional();
            Map(m => m.SocialPlatform1).Name(ContactImportCsvSpec.G_Web1Type).Optional();
            Map(m => m.SocialUrl1).Name(ContactImportCsvSpec.G_Web1Value).Optional();
            Map(m => m.SocialPlatform2).Name(ContactImportCsvSpec.G_Web2Type).Optional();
            Map(m => m.SocialUrl2).Name(ContactImportCsvSpec.G_Web2Value).Optional();
            // ThaiX extension columns (same name in both template and Google format)
            Map(m => m.DocumentType).Name(ContactImportCsvSpec.DocumentType).Optional();
            Map(m => m.DocumentNumber).Name(ContactImportCsvSpec.DocumentNumber).Optional();
            Map(m => m.IssuedBy).Name(ContactImportCsvSpec.IssuedBy).Optional();
            Map(m => m.IssuedPlace).Name(ContactImportCsvSpec.IssuedPlace).Optional();
            Map(m => m.IssuedDate).Name(ContactImportCsvSpec.IssuedDate).Optional();
            Map(m => m.ExpiryDate).Name(ContactImportCsvSpec.ExpiryDate).Optional();
            Map(m => m.BankCode).Name(ContactImportCsvSpec.BankCode).Optional();
            Map(m => m.BankAccountNumber).Name(ContactImportCsvSpec.BankAccountNumber).Optional();
            Map(m => m.BankAccountName).Name(ContactImportCsvSpec.BankAccountName).Optional();
            Map(m => m.BankCurrencyCode).Name(ContactImportCsvSpec.BankCurrencyCode).Optional();
        }
    }
}
