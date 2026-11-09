using ThaiX.Client.Constants;

namespace ThaiX.Client.Services.Games;

public sealed record GameDefinition(
    string Id,
    string TitleKey,
    string DescriptionKey,
    string CategoryKey,
    string Icon,
    string InstructionsKey,
    string ScoreKey);

public static class GameCatalog
{
    public static IReadOnlyList<GameDefinition> All { get; } =
    [
        new("2048", ResourceKeys.Games.Name2048, ResourceKeys.Games.Description2048, ResourceKeys.Games.Puzzle, "grid_view", ResourceKeys.Games.Instructions2048, ResourceKeys.Games.Score),
        new("minesweeper", ResourceKeys.Games.NameMinesweeper, ResourceKeys.Games.DescriptionMinesweeper, ResourceKeys.Games.Puzzle, "bomb", ResourceKeys.Games.InstructionsMinesweeper, ResourceKeys.Games.TimeSeconds),
        new("sudoku", ResourceKeys.Games.NameSudoku, ResourceKeys.Games.DescriptionSudoku, ResourceKeys.Games.Puzzle, "apps", ResourceKeys.Games.InstructionsSudoku, ResourceKeys.Games.TimeSeconds),
        new("memory-cards", ResourceKeys.Games.NameMemory, ResourceKeys.Games.DescriptionMemory, ResourceKeys.Games.Puzzle, "style", ResourceKeys.Games.InstructionsMemory, ResourceKeys.Games.Moves),
        new("15-puzzle", ResourceKeys.Games.Name15Puzzle, ResourceKeys.Games.Description15Puzzle, ResourceKeys.Games.Puzzle, "view_module", ResourceKeys.Games.Instructions15Puzzle, ResourceKeys.Games.Moves),
        new("lights-out", ResourceKeys.Games.NameLightsOut, ResourceKeys.Games.DescriptionLightsOut, ResourceKeys.Games.Puzzle, "lightbulb", ResourceKeys.Games.InstructionsLightsOut, ResourceKeys.Games.Moves),
        new("snake", ResourceKeys.Games.NameSnake, ResourceKeys.Games.DescriptionSnake, ResourceKeys.Games.Arcade, "timeline", ResourceKeys.Games.InstructionsSnake, ResourceKeys.Games.Score),
        new("tetris", ResourceKeys.Games.NameTetris, ResourceKeys.Games.DescriptionTetris, ResourceKeys.Games.Arcade, "view_stream", ResourceKeys.Games.InstructionsTetris, ResourceKeys.Games.Score),
        new("flappy", ResourceKeys.Games.NameFlappy, ResourceKeys.Games.DescriptionFlappy, ResourceKeys.Games.Arcade, "flight", ResourceKeys.Games.InstructionsFlappy, ResourceKeys.Games.Score),
        new("breakout", ResourceKeys.Games.NameBreakout, ResourceKeys.Games.DescriptionBreakout, ResourceKeys.Games.Arcade, "sports_tennis", ResourceKeys.Games.InstructionsBreakout, ResourceKeys.Games.Score),
        new("pong", ResourceKeys.Games.NamePong, ResourceKeys.Games.DescriptionPong, ResourceKeys.Games.Arcade, "sports_esports", ResourceKeys.Games.InstructionsPong, ResourceKeys.Games.Wins),
        new("wordle", ResourceKeys.Games.NameWordle, ResourceKeys.Games.DescriptionWordle, ResourceKeys.Games.WordSkill, "spellcheck", ResourceKeys.Games.InstructionsWordle, ResourceKeys.Games.Guesses),
        new("hangman", ResourceKeys.Games.NameHangman, ResourceKeys.Games.DescriptionHangman, ResourceKeys.Games.WordSkill, "psychology", ResourceKeys.Games.InstructionsHangman, ResourceKeys.Games.Score),
        new("typing-speed", ResourceKeys.Games.NameTyping, ResourceKeys.Games.DescriptionTyping, ResourceKeys.Games.WordSkill, "keyboard", ResourceKeys.Games.InstructionsTyping, ResourceKeys.Games.Wpm),
        new("tic-tac-toe", ResourceKeys.Games.NameTicTacToe, ResourceKeys.Games.DescriptionTicTacToe, ResourceKeys.Games.Strategy, "grid_3x3", ResourceKeys.Games.InstructionsTicTacToe, ResourceKeys.Games.Wins),
        new("connect-four", ResourceKeys.Games.NameConnectFour, ResourceKeys.Games.DescriptionConnectFour, ResourceKeys.Games.Strategy, "view_week", ResourceKeys.Games.InstructionsConnectFour, ResourceKeys.Games.Wins)
    ];

    public static GameDefinition? Find(string? id) =>
        All.FirstOrDefault(game => string.Equals(game.Id, id, StringComparison.OrdinalIgnoreCase));
}
