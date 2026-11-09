namespace ThaiX.Application.Features.Games;

public static class GameScoreRules
{
    private static readonly HashSet<string> SupportedGames = new(StringComparer.OrdinalIgnoreCase)
    {
        "2048", "minesweeper", "sudoku", "memory-cards", "15-puzzle", "lights-out",
        "snake", "tetris", "flappy", "breakout", "pong", "wordle", "hangman",
        "typing-speed", "tic-tac-toe", "connect-four"
    };

    private static readonly HashSet<string> LowerIsBetterGames = new(StringComparer.OrdinalIgnoreCase)
    {
        "minesweeper", "sudoku", "memory-cards", "15-puzzle", "lights-out", "wordle"
    };

    public static bool IsSupported(string gameId) =>
        !string.IsNullOrWhiteSpace(gameId) && SupportedGames.Contains(gameId);

    public static bool IsLowerBetter(string gameId) => LowerIsBetterGames.Contains(gameId);
    
    public static bool IsValidScore(string gameId, decimal score)
    {
        if (string.IsNullOrWhiteSpace(gameId) || score != decimal.Truncate(score))
            return false;

        return gameId.ToLowerInvariant() switch
        {
            "2048" or "tetris" => score is >= 0 and <= 1_000_000,
            "minesweeper" or "sudoku" => score is >= 0 and <= 86_400,
            "memory-cards" => score is >= 6 and <= 10_000,
            "15-puzzle" or "lights-out" => score is >= 0 and <= 100_000,
            "snake" => score is >= 0 and <= 3_000 && score % 10 == 0,
            "flappy" or "breakout" => score is >= 0 and <= 100_000,
            "pong" or "tic-tac-toe" or "connect-four" => score is 0 or 1,
            "wordle" => score is >= 1 and <= 7,
            "hangman" => score is >= 0 and <= 6,
            "typing-speed" => score is >= 0 and <= 1_000,
            _ => false
        };
    }

    public static bool IsCumulativeWinGame(string gameId) =>
        string.Equals(gameId, "pong", StringComparison.OrdinalIgnoreCase)
        || string.Equals(gameId, "tic-tac-toe", StringComparison.OrdinalIgnoreCase)
        || string.Equals(gameId, "connect-four", StringComparison.OrdinalIgnoreCase);

    public static bool IsBetter(string gameId, decimal candidate, decimal current) =>
        IsLowerBetter(gameId) ? candidate < current : candidate > current;

    public static string BuildBinCode(string gameId) =>
        $"GAMES_LEADERBOARD_{gameId.Replace('-', '_').ToUpperInvariant()}";
}
