using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.Lottery;

/// <summary>
/// A single Power 6/55 lottery draw result, sourced from ketquadientoan.com.
/// Draws occur on Tuesday (T3), Thursday (T5), and Saturday (T7).
/// </summary>
public sealed class Power655Result : BaseEntity
{
    private Power655Result() { }

    /// <summary>Draw date (Vietnam timezone).</summary>
    public DateOnly DrawDate { get; private set; }

    /// <summary>Day of week in Vietnamese (T2, T3, T4, T5, T6, T7, CN).</summary>
    public string DayOfWeek { get; private set; } = string.Empty;

    /// <summary>First winning number (01-55).</summary>
    public int Num1 { get; private set; }

    /// <summary>Second winning number (01-55).</summary>
    public int Num2 { get; private set; }

    /// <summary>Third winning number (01-55).</summary>
    public int Num3 { get; private set; }

    /// <summary>Fourth winning number (01-55).</summary>
    public int Num4 { get; private set; }

    /// <summary>Fifth winning number (01-55).</summary>
    public int Num5 { get; private set; }

    /// <summary>Sixth winning number (01-55).</summary>
    public int Num6 { get; private set; }

    /// <summary>Bonus number for Jackpot 2 (01-55).</summary>
    public int BonusNum { get; private set; }

    /// <summary>Jackpot 1 prize value in VND. 0 means no winner.</summary>
    public long Jackpot1Value { get; private set; }

    /// <summary>Jackpot 2 prize value in VND. 0 means no winner.</summary>
    public long Jackpot2Value { get; private set; }

    public static Power655Result Create(
        DateOnly drawDate,
        string dayOfWeek,
        int num1,
        int num2,
        int num3,
        int num4,
        int num5,
        int num6,
        int bonusNum,
        long jackpot1Value,
        long jackpot2Value)
    {
        return new Power655Result
        {
            Id = Guid.NewGuid(),
            DrawDate = drawDate,
            DayOfWeek = dayOfWeek,
            Num1 = num1,
            Num2 = num2,
            Num3 = num3,
            Num4 = num4,
            Num5 = num5,
            Num6 = num6,
            BonusNum = bonusNum,
            Jackpot1Value = jackpot1Value,
            Jackpot2Value = jackpot2Value
        };
    }
}
