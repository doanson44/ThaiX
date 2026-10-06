namespace ThaiX.Domain.Aggregates.Contacts;

/// <summary>
/// Represents the single identity document (e.g. passport, national ID) for a contact.
/// Document number is stored encrypted; only the last 4 digits are kept in plain text.
/// A contact has at most one identity document.
/// </summary>
public sealed class ContactIdentityDocument
{
    public Guid Id { get; private set; }
    public Guid ContactId { get; private set; }
    public string DocumentType { get; private set; } = string.Empty;
    public string EncryptedDocumentNumber { get; private set; } = string.Empty;
    public string DocumentNumberLast4 { get; private set; } = string.Empty;
    public string IssuedBy { get; private set; } = string.Empty;
    public string IssuedPlace { get; private set; } = string.Empty;
    public DateOnly IssuedDate { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }

    // Private constructor for EF Core
    private ContactIdentityDocument() { }

    public static ContactIdentityDocument Create(
        Guid contactId,
        string documentType,
        string encryptedDocumentNumber,
        string documentNumberLast4,
        string issuedBy,
        string issuedPlace,
        DateOnly issuedDate,
        DateOnly? expiryDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentType, nameof(documentType));
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedDocumentNumber, nameof(encryptedDocumentNumber));
        ArgumentException.ThrowIfNullOrWhiteSpace(documentNumberLast4, nameof(documentNumberLast4));
        ArgumentException.ThrowIfNullOrWhiteSpace(issuedBy, nameof(issuedBy));
        ArgumentException.ThrowIfNullOrWhiteSpace(issuedPlace, nameof(issuedPlace));

        return new ContactIdentityDocument
        {
            Id = Guid.NewGuid(),
            ContactId = contactId,
            DocumentType = documentType.Trim(),
            EncryptedDocumentNumber = encryptedDocumentNumber,
            DocumentNumberLast4 = documentNumberLast4,
            IssuedBy = issuedBy.Trim(),
            IssuedPlace = issuedPlace.Trim(),
            IssuedDate = issuedDate,
            ExpiryDate = expiryDate
        };
    }

    internal void Update(
        string documentType,
        string encryptedDocumentNumber,
        string documentNumberLast4,
        string issuedBy,
        string issuedPlace,
        DateOnly issuedDate,
        DateOnly? expiryDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentType, nameof(documentType));
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedDocumentNumber, nameof(encryptedDocumentNumber));
        ArgumentException.ThrowIfNullOrWhiteSpace(documentNumberLast4, nameof(documentNumberLast4));
        ArgumentException.ThrowIfNullOrWhiteSpace(issuedBy, nameof(issuedBy));
        ArgumentException.ThrowIfNullOrWhiteSpace(issuedPlace, nameof(issuedPlace));

        DocumentType = documentType.Trim();
        EncryptedDocumentNumber = encryptedDocumentNumber;
        DocumentNumberLast4 = documentNumberLast4;
        IssuedBy = issuedBy.Trim();
        IssuedPlace = issuedPlace.Trim();
        IssuedDate = issuedDate;
        ExpiryDate = expiryDate;
    }
}
