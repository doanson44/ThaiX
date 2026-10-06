using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.Contacts.Models;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Application.Features.Contacts.Commands.UpsertContacts;

/// <summary>
/// Upsert by email first, then phone. Soft-deleted match → Restore + apply import on that same Contact.
/// Phone match is refused when the row's emails conflict with the phone owner's emails (shared-number case).
/// Never strip/move another contact's emails/phones. Mutations only via Contact aggregate methods.
/// </summary>
public sealed class UpsertContactsCommandHandler : IRequestHandler<UpsertContactsCommand, UpsertContactsResult>
{
    private const int PhoneValueMaxLength = 20;
    private const int StreetMaxLength = 500;
    private const int BranchNameMaxLength = 20;
    private const int BankCodeMaxLength = 20;

    private readonly IApplicationDbContext _dbContext;
    private readonly IEncryptionService _encryptionService;

    public UpsertContactsCommandHandler(IApplicationDbContext dbContext, IEncryptionService encryptionService)
    {
        _dbContext = dbContext;
        _encryptionService = encryptionService;
    }

    public async Task<UpsertContactsResult> Handle(UpsertContactsCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<ImportRowError>();
        var inserted = 0;
        var updated = 0;
        var skipped = 0;

        var rowsToProcess = new List<(ParsedImportRow Parsed, string? PrimaryNorm, string? PrimaryEmail, ContactImportRow Row)>();

        foreach (var parsed in request.Rows)
        {
            var row = parsed.Data;
            if (string.IsNullOrWhiteSpace(row.FirstName) || string.IsNullOrWhiteSpace(row.LastName))
            {
                errors.Add(new ImportRowError { RowNumber = parsed.RowNumber, RawData = FormatRow(row), Error = "FirstName and LastName are required." });
                skipped++;
                continue;
            }

            var norms = row.Phones.Select(SanitizePhone).Select(PhoneNormalizer.Normalize).Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();
            var emails = row.Emails.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim().ToLowerInvariant()).Distinct().ToList();

            if (norms.Count == 0 && emails.Count == 0)
            {
                errors.Add(new ImportRowError { RowNumber = parsed.RowNumber, RawData = FormatRow(row), Error = "At least one identity signal (phone or email) is required." });
                skipped++;
                continue;
            }

            var primaryNorm = norms.FirstOrDefault();
            var primaryEmail = emails.FirstOrDefault();
            rowsToProcess.Add((parsed, primaryNorm, primaryEmail, row));
        }

        if (rowsToProcess.Count == 0)
            return new UpsertContactsResult { InsertedCount = 0, UpdatedCount = 0, SkippedCount = skipped, Errors = errors };

        var allNorms = request.Rows
            .SelectMany(parsed => parsed.Data.Phones.Select(SanitizePhone).Select(PhoneNormalizer.Normalize).Where(n => !string.IsNullOrEmpty(n)))
            .Distinct()
            .ToList();
        var allEmails = request.Rows
            .SelectMany(parsed => parsed.Data.Emails.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim().ToLowerInvariant()))
            .Distinct()
            .ToList();

        var phoneToContactId = await _dbContext.ContactPhones
            .AsNoTracking()
            .Where(p => allNorms.Contains(p.NormalizedValue))
            .Select(p => new { p.NormalizedValue, p.ContactId })
            .ToListAsync(cancellationToken);
        var phoneDict = phoneToContactId.GroupBy(x => x.NormalizedValue).ToDictionary(g => g.Key, g => g.First().ContactId);

        var emailToContactId = await _dbContext.ContactEmails
            .AsNoTracking()
            .Where(e => allEmails.Contains(e.Value))
            .Select(e => new { e.Value, e.ContactId })
            .ToListAsync(cancellationToken);
        var emailDict = emailToContactId.GroupBy(x => x.Value).ToDictionary(g => g.Key, g => g.First().ContactId);

        // Load email hits and phone hits; resolve order + phone/email conflict at match time.
        var contactIdsToLoad = rowsToProcess
            .SelectMany(r =>
            {
                var ids = new List<Guid>();
                if (!string.IsNullOrEmpty(r.PrimaryEmail) && emailDict.TryGetValue(r.PrimaryEmail, out var emailId))
                    ids.Add(emailId);
                if (!string.IsNullOrEmpty(r.PrimaryNorm) && phoneDict.TryGetValue(r.PrimaryNorm, out var phoneId))
                    ids.Add(phoneId);
                return ids;
            })
            .Distinct()
            .ToList();

        var contactsForUpdate = contactIdsToLoad.Count > 0
            ? await _dbContext.Contacts
                .IgnoreQueryFilters()
                .Include(c => c.Phones)
                .Include(c => c.Emails)
                .Include(c => c.Tags)
                .Include(c => c.Addresses)
                .Include(c => c.SocialLinks)
                .Include(c => c.BankAccounts)
                .Include(c => c.IdentityDocument)
                .Where(c => contactIdsToLoad.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, cancellationToken)
            : new Dictionary<Guid, Contact>();

        var createdByPhone = new Dictionary<string, Contact>(StringComparer.Ordinal);
        var createdByEmail = new Dictionary<string, Contact>(StringComparer.OrdinalIgnoreCase);

        foreach (var (parsed, primaryNorm, primaryEmail, row) in rowsToProcess)
        {
            var rowEmails = row.Emails
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            var contact = ResolveContact(
                primaryNorm,
                primaryEmail,
                rowEmails,
                contactsForUpdate,
                phoneDict,
                emailDict,
                createdByPhone,
                createdByEmail);

            try
            {
                if (contact is not null)
                {
                    if (contact.IsDeleted)
                        contact.Restore();

                    ApplyImportToContact(contact, row, phoneDict, emailDict);
                    updated++;
                }
                else
                {
                    contact = CreateContactFromRow(row, phoneDict, emailDict);
                    foreach (var em in contact.Emails.Select(e => e.Value))
                    {
                        createdByEmail[em] = contact;
                        emailDict[em] = contact.Id;
                    }
                    foreach (var phone in contact.Phones)
                    {
                        if (string.IsNullOrEmpty(phone.NormalizedValue)) continue;
                        createdByPhone[phone.NormalizedValue] = contact;
                        phoneDict[phone.NormalizedValue] = contact.Id;
                    }
                    _dbContext.Contacts.Add(contact);
                    inserted++;
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ImportRowError { RowNumber = parsed.RowNumber, RawData = FormatRow(row), Error = ex.Message });
                skipped++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpsertContactsResult
        {
            InsertedCount = inserted,
            UpdatedCount = updated,
            SkippedCount = skipped,
            Errors = errors
        };
    }

    /// <summary>
    /// Email-first. Phone match only when it does not collide with a different email identity
    /// (e.g. Fredrik 0936183322 vs Melissa +84936183322).
    /// </summary>
    private static Contact? ResolveContact(
        string? primaryNorm,
        string? primaryEmail,
        IReadOnlyList<string> rowEmails,
        IReadOnlyDictionary<Guid, Contact> contactsForUpdate,
        IReadOnlyDictionary<string, Guid> phoneDict,
        IReadOnlyDictionary<string, Guid> emailDict,
        IReadOnlyDictionary<string, Contact> createdByPhone,
        IReadOnlyDictionary<string, Contact> createdByEmail)
    {
        if (!string.IsNullOrEmpty(primaryEmail))
        {
            if (emailDict.TryGetValue(primaryEmail, out var emailOwnerId) &&
                contactsForUpdate.TryGetValue(emailOwnerId, out var byEmailDb))
                return byEmailDb;

            if (createdByEmail.TryGetValue(primaryEmail, out var byEmailBatch))
                return byEmailBatch;
        }

        if (string.IsNullOrEmpty(primaryNorm))
            return null;

        Contact? byPhone = null;
        if (phoneDict.TryGetValue(primaryNorm, out var phoneOwnerId))
            contactsForUpdate.TryGetValue(phoneOwnerId, out byPhone);
        if (byPhone is null)
            createdByPhone.TryGetValue(primaryNorm, out byPhone);

        if (byPhone is null)
            return null;

        if (PhoneMatchConflictsWithEmails(byPhone, rowEmails, primaryEmail, emailDict))
            return null;

        return byPhone;
    }

    private static bool PhoneMatchConflictsWithEmails(
        Contact phoneOwner,
        IReadOnlyList<string> rowEmails,
        string? primaryEmail,
        IReadOnlyDictionary<string, Guid> emailDict)
    {
        if (rowEmails.Count == 0)
            return false;

        if (!string.IsNullOrEmpty(primaryEmail) &&
            emailDict.TryGetValue(primaryEmail, out var emailOwnerId) &&
            emailOwnerId != phoneOwner.Id)
            return true;

        if (phoneOwner.Emails.Count == 0)
            return false;

        var ownerEmails = phoneOwner.Emails
            .Select(e => e.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return !rowEmails.Any(ownerEmails.Contains);
    }

    private Contact CreateContactFromRow(ContactImportRow row, Dictionary<string, Guid> phoneDict, Dictionary<string, Guid> emailDict)
    {
        var contact = Contact.Create(
            row.FirstName.Trim(),
            row.LastName.Trim(),
            row.Company?.Trim(),
            row.JobTitle?.Trim(),
            null,
            row.Birthday,
            row.Notes?.Trim());

        var emails = row.Emails.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim().ToLowerInvariant()).ToList();
        for (var i = 0; i < emails.Count; i++)
        {
            if (emailDict.ContainsKey(emails[i])) continue;
            contact.AddEmail(emails[i], contact.Emails.Count == 0);
        }

        var phones = row.Phones.Select(SanitizePhone).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        for (var i = 0; i < phones.Count; i++)
        {
            var p = phones[i];
            var norm = PhoneNormalizer.Normalize(p);
            if (string.IsNullOrEmpty(norm) || phoneDict.ContainsKey(norm)) continue;
            contact.AddPhone(p!, contact.Phones.Count == 0);
        }

        foreach (var tag in NormalizeTags(row.Tags))
            try { contact.AddTag(tag); } catch (InvalidOperationException) { /* duplicate */ }

        var addressFirst = true;
        foreach (var addr in row.Addresses)
        {
            var street = BuildImportStreet(addr);
            if (string.IsNullOrWhiteSpace(street)) continue;
            // Import: keep full free-text on Street; CountryCode holds CSV country name or IMPORT sentinel.
            contact.AddAddress(street, ResolveImportCountryCode(addr), "", "", "", addressFirst);
            addressFirst = false;
        }

        foreach (var link in row.SocialLinks)
            if (!string.IsNullOrWhiteSpace(link.Platform) && !string.IsNullOrWhiteSpace(link.Url))
                contact.AddSocialLink(link.Platform.Trim(), link.Url.Trim());

        var bankFirst = true;
        foreach (var ba in row.BankAccounts)
        {
            if (!TryResolveImportBankCode(ba.BankCode, out var bankCode)) continue;
            if (string.IsNullOrWhiteSpace(ba.AccountNumber) || string.IsNullOrWhiteSpace(ba.AccountName) || string.IsNullOrWhiteSpace(ba.CurrencyCode))
                continue;
            var encrypted = _encryptionService.Encrypt(ba.AccountNumber);
            var last4 = ba.AccountNumber.Length >= 4 ? ba.AccountNumber[^4..] : ba.AccountNumber;
            var branch = Truncate(ba.BranchName?.Trim(), BranchNameMaxLength);
            contact.AddBankAccount(bankCode, branch, encrypted, last4, ba.AccountName.Trim(), ba.CurrencyCode.Trim().ToUpperInvariant(), ba.IsPrimary && bankFirst);
            bankFirst = false;
        }

        if (row.IdentityDocument is { } doc && CanImportIdentityDocument(doc))
        {
            var encrypted = _encryptionService.Encrypt(doc.DocumentNumber);
            var last4 = doc.DocumentNumber.Length >= 4 ? doc.DocumentNumber[^4..] : doc.DocumentNumber;
            contact.SetIdentityDocument(
                doc.DocumentType.Trim(),
                encrypted,
                last4,
                doc.IssuedBy?.Trim() ?? "",
                doc.IssuedPlace?.Trim() ?? "",
                doc.IssuedDate,
                doc.ExpiryDate);
        }

        return contact;
    }

    /// <summary>
    /// Prefer moving this onto Contact as ApplyImport(...).
    /// Rule: never steal/move ContactEmail/Phone across contacts; each child stays on its owner.
    /// Identities are additive (Ensure*) — no Remove+Add of the same unique value.
    /// </summary>
    private void ApplyImportToContact(Contact contact, ContactImportRow row, Dictionary<string, Guid> phoneDict, Dictionary<string, Guid> emailDict)
    {
        contact.UpdateProfile(row.FirstName.Trim(), row.LastName.Trim(), row.Company?.Trim(), row.JobTitle?.Trim(), null, row.Birthday, row.Notes?.Trim());

        var desiredEmails = row.Emails.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim().ToLowerInvariant()).Distinct().ToList();
        EnsureEmails(contact, desiredEmails, emailDict);

        var desiredPhones = row.Phones.Select(SanitizePhone).Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => (Raw: p!, Norm: PhoneNormalizer.Normalize(p))).Where(x => !string.IsNullOrEmpty(x.Norm)).DistinctBy(x => x.Norm).Select(x => x.Raw!).ToList();
        EnsurePhones(contact, desiredPhones, phoneDict);

        EnsureTags(contact, NormalizeTags(row.Tags).ToList());

        if (row.Addresses.Count > 0)
            EnsureAddresses(contact, row.Addresses);

        if (row.SocialLinks.Count > 0)
            EnsureSocialLinks(contact, row.SocialLinks);

        if (row.BankAccounts.Count > 0)
            EnsureBankAccounts(contact, row.BankAccounts);

        if (row.IdentityDocument is { } doc)
            EnsureIdentityDocument(contact, doc);
    }

    private static void EnsureEmails(Contact contact, IReadOnlyList<string> desired, Dictionary<string, Guid> emailDict)
    {
        foreach (var value in desired)
        {
            if (contact.Emails.Any(x => x.Value.Equals(value, StringComparison.OrdinalIgnoreCase)))
            {
                emailDict[value] = contact.Id;
                continue;
            }

            // Owned by another contact — that contact keeps its email; we do not move it.
            if (emailDict.TryGetValue(value, out var ownerId) && ownerId != contact.Id)
                continue;

            contact.AddEmail(value, false);
            emailDict[value] = contact.Id;
        }

        var firstDesired = desired.FirstOrDefault();
        if (firstDesired is null) return;

        var primary = contact.Emails.FirstOrDefault(e => e.Value.Equals(firstDesired, StringComparison.OrdinalIgnoreCase));
        if (primary is { } && !primary.IsPrimary)
            contact.SetPrimaryEmail(primary.Id);
    }

    private static void EnsurePhones(Contact contact, IReadOnlyList<string> desiredRaw, Dictionary<string, Guid> phoneDict)
    {
        var desiredNorm = desiredRaw.Select(PhoneNormalizer.Normalize).Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();

        foreach (var raw in desiredRaw)
        {
            var norm = PhoneNormalizer.Normalize(SanitizePhone(raw));
            if (string.IsNullOrEmpty(norm)) continue;

            if (contact.Phones.Any(x => x.NormalizedValue == norm))
            {
                phoneDict[norm] = contact.Id;
                continue;
            }

            if (phoneDict.TryGetValue(norm, out var ownerId) && ownerId != contact.Id)
                continue;

            contact.AddPhoneIfNew(raw, false);
            phoneDict[norm] = contact.Id;
        }

        var firstNorm = desiredNorm.FirstOrDefault();
        if (firstNorm is null) return;

        var primary = contact.Phones.FirstOrDefault(p => p.NormalizedValue == firstNorm);
        if (primary is { } && !primary.IsPrimary)
            contact.SetPrimaryPhone(primary.Id);
    }

    private static void EnsureTags(Contact contact, IReadOnlyList<string> desired)
    {
        foreach (var name in desired)
        {
            if (contact.Tags.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                continue;
            try { contact.AddTag(name); } catch (InvalidOperationException) { /* duplicate */ }
        }
    }

    private static void EnsureAddresses(Contact contact, IReadOnlyList<ContactImportAddress> desired)
    {
        var firstPrimary = !contact.Addresses.Any();
        foreach (var d in desired)
        {
            var street = BuildImportStreet(d);
            if (string.IsNullOrWhiteSpace(street)) continue;
            if (contact.Addresses.Any(a => string.Equals(street, a.Street, StringComparison.Ordinal)))
                continue;
            contact.AddAddress(street, ResolveImportCountryCode(d), "", "", "", firstPrimary);
            firstPrimary = false;
        }
    }

    /// <summary>
    /// Import keeps the whole address as one Street line (CSV uses names/free text, not master codes).
    /// </summary>
    private static string ResolveImportCountryCode(ContactImportAddress addr) =>
        string.IsNullOrWhiteSpace(addr.CountryCode) ? "IMPORT" : addr.CountryCode.Trim();

    private static string BuildImportStreet(ContactImportAddress addr)
    {
        var parts = new[]
            {
          addr.Street,
          addr.DistrictCode,
          addr.CityCode,
          addr.CountryCode,
          addr.PostalCode
        }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p!.Trim());

        var street = string.Join(", ", parts);
        return street.Length <= StreetMaxLength ? street : street[..StreetMaxLength];
    }

    private static void EnsureSocialLinks(Contact contact, IReadOnlyList<ContactImportSocialLink> desired)
    {
        foreach (var d in desired)
        {
            if (string.IsNullOrWhiteSpace(d.Platform) || string.IsNullOrWhiteSpace(d.Url)) continue;
            var platform = d.Platform.Trim();
            var url = d.Url.Trim();
            if (contact.SocialLinks.Any(l => l.Platform.Equals(platform, StringComparison.OrdinalIgnoreCase)))
                continue;
            contact.AddSocialLink(platform, url);
        }
    }

    private void EnsureBankAccounts(Contact contact, IReadOnlyList<ContactImportBankAccount> desired)
    {
        var firstPrimary = !contact.BankAccounts.Any();
        foreach (var d in desired)
        {
            if (!TryResolveImportBankCode(d.BankCode, out var bankCode)) continue;
            if (string.IsNullOrWhiteSpace(d.AccountNumber) || string.IsNullOrWhiteSpace(d.AccountName) || string.IsNullOrWhiteSpace(d.CurrencyCode))
                continue;
            var accountName = d.AccountName.Trim();
            var currencyCode = d.CurrencyCode.Trim().ToUpperInvariant();
            if (contact.BankAccounts.Any(ba => ba.BankCode == bankCode && ba.AccountName == accountName && ba.CurrencyCode == currencyCode))
                continue;

            var encrypted = _encryptionService.Encrypt(d.AccountNumber);
            var last4 = d.AccountNumber.Length >= 4 ? d.AccountNumber[^4..] : d.AccountNumber;
            var branch = Truncate(d.BranchName?.Trim(), BranchNameMaxLength);
            contact.AddBankAccount(bankCode, branch, encrypted, last4, accountName, currencyCode, d.IsPrimary && firstPrimary);
            firstPrimary = false;
        }
    }

    private void EnsureIdentityDocument(Contact contact, ContactImportIdentityDocument doc)
    {
        // Titan CSV has CMND + number + date but empty IssuedBy/IssuedPlace — skip incomplete docs
        // so the contact itself still imports (Niteco rows have no identity columns).
        if (!CanImportIdentityDocument(doc))
            return;

        var encrypted = _encryptionService.Encrypt(doc.DocumentNumber);
        var last4 = doc.DocumentNumber.Length >= 4 ? doc.DocumentNumber[^4..] : doc.DocumentNumber;
        var issuedBy = doc.IssuedBy?.Trim() ?? "";
        var issuedPlace = doc.IssuedPlace?.Trim() ?? "";

        if (contact.IdentityDocument is null)
            contact.SetIdentityDocument(doc.DocumentType.Trim(), encrypted, last4, issuedBy, issuedPlace, doc.IssuedDate, doc.ExpiryDate);
        else
            contact.UpdateIdentityDocument(contact.IdentityDocument.Id, doc.DocumentType.Trim(), encrypted, last4, issuedBy, issuedPlace, doc.IssuedDate, doc.ExpiryDate);
    }

    private static bool CanImportIdentityDocument(ContactImportIdentityDocument doc) =>
        !string.IsNullOrWhiteSpace(doc.DocumentType)
        && !string.IsNullOrWhiteSpace(doc.DocumentNumber)
        && !string.IsNullOrWhiteSpace(doc.IssuedBy)
        && !string.IsNullOrWhiteSpace(doc.IssuedPlace);

    /// <summary>
    /// CSV uses bank names (Techcombank) or junk (false). Master Banks.Code is TCB, TPB, …
    /// </summary>
    private static bool TryResolveImportBankCode(string? raw, out string bankCode)
    {
        bankCode = "";
        if (string.IsNullOrWhiteSpace(raw)) return false;

        var value = raw.Trim();
        if (value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("true", StringComparison.OrdinalIgnoreCase))
            return false;

        bankCode = value.Equals("Techcombank", StringComparison.OrdinalIgnoreCase)
            ? "TCB"
            : value.ToUpperInvariant();

        if (bankCode.Length > BankCodeMaxLength)
            bankCode = bankCode[..BankCodeMaxLength];

        return bankCode.Length > 0;
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static IEnumerable<string> NormalizeTags(IReadOnlyList<string> tags)
    {
        return tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static string? SanitizePhone(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = raw.Trim();
        var idx = s.IndexOf(" ::: ", StringComparison.Ordinal);
        if (idx >= 0) s = s[..idx].Trim();
        if (string.IsNullOrWhiteSpace(s)) return null;
        return s.Length > PhoneValueMaxLength ? s[..PhoneValueMaxLength] : s;
    }

    private static string FormatRow(ContactImportRow row) =>
        $"{row.FirstName},{row.LastName},{string.Join(";", row.Emails)},{string.Join(";", row.Phones)},{row.Company},{row.JobTitle},{row.Birthday},{row.Notes},{string.Join(";", row.Tags)}";
}
