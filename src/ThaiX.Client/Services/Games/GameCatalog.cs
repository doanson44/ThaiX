using ThaiX.Client.Constants;
using ThaiX.Client.Pages.Games;

namespace ThaiX.Client.Services.Games;

public sealed record GameDefinition(
    string Id,
    string TitleKey,
    string DescriptionKey,
    string CategoryKey,
    string Icon,
    string InstructionsKey,
    string ScoreKey,
    Type Component);

public static class GameCatalog
{
    public static IReadOnlyList<GameDefinition> All { get; } =
    [
        new("2048", ResourceKeys.Games.Name2048, ResourceKeys.Games.Description2048, ResourceKeys.Games.Puzzle, "grid_view", ResourceKeys.Games.Instructions2048, ResourceKeys.Games.Score, typeof(Game2048)),
        new("minesweeper", ResourceKeys.Games.NameMinesweeper, ResourceKeys.Games.DescriptionMinesweeper, ResourceKeys.Games.Puzzle, "bomb", ResourceKeys.Games.InstructionsMinesweeper, ResourceKeys.Games.TimeSeconds, typeof(MinesweeperGame)),
        new("sudoku", ResourceKeys.Games.NameSudoku, ResourceKeys.Games.DescriptionSudoku, ResourceKeys.Games.Puzzle, "apps", ResourceKeys.Games.InstructionsSudoku, ResourceKeys.Games.TimeSeconds, typeof(SudokuGame)),
        new("memory-cards", ResourceKeys.Games.NameMemory, ResourceKeys.Games.DescriptionMemory, ResourceKeys.Games.Puzzle, "style", ResourceKeys.Games.InstructionsMemory, ResourceKeys.Games.Moves, typeof(MemoryGame)),
        new("15-puzzle", ResourceKeys.Games.Name15Puzzle, ResourceKeys.Games.Description15Puzzle, ResourceKeys.Games.Puzzle, "view_module", ResourceKeys.Games.Instructions15Puzzle, ResourceKeys.Games.Moves, typeof(FifteenPuzzleGame)),
        new("lights-out", ResourceKeys.Games.NameLightsOut, ResourceKeys.Games.DescriptionLightsOut, ResourceKeys.Games.Puzzle, "lightbulb", ResourceKeys.Games.InstructionsLightsOut, ResourceKeys.Games.Moves, typeof(LightsOutGame)),
        new("snake", ResourceKeys.Games.NameSnake, ResourceKeys.Games.DescriptionSnake, ResourceKeys.Games.Arcade, "timeline", ResourceKeys.Games.InstructionsSnake, ResourceKeys.Games.Score, typeof(SnakeGame)),
        new("tetris", ResourceKeys.Games.NameTetris, ResourceKeys.Games.DescriptionTetris, ResourceKeys.Games.Arcade, "view_stream", ResourceKeys.Games.InstructionsTetris, ResourceKeys.Games.Score, typeof(TetrisGame)),
        new("flappy", ResourceKeys.Games.NameFlappy, ResourceKeys.Games.DescriptionFlappy, ResourceKeys.Games.Arcade, "flight", ResourceKeys.Games.InstructionsFlappy, ResourceKeys.Games.Score, typeof(FlappyGame)),
        new("breakout", ResourceKeys.Games.NameBreakout, ResourceKeys.Games.DescriptionBreakout, ResourceKeys.Games.Arcade, "sports_tennis", ResourceKeys.Games.InstructionsBreakout, ResourceKeys.Games.Score, typeof(BreakoutGame)),
        new("pong", ResourceKeys.Games.NamePong, ResourceKeys.Games.DescriptionPong, ResourceKeys.Games.Arcade, "sports_esports", ResourceKeys.Games.InstructionsPong, ResourceKeys.Games.Wins, typeof(PongGame)),
        new("wordle", ResourceKeys.Games.NameWordle, ResourceKeys.Games.DescriptionWordle, ResourceKeys.Games.WordSkill, "spellcheck", ResourceKeys.Games.InstructionsWordle, ResourceKeys.Games.Guesses, typeof(WordleGame)),
        new("hangman", ResourceKeys.Games.NameHangman, ResourceKeys.Games.DescriptionHangman, ResourceKeys.Games.WordSkill, "psychology", ResourceKeys.Games.InstructionsHangman, ResourceKeys.Games.Score, typeof(HangmanGame)),
        new("typing-speed", ResourceKeys.Games.NameTyping, ResourceKeys.Games.DescriptionTyping, ResourceKeys.Games.WordSkill, "keyboard", ResourceKeys.Games.InstructionsTyping, ResourceKeys.Games.Wpm, typeof(TypingGame)),
        new("tic-tac-toe", ResourceKeys.Games.NameTicTacToe, ResourceKeys.Games.DescriptionTicTacToe, ResourceKeys.Games.Strategy, "grid_3x3", ResourceKeys.Games.InstructionsTicTacToe, ResourceKeys.Games.Wins, typeof(TicTacToeGame)),
        new("connect-four", ResourceKeys.Games.NameConnectFour, ResourceKeys.Games.DescriptionConnectFour, ResourceKeys.Games.Strategy, "view_week", ResourceKeys.Games.InstructionsConnectFour, ResourceKeys.Games.Wins, typeof(ConnectFourGame)),
        new("tower-defense", ResourceKeys.Games.NameTowerDefense, ResourceKeys.Games.DescriptionTowerDefense, ResourceKeys.Games.Strategy, "castle", ResourceKeys.Games.InstructionsTowerDefense, ResourceKeys.Games.Score, typeof(TowerDefenseGame))
    ];

    public static GameDefinition? Find(string? id) =>
        All.FirstOrDefault(game => string.Equals(game.Id, id, StringComparison.OrdinalIgnoreCase));
}
