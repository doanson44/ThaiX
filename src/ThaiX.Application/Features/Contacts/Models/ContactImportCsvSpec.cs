namespace ThaiX.Application.Features.Contacts.Models;

/// <summary>
/// Canonical CSV specification for contact import/export.
/// The primary format follows Google Contacts CSV conventions for standard fields,
/// with ThaiX-specific extension columns appended at the end.
/// The legacy ThaiX format (FirstName, LastName, Email1 ...) is still accepted on import.
/// </summary>
public static class ContactImportCsvSpec
{
    /// <summary>CSV delimiter.</summary>
    public const char Delimiter = ',';

    /// <summary>Encoding for CSV files (UTF-8 with BOM allowed).</summary>
    public const string EncodingName = "UTF-8";

    /// <summary>Separator for multi-value in a single cell (e.g. Tags) in the ThaiX native format.</summary>
    public const string ListSeparator = ";";

    /// <summary>Separator used by Google Contacts for Group Membership values.</summary>
    public const string GoogleListSeparator = " ::: ";

    /// <summary>Date format for Birthday and identity document dates (ISO 8601).</summary>
    public const string DateFormat = "yyyy-MM-dd";

    // -------------------------------------------------------------------------
    // Google Contacts CSV column names (primary/preferred format)
    // -------------------------------------------------------------------------

    public const string G_GivenName = "Given Name";
    public const string G_FamilyName = "Family Name";
    public const string G_Birthday = "Birthday";
    public const string G_Notes = "Notes";
    public const string G_GroupMembership = "Group Membership";

    public const string G_Email1Value = "E-mail 1 - Value";
    public const string G_Email2Value = "E-mail 2 - Value";
    public const string G_Email3Value = "E-mail 3 - Value";

    public const string G_Phone1Value = "Phone 1 - Value";
    public const string G_Phone2Value = "Phone 2 - Value";
    public const string G_Phone3Value = "Phone 3 - Value";

    public const string G_Org1Name = "Organization 1 - Name";
    public const string G_Org1Title = "Organization 1 - Title";

    public const string G_Addr1Street = "Address 1 - Street";
    public const string G_Addr1Country = "Address 1 - Country";
    public const string G_Addr1City = "Address 1 - City";
    public const string G_Addr1Region = "Address 1 - Region";
    public const string G_Addr1PostalCode = "Address 1 - Postal Code";
    public const string G_Addr1ExtendedAddress = "Address 1 - Extended Address";

    public const string G_Web1Type = "Website 1 - Type";
    public const string G_Web1Value = "Website 1 - Value";
    public const string G_Web2Type = "Website 2 - Type";
    public const string G_Web2Value = "Website 2 - Value";

    // -------------------------------------------------------------------------
    // ThaiX extension column names (appended after standard Google columns)
    // Used in both the import template (full values) and export (masked for sensitive fields).
    // -------------------------------------------------------------------------

    public const string DocumentType = "DocumentType";
    /// <summary>Full document number column — for user-filled import template.</summary>
    public const string DocumentNumber = "DocumentNumber";
    /// <summary>Masked last-4 document number column — written in exports (sensitive data).</summary>
    public const string DocumentNumberLast4 = "DocumentNumberLast4";
    public const string IssuedBy = "IssuedBy";
    public const string IssuedPlace = "IssuedPlace";
    public const string IssuedDate = "IssuedDate";
    public const string ExpiryDate = "ExpiryDate";

    public const string BankCode = "BankCode";
    /// <summary>Full bank account number column — for user-filled import template.</summary>
    public const string BankAccountNumber = "BankAccountNumber";
    /// <summary>Masked last-4 account number column — written in exports (sensitive data).</summary>
    public const string BankAccountLast4 = "BankAccountLast4";
    public const string BankAccountName = "BankAccountName";
    public const string BankCurrencyCode = "BankCurrencyCode";

    // -------------------------------------------------------------------------
    // Legacy ThaiX column names (backward-compatible; still accepted on import)
    // -------------------------------------------------------------------------

    public const string FirstName = "FirstName";
    public const string LastName = "LastName";
    public const string Email1 = "Email1";
    public const string Email2 = "Email2";
    public const string Email3 = "Email3";
    public const string Phone1 = "Phone1";
    public const string Phone2 = "Phone2";
    public const string Phone3 = "Phone3";
    public const string Company = "Company";
    public const string JobTitle = "JobTitle";
    public const string Birthday = "Birthday";
    public const string Notes = "Notes";
    public const string Tags = "Tags";
    public const string Street = "Street";
    public const string CountryCode = "CountryCode";
    public const string CityCode = "CityCode";
    public const string DistrictCode = "DistrictCode";
    public const string PostalCode = "PostalCode";
    public const string SocialPlatform1 = "SocialPlatform1";
    public const string SocialUrl1 = "SocialUrl1";
    public const string SocialPlatform2 = "SocialPlatform2";
    public const string SocialUrl2 = "SocialUrl2";

    // -------------------------------------------------------------------------
    // Header collections
    // -------------------------------------------------------------------------

    /// <summary>
    /// Required headers when the file uses the legacy ThaiX format (FirstName, LastName).
    /// </summary>
    public static readonly IReadOnlyList<string> RequiredHeaders = new[] { FirstName, LastName };

    /// <summary>
    /// Required headers when the file uses the Google Contacts format.
    /// </summary>
    public static readonly IReadOnlyList<string> GoogleRequiredHeaders = new[] { G_GivenName, G_FamilyName };

    /// <summary>
    /// Google Contacts import template column headers.
    /// Standard Google columns first, then ThaiX extension columns.
    /// Sensitive fields use full-value column names so users can enter their own data.
    /// </summary>
    public static readonly IReadOnlyList<string> GoogleTemplateHeaders = new[]
    {
        G_GivenName, G_FamilyName, G_Birthday, G_Notes, G_GroupMembership,
        G_Email1Value, G_Email2Value, G_Email3Value,
        G_Phone1Value, G_Phone2Value, G_Phone3Value,
        G_Org1Name, G_Org1Title,
        G_Addr1Street, G_Addr1Country, G_Addr1City, G_Addr1Region, G_Addr1PostalCode, G_Addr1ExtendedAddress,
        G_Web1Type, G_Web1Value, G_Web2Type, G_Web2Value,
        DocumentType, DocumentNumber, IssuedBy, IssuedPlace, IssuedDate, ExpiryDate,
        BankCode, BankAccountNumber, BankAccountName, BankCurrencyCode
    };

    /// <summary>
    /// Google Contacts export column headers.
    /// Identical to <see cref="GoogleTemplateHeaders"/> except sensitive columns use Last4 variants.
    /// </summary>
    public static readonly IReadOnlyList<string> GoogleExportHeaders = new[]
    {
        G_GivenName, G_FamilyName, G_Birthday, G_Notes, G_GroupMembership,
        G_Email1Value, G_Email2Value, G_Email3Value,
        G_Phone1Value, G_Phone2Value, G_Phone3Value,
        G_Org1Name, G_Org1Title,
        G_Addr1Street, G_Addr1Country, G_Addr1City, G_Addr1Region, G_Addr1PostalCode, G_Addr1ExtendedAddress,
        G_Web1Type, G_Web1Value, G_Web2Type, G_Web2Value,
        DocumentType, DocumentNumberLast4, IssuedBy, IssuedPlace, IssuedDate, ExpiryDate,
        BankCode, BankAccountLast4, BankAccountName, BankCurrencyCode
    };

    /// <summary>Legacy ThaiX format headers (kept for backward compatibility).</summary>
    public static readonly IReadOnlyList<string> AllHeaders = new[]
    {
        FirstName, LastName,
        Email1, Email2, Email3,
        Phone1, Phone2, Phone3,
        Company, JobTitle, Birthday, Notes, Tags,
        Street, CountryCode, CityCode, DistrictCode, PostalCode,
        DocumentType, DocumentNumber, IssuedBy, IssuedPlace, IssuedDate, ExpiryDate,
        SocialPlatform1, SocialUrl1, SocialPlatform2, SocialUrl2,
        BankCode, BankAccountNumber, BankAccountName, BankCurrencyCode
    };
}
