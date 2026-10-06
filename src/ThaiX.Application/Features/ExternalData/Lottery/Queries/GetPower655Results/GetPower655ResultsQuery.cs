using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.Lottery.Queries.GetPower655Results;

/// <summary>
/// Query to retrieve Power 6/55 lottery results from ketquadientoan.com.
/// Date range uses dd-MM-yyyy format.
/// </summary>
public sealed record GetPower655ResultsQuery : IAppQuery<Power655ResultsResponse>
{
    public required string DateFrom { get; init; }
    public required string DateTo { get; init; }
}

/// <summary>
/// Response containing Power 6/55 lottery results.
/// </summary>
public sealed record Power655ResultsResponse
{
    public IReadOnlyList<Power655ResultItem> Results { get; init; } = [];
    public int TotalCount { get; init; }
}

/// <summary>
/// A single Power 6/55 draw result.
/// </summary>
public sealed record Power655ResultItem
{
    /// <summary>Draw date (dd/MM/yyyy).</summary>
    public required string DrawDate { get; init; }

    /// <summary>Day of week in Vietnamese (T2, T3, ..., CN).</summary>
    public required string DayOfWeek { get; init; }

    /// <summary>6 main winning numbers.</summary>
    public required IReadOnlyList<int> Numbers { get; init; }

    /// <summary>Bonus (jackpot phu) number.</summary>
    public required int BonusNumber { get; init; }

    /// <summary>Jackpot 1 prize value in VND.</summary>
    public long Jackpot1Value { get; init; }

    /// <summary>Jackpot 2 prize value in VND.</summary>
    public long Jackpot2Value { get; init; }
}
