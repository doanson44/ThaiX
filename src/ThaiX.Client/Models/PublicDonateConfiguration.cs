namespace ThaiX.Client.Models;

public sealed record PublicDonateConfigurationResponse
{
    public bool Success { get; init; }
    public PublicDonateConfiguration? Data { get; init; }
}

public sealed record PublicDonateConfiguration
{
    public bool Enabled { get; init; }
    public string QrImage { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public string AccountNumber { get; init; } = string.Empty;
}
