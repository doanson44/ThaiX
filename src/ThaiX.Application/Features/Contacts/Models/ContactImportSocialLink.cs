namespace ThaiX.Application.Features.Contacts.Models;

/// <summary>
/// Social link data for contact import. Not a domain entity.
/// </summary>
public sealed record ContactImportSocialLink
{
    public required string Platform { get; init; }
    public required string Url { get; init; }
}
