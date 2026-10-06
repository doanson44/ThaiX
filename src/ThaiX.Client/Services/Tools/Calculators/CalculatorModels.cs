namespace ThaiX.Client.Services.Tools.Calculators;

public enum TradeSide
{
    Long = 0,
    Short = 1
}

public enum FeeMode
{
    Taker = 0,
    Maker = 1,
    Custom = 2
}

public sealed record CalcLine(string LabelKey, string Value);

public sealed record CalculatorResult(
    IReadOnlyList<CalcLine> Breakdown,
    string Formula,
    string InterpretationKey,
    IReadOnlyList<string>? InterpretationArgs = null);

public sealed class CalculatorInputException : Exception
{
    public CalculatorInputException(string message) : base(message) { }
}
