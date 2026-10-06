namespace ThaiX.Application.Features.Contacts.Models;

/// <summary>
/// Identity document data for contact import. DocumentNumber is plain; handler encrypts before storing.
/// </summary>
public sealed record ContactImportIdentityDocument
{
    public required string DocumentType { get; init; }
    public required string DocumentNumber { get; init; }
    public required string IssuedBy { get; init; }
    public required string IssuedPlace { get; init; }
    public required DateOnly IssuedDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }
}
