namespace ThaiX.Client.Models.Lottery;

public sealed record Power655AnalysisResponse
{
    public IReadOnlyList<Power655DrawDto> Draws { get; init; } = [];
    public IReadOnlyList<NumberFrequencyRecord> TopMostFrequent { get; init; } = [];
    public IReadOnlyList<NumberFrequencyRecord> TopLeastFrequent { get; init; } = [];
    public IReadOnlyList<Power655PredictionDto> Predictions { get; init; } = [];
}

public sealed record Power655DrawDto
{
    public Guid Id { get; init; }
    public DateOnly DrawDate { get; init; }
    public string DayOfWeek { get; init; } = string.Empty;
    public IReadOnlyList<int> Numbers { get; init; } = [];
    public int BonusNum { get; init; }
    public long Jackpot1Value { get; init; }
    public long Jackpot2Value { get; init; }
}

public sealed record NumberFrequencyRecord
{
    public int Number { get; init; }
    public int Frequency { get; init; }
}

public sealed record Power655PredictionDto
{
    public Guid Id { get; init; }
    public DateOnly TargetDrawDate { get; init; }
    public string PredictionType { get; init; } = string.Empty;
    public IReadOnlyList<int> Numbers { get; init; } = [];
    public int TupleFrequency { get; init; }
    public string Reasoning { get; init; } = string.Empty;
    public int MatchedNumbers { get; init; }
    public bool IsCompleted { get; init; }
}
