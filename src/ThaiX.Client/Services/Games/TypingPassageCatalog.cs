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
}
