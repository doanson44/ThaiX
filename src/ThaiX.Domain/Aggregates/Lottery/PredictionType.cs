namespace ThaiX.Domain.Aggregates.Lottery;

/// <summary>
/// Indicates the type of prediction: Pick3 (3 numbers), Pick4 (4 numbers),
/// Pick5 (5 numbers), or Pick6 (6 numbers). The numeric value corresponds
/// to how many numbers are predicted.
/// </summary>
public enum PredictionType
{
    Pick3 = 3,
    Pick4 = 4,
    Pick5 = 5,
    Pick6 = 6
}
