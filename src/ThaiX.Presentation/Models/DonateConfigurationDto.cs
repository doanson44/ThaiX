namespace ThaiX.Presentation.Models;

public sealed record DonateConfigurationDto(
    bool Enabled,
    string QrImage,
    string AccountName,
    string AccountNumber);
