namespace ThaiX.Client.Models.MarketScanner;

public sealed record MarketScannerExchangeSettingsModel
{
    public bool RequiredBybit { get; set; }
    public bool RequiredBinance { get; set; }
}