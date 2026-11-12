namespace ThaiX.Client.Services.Games;

/// <summary>Pure rules shared by the game boards, kept free of UI state so they can be unit tested.</summary>
public static class GameLogic
{
    private static readonly int[][] TicTacToeLines =
    [
        [0, 1, 2], [3, 4, 5], [6, 7, 8], [0, 3, 6], [1, 4, 7], [2, 5, 8], [0, 4, 8], [2, 4, 6]
    ];

    /// <summary>Slides a 2048 line toward index 0, merging each pair of equal tiles once.</summary>
    public static (int[] Line, int Gained) MergeLine(IReadOnlyList<int> line)
    {
        var values = line.Where(value => value != 0).ToList();
        var merged = new int[line.Count];
        var gained = 0;
        var next = 0;

        for (var i = 0; i < values.Count; i++)
        {
            if (i + 1 < values.Count && values[i] == values[i + 1])
            {
                merged[next++] = values[i] * 2;
                gained += values[i] * 2;
                i++;
            }
            else
            {
                merged[next++] = values[i];
            }
        }

        return (merged, gained);
    }

    /// <summary>
    /// Scores a Wordle guess per letter: 'c' correct position, 'p' present elsewhere, 'a' absent.
    /// Repeated letters are only marked present as often as they remain unmatched in the word.
    /// </summary>
    public static string WordleStatuses(string guess, string word)
    {
        var statuses = new char[guess.Length];
        var remaining = new Dictionary<char, int>();

        for (var i = 0; i < guess.Length; i++)
        {
            if (guess[i] == word[i])
                statuses[i] = 'c';
            else
                remaining[word[i]] = remaining.GetValueOrDefault(word[i]) + 1;
        }

        for (var i = 0; i < guess.Length; i++)
        {
            if (statuses[i] == 'c')
                continue;

            if (remaining.GetValueOrDefault(guess[i]) > 0)
            {
                statuses[i] = 'p';
                remaining[guess[i]]--;
            }
            else
            {
                statuses[i] = 'a';
            }
        }

        return new string(statuses);
    }

    public static int[][] RotateClockwise(int[][] piece)
    {
        var rows = piece.Length;
        var columns = piece[0].Length;
        var rotated = new int[columns][];

        for (var i = 0; i < columns; i++)
        {
            rotated[i] = new int[rows];
            for (var j = 0; j < rows; j++)
                rotated[i][j] = piece[rows - 1 - j][i];
        }

        return rotated;
    }

    public static bool TicTacToeWon(char[] cells, char player) =>
        TicTacToeLines.Any(line => line.All(index => cells[index] == player));

    /// <summary>Returns the optimal move for 'O' on a 9-cell board, or -1 when the board is full.</summary>
    public static int BestTicTacToeMove(char[] cells)
    {
        var best = int.MinValue;
        var move = -1;

        for (var i = 0; i < cells.Length; i++)
        {
            if (cells[i] != '\0')
                continue;

            cells[i] = 'O';
            var score = Minimax(cells, 0, maximizing: false);
            cells[i] = '\0';

            if (score > best)
            {
                best = score;
                move = i;
            }
        }

        return move;
    }

    /// <summary>Checks a Connect Four board indexed [row, column] for four in a row.</summary>
    public static bool ConnectFourWon(int[,] board, int player)
    {
        var rows = board.GetLength(0);
        var columns = board.GetLength(1);
        (int Row, int Column)[] directions = [(0, 1), (1, 0), (1, 1), (1, -1)];

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                foreach (var (dr, dc) in directions)
                {
                    var count = 0;
                    for (var step = 0; step < 4; step++)
                    {
                        var r = row + dr * step;
                        var c = column + dc * step;
                        if (r < 0 || r >= rows || c < 0 || c >= columns || board[r, c] != player)
                            break;
                        count++;
                    }

                    if (count == 4)
                        return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the point reached after walking <paramref name="distance"/> along a polyline,
    /// clamped to its first and last point.
    /// </summary>
    public static (double X, double Y) PointOnPath(IReadOnlyList<(double X, double Y)> path, double distance)
    {
        for (var i = 1; i < path.Count; i++)
        {
            var (fromX, fromY) = path[i - 1];
            var (toX, toY) = path[i];
            var length = double.Hypot(toX - fromX, toY - fromY);
            if (distance > length && i < path.Count - 1)
            {
                distance -= length;
                continue;
            }

            var ratio = length == 0 ? 0 : Math.Clamp(distance / length, 0, 1);
            return (fromX + (toX - fromX) * ratio, fromY + (toY - fromY) * ratio);
        }

        return path[0];
    }

    private static int Minimax(char[] cells, int depth, bool maximizing)
    {
        if (TicTacToeWon(cells, 'O'))
            return 10 - depth;
        if (TicTacToeWon(cells, 'X'))
            return depth - 10;
        if (cells.All(cell => cell != '\0'))
            return 0;

        var best = maximizing ? int.MinValue : int.MaxValue;
        for (var i = 0; i < cells.Length; i++)
        {
            if (cells[i] != '\0')
                continue;

            cells[i] = maximizing ? 'O' : 'X';
            var score = Minimax(cells, depth + 1, !maximizing);
            cells[i] = '\0';
            best = maximizing ? Math.Max(best, score) : Math.Min(best, score);
        }

        return best;
    }
}
