using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.Lottery;

/// <summary>
/// A prediction for a Power 6/55 draw. Predictions can be Pick3, Pick4, Pick5, or Pick6.
/// The <see cref="Power655ResultId"/> is nullable — set after the actual result is available
/// to link and calculate <see cref="MatchedNumbers"/>.
/// </summary>
public sealed class Power655Prediction : BaseEntity
{
    private Power655Prediction() { }

    /// <summary>The draw date this prediction targets.</summary>
    public DateOnly TargetDrawDate { get; private set; }

    /// <summary>How many numbers were predicted.</summary>
    public PredictionType PredictionType { get; private set; }

    /// <summary>First predicted number (1-55). Always required.</summary>
    public int Num1 { get; private set; }

    /// <summary>Second predicted number (1-55). Always required.</summary>
    public int Num2 { get; private set; }

    /// <summary>Third predicted number (1-55). Always required.</summary>
    public int Num3 { get; private set; }

    /// <summary>Fourth predicted number (1-55).</summary>
    public int Num4 { get; private set; }

    /// <summary>Fifth predicted number (1-55).</summary>
    public int Num5 { get; private set; }

    /// <summary>Sixth predicted number (1-55).</summary>
    public int Num6 { get; private set; }

    /// <summary>How many times this N-tuple combination appeared in historical draws.</summary>
    public int TupleFrequency { get; private set; }

    /// <summary>AI or heuristic reasoning behind the prediction.</summary>
    public string Reasoning { get; private set; } = string.Empty;

    /// <summary>How many predicted numbers matched the actual draw result. 0 until the result is linked.</summary>
    public int MatchedNumbers { get; private set; }

    /// <summary>
    /// FK to the actual <see cref="Power655Result"/> used for comparison.
    /// Null until the draw occurs and the result is available.
    /// </summary>
    public Guid? Power655ResultId { get; private set; }

    // ── Navigation ────────────────────────────────────────────────────────

    public Power655Result? Power655Result { get; private set; }

    // ── Factory ────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new prediction for a target draw date.
    /// </summary>
    /// <param name="targetDrawDate">The draw date this prediction is for.</param>
    /// <param name="predictionType">Pick3, Pick4, Pick5, or Pick6.</param>
    /// <param name="num1">First number (1-55).</param>
    /// <param name="num2">Second number (1-55).</param>
    /// <param name="num3">Third number (1-55).</param>
    /// <param name="num4">Fourth number (1-55).</param>
    /// <param name="num5">Fifth number (1-55).</param>
    /// <param name="num6">Sixth number (1-55).</param>
    /// <param name="tupleFrequency">How many times this N-tuple appeared in historical draws.</param>
    /// <param name="reasoning">Explanation of the prediction logic.</param>
    public static Power655Prediction Create(
        DateOnly targetDrawDate,
        PredictionType predictionType,
        int num1,
        int num2,
        int num3,
        int num4,
        int num5,
        int num6,
        int tupleFrequency,
        string reasoning)
    {
        return new Power655Prediction
        {
            Id = Guid.NewGuid(),
            TargetDrawDate = targetDrawDate,
            PredictionType = predictionType,
            Num1 = num1,
            Num2 = num2,
            Num3 = num3,
            Num4 = num4,
            Num5 = num5,
            Num6 = num6,
            TupleFrequency = tupleFrequency,
            Reasoning = reasoning ?? string.Empty,
            MatchedNumbers = 0
        };
    }

    // ── Behaviour ──────────────────────────────────────────────────────────

    /// <summary>
    /// Links this prediction to an actual draw result and sets the matched count.
    /// </summary>
    public void SetResult(Guid power655ResultId, int matchedNumbers)
    {
        Power655ResultId = power655ResultId;
        MatchedNumbers = matchedNumbers;
    }
}
