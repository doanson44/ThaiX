namespace ThaiX.Client.Services.Games;

public enum TypingDifficulty
{
    Easy,
    Medium,
    Hard
}

public static class TypingPassageCatalog
{
    private static readonly IReadOnlyDictionary<TypingDifficulty, string[]> PassageKeys =
        new Dictionary<TypingDifficulty, string[]>
        {
            [TypingDifficulty.Easy] =
            [
                "Games.TypingEasyPassage1",
                "Games.TypingEasyPassage2",
                "Games.TypingEasyPassage3"
            ],
            [TypingDifficulty.Medium] =
            [
                "Games.TypingMediumPassage1",
                "Games.TypingMediumPassage2",
                "Games.TypingMediumPassage3"
            ],
            [TypingDifficulty.Hard] =
            [
                "Games.TypingHardPassage1",
                "Games.TypingHardPassage2",
                "Games.TypingHardPassage3"
            ]
        };

    public static IReadOnlyList<string> GetPassageKeys(TypingDifficulty difficulty) =>
        PassageKeys[difficulty];

    public static string SelectPassage(
        TypingDifficulty difficulty,
        Func<string, string> resolve,
        IReadOnlyCollection<string> recentPassages,
        Random? random = null)
    {
        var passages = GetPassageKeys(difficulty).Select(resolve).ToArray();
        var available = passages
            .Where(passage => !recentPassages.Contains(passage, StringComparer.Ordinal))
            .ToArray();
        var candidates = available.Length > 0 ? available : passages;

        return candidates[(random ?? Random.Shared).Next(candidates.Length)];
    }
}
