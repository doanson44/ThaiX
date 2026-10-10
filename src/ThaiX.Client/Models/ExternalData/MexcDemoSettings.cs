namespace ThaiX.Client.Models.ExternalData;

public sealed record MexcDemoSettings(
    decimal InitialCapital,
    decimal FeePercent,
    decimal CapitalPerTradePercent,
    int Leverage);
