using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.Contacts;

/// <summary>
/// Contact aggregate root.
/// Manages personal information, communication channels, addresses,
/// social links, tags, and bank accounts for a contact.
/// </summary>
public sealed class Contact : BaseAuditableEntity
{
    private readonly List<ContactEmail> _emails = new();
    private readonly List<ContactPhone> _phones = new();
    private readonly List<ContactAddress> _addresses = new();
    private readonly List<ContactSocialLink> _socialLinks = new();
    private readonly List<ContactTag> _tags = new();
    private readonly List<ContactBankAccount> _bankAccounts = new();
    private ContactIdentityDocument? _identityDocument;

    /// <summary>
    /// Contact's full name (value object, stored as owned columns).
    /// </summary>
    public FullName FullName { get; private set; } = null!;

    /// <summary>
    /// Email addresses associated with the contact.
    /// </summary>
    public IReadOnlyCollection<ContactEmail> Emails => _emails.AsReadOnly();

    /// <summary>
    /// Phone numbers associated with the contact.
    /// </summary>
    public IReadOnlyCollection<ContactPhone> Phones => _phones.AsReadOnly();

    /// <summary>
    /// Physical addresses associated with the contact.
    /// </summary>
    public IReadOnlyCollection<ContactAddress> Addresses => _addresses.AsReadOnly();

    /// <summary>
    /// Social media links associated with the contact.
    /// </summary>
    public IReadOnlyCollection<ContactSocialLink> SocialLinks => _socialLinks.AsReadOnly();

    /// <summary>
    /// Tags/labels applied to the contact.
    /// </summary>
    public IReadOnlyCollection<ContactTag> Tags => _tags.AsReadOnly();

    /// <summary>
    /// Bank accounts belonging to the contact.
    /// </summary>
    public IReadOnlyCollection<ContactBankAccount> BankAccounts => _bankAccounts.AsReadOnly();

    /// <summary>
    /// The single identity document (passport, national ID, etc.) for the contact. At most one per contact.
    /// </summary>
    public ContactIdentityDocument? IdentityDocument => _identityDocument;

    /// <summary>
    /// Arbitrary key-value metadata. Stored as JSON.
    /// </summary>
    public Dictionary<string, string> CustomFields { get; private set; } = new();

    /// <summary>
    /// Company / organization the contact belongs to.
    /// </summary>
    public string? Company { get; private set; }

    /// <summary>
    /// Job title / role of the contact.
    /// </summary>
    public string? JobTitle { get; private set; }

    /// <summary>
    /// URL of the contact's avatar image.
    /// </summary>
    public string? AvatarUrl { get; private set; }

    /// <summary>
    /// Contact's date of birth.
    /// </summary>
    public DateOnly? Birthday { get; private set; }

    /// <summary>
    /// Free-text notes about the contact.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Whether the contact is archived (hidden from default views).
    /// </summary>
    public bool IsArchived { get; private set; }

    // Private constructor for EF Core
    private Contact() { }

    /// <summary>
    /// Creates a new Contact with basic profile information.
    /// </summary>
    public static Contact Create(
        string firstName,
        string lastName,
        string? company = null,
        string? jobTitle = null,
        string? avatarUrl = null,
        DateOnly? birthday = null,
        string? notes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName, nameof(firstName));
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName, nameof(lastName));

        return new Contact
        {
            Id = Guid.NewGuid(),
            FullName = new FullName(firstName, lastName),
            Company = company?.Trim(),
            JobTitle = jobTitle?.Trim(),
            AvatarUrl = avatarUrl?.Trim(),
            Birthday = birthday,
            Notes = notes?.Trim(),
            IsArchived = false,
            CustomFields = new()
        };
    }

    /// <summary>
    /// Updates the contact's profile fields.
    /// </summary>
    public void UpdateProfile(
        string firstName,
        string lastName,
        string? company,
        string? jobTitle,
        string? avatarUrl,
        DateOnly? birthday,
        string? notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName, nameof(firstName));
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName, nameof(lastName));

        FullName = new FullName(firstName, lastName);
        Company = company?.Trim();
        JobTitle = jobTitle?.Trim();
        AvatarUrl = avatarUrl?.Trim();
        Birthday = birthday;
        Notes = notes?.Trim();
    }

    /// <summary>
    /// Sets the contact's avatar URL (e.g. after upload). No public setter; use this method.
    /// </summary>
    public void SetAvatar(string? avatarUrl)
    {
        AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
    }

    #region Email

    public void AddEmail(string value, bool isPrimary)
    {
        if (isPrimary)
            _emails.ForEach(e => e.SetPrimary(false));

        _emails.Add(ContactEmail.Create(Id, value, isPrimary));
    }

    public void RemoveEmail(Guid emailId)
    {
        var email = _emails.FirstOrDefault(e => e.Id == emailId)
            ?? throw new InvalidOperationException($"Email with ID '{emailId}' not found.");
        _emails.Remove(email);
    }

    /// <summary>
    /// Sets the primary email by id. All other emails are set non-primary.
    /// </summary>
    public void SetPrimaryEmail(Guid emailId)
    {
        var email = _emails.FirstOrDefault(e => e.Id == emailId)
            ?? throw new InvalidOperationException($"Email with ID '{emailId}' not found.");
        _emails.ForEach(e => e.SetPrimary(e.Id == emailId));
    }

    #endregion

    #region Phone

    public void AddPhone(string value, bool isPrimary)
    {
        var normalized = PhoneNormalizer.Normalize(value);
        if (string.IsNullOrEmpty(normalized))
            throw new ArgumentException("Phone number has no valid digits.", nameof(value));
        if (_phones.Any(p => p.NormalizedValue == normalized))
            throw new InvalidOperationException($"Phone with normalized value '{normalized}' already exists on this contact.");

        if (isPrimary)
            _phones.ForEach(p => p.SetPrimary(false));

        _phones.Add(ContactPhone.Create(Id, value, isPrimary));
    }

    /// <summary>
    /// Adds a phone if no phone with the same normalized value exists on this contact.
    /// Used by import to avoid duplicate numbers. Primary is set only when adding the first phone.
    /// </summary>
    public void AddPhoneIfNew(string value, bool setPrimaryIfFirst)
    {
        var normalized = PhoneNormalizer.Normalize(value);
        if (string.IsNullOrEmpty(normalized))
            return;
        if (_phones.Any(p => p.NormalizedValue == normalized))
            return;
        var isFirst = _phones.Count == 0;
        AddPhone(value, setPrimaryIfFirst && isFirst);
    }

    public void RemovePhone(Guid phoneId)
    {
        var phone = _phones.FirstOrDefault(p => p.Id == phoneId)
            ?? throw new InvalidOperationException($"Phone with ID '{phoneId}' not found.");
        _phones.Remove(phone);
    }

    /// <summary>
    /// Sets the primary phone by id. All other phones are set non-primary.
    /// </summary>
    public void SetPrimaryPhone(Guid phoneId)
    {
        var phone = _phones.FirstOrDefault(p => p.Id == phoneId)
            ?? throw new InvalidOperationException($"Phone with ID '{phoneId}' not found.");
        _phones.ForEach(p => p.SetPrimary(p.Id == phoneId));
    }

    #endregion

    #region Address

    public void AddAddress(
        string street,
        string countryCode,
        string cityCode,
        string districtCode,
        string postalCode,
        bool isPrimary)
    {
        if (isPrimary)
            _addresses.ForEach(a => a.SetPrimary(false));

        _addresses.Add(ContactAddress.Create(Id, street, countryCode, cityCode, districtCode, postalCode, isPrimary));
    }

    public void RemoveAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new InvalidOperationException($"Address with ID '{addressId}' not found.");
        _addresses.Remove(address);
    }

    #endregion

    #region Social Link

    public void AddSocialLink(string platform, string url)
    {
        _socialLinks.Add(ContactSocialLink.Create(Id, platform, url));
    }

    public void RemoveSocialLink(Guid socialLinkId)
    {
        var link = _socialLinks.FirstOrDefault(l => l.Id == socialLinkId)
            ?? throw new InvalidOperationException($"Social link with ID '{socialLinkId}' not found.");
        _socialLinks.Remove(link);
    }

    #endregion

    #region Tag

    public void AddTag(string name)
    {
        if (_tags.Any(t => t.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Tag '{name}' already exists on this contact.");

        _tags.Add(ContactTag.Create(Id, name));
    }

    public void RemoveTag(Guid tagId)
    {
        var tag = _tags.FirstOrDefault(t => t.Id == tagId)
            ?? throw new InvalidOperationException($"Tag with ID '{tagId}' not found.");
        _tags.Remove(tag);
    }

    #endregion

    #region Bank Account

    public void AddBankAccount(
        string bankCode,
        string? branchName,
        string encryptedAccountNumber,
        string accountNumberLast4,
        string accountName,
        string currencyCode,
        bool isPrimary)
    {
        if (isPrimary)
            _bankAccounts.ForEach(ba => ba.SetPrimary(false));

        _bankAccounts.Add(ContactBankAccount.Create(
            Id, bankCode, branchName, encryptedAccountNumber,
            accountNumberLast4, accountName, currencyCode, isPrimary));
    }

    public void RemoveBankAccount(Guid bankAccountId)
    {
        var account = _bankAccounts.FirstOrDefault(ba => ba.Id == bankAccountId)
            ?? throw new InvalidOperationException($"Bank account with ID '{bankAccountId}' not found.");
        _bankAccounts.Remove(account);
    }

    #endregion

    #region Identity Document

    /// <summary>
    /// Sets the contact's identity document. At most one allowed; throws if one already exists.
    /// </summary>
    public void SetIdentityDocument(
        string documentType,
        string encryptedDocumentNumber,
        string documentNumberLast4,
        string issuedBy,
        string issuedPlace,
        DateOnly issuedDate,
        DateOnly? expiryDate)
    {
        if (_identityDocument != null)
            throw new InvalidOperationException("Contact already has an identity document; use UpdateIdentityDocument or RemoveIdentityDocument first.");

        _identityDocument = ContactIdentityDocument.Create(
            Id, documentType, encryptedDocumentNumber, documentNumberLast4,
            issuedBy, issuedPlace, issuedDate, expiryDate);
    }

    /// <summary>
    /// Updates the existing identity document. Throws if none exists or id does not match.
    /// </summary>
    public void UpdateIdentityDocument(
        Guid identityDocumentId,
        string documentType,
        string encryptedDocumentNumber,
        string documentNumberLast4,
        string issuedBy,
        string issuedPlace,
        DateOnly issuedDate,
        DateOnly? expiryDate)
    {
        if (_identityDocument is null)
            throw new InvalidOperationException("Contact has no identity document to update.");
        if (_identityDocument.Id != identityDocumentId)
            throw new InvalidOperationException($"Identity document with ID '{identityDocumentId}' not found.");

        _identityDocument.Update(
            documentType, encryptedDocumentNumber, documentNumberLast4,
            issuedBy, issuedPlace, issuedDate, expiryDate);
    }

    /// <summary>
    /// Removes the identity document. Throws if none exists or id does not match.
    /// Caller must remove the entity from the context after this.
    /// </summary>
    public void RemoveIdentityDocument(Guid identityDocumentId)
    {
        if (_identityDocument is null)
            throw new InvalidOperationException("Contact has no identity document to remove.");
        if (_identityDocument.Id != identityDocumentId)
            throw new InvalidOperationException($"Identity document with ID '{identityDocumentId}' not found.");

        _identityDocument = null;
    }

    #endregion

    #region Custom Fields

    public void SetCustomField(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
        CustomFields[key.Trim()] = value;
    }

    public void RemoveCustomField(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
        CustomFields.Remove(key.Trim());
    }

    #endregion

    #region Lifecycle

    /// <summary>
    /// Archives the contact (hides from default views).
    /// </summary>
    public void Archive()
    {
        IsArchived = true;
    }

    /// <summary>
    /// Restores an archived contact to active state.
    /// </summary>
    public void Unarchive()
    {
        IsArchived = false;
    }

    /// <summary>
    /// Soft-deletes the contact.
    /// </summary>
    public void SoftDelete()
    {
        Delete();
    }

    /// <summary>
    /// Restores a soft-deleted contact.
    /// </summary>
    public void RestoreEntity()
    {
        Restore();
    }

    #endregion
}
