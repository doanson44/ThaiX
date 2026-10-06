namespace ThaiX.Application.Common.Enums;

/// <summary>
/// Channel through which a bank deposit rate offer is available.
/// </summary>
public enum BankRateChannel
{
    /// <summary>Deposits opened through the online channel.</summary>
    Online = 0,

    /// <summary>Deposits opened at the counter.</summary>
    Offline = 1
}
