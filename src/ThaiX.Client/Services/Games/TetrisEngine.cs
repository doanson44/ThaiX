namespace ThaiX.Client.Services.Games;

public enum TetrisPieceKind
{
    I = 1,
    O,
    T,
    J,
    L,
    S,
    Z
}

public sealed class TetrisEngine
{
    public const int Columns = 10;
    public const int Rows = 20;

    private static readonly IReadOnlyDictionary<TetrisPieceKind, int[][]> SpawnShapes =
        new Dictionary<TetrisPieceKind, int[][]>
        {
            [TetrisPieceKind.I] = [[0, 0, 0, 0], [1, 1, 1, 1], [0, 0, 0, 0], [0, 0, 0, 0]],
            [TetrisPieceKind.O] = [[1, 1], [1, 1]],
            [TetrisPieceKind.T] = [[0, 1, 0], [1, 1, 1], [0, 0, 0]],
            [TetrisPieceKind.J] = [[1, 0, 0], [1, 1, 1], [0, 0, 0]],
            [TetrisPieceKind.L] = [[0, 0, 1], [1, 1, 1], [0, 0, 0]],
            [TetrisPieceKind.S] = [[0, 1, 1], [1, 1, 0], [0, 0, 0]],
            [TetrisPieceKind.Z] = [[1, 1, 0], [0, 1, 1], [0, 0, 0]]
        };

    private static readonly (int X, int Y)[][] JlstzKicks =
    [
        [(0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2)],
        [(0, 0), (1, 0), (1, -1), (0, 2), (1, 2)],
        [(0, 0), (1, 0), (1, -1), (0, 2), (1, 2)],
        [(0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2)],
        [(0, 0), (1, 0), (1, 1), (0, -2), (1, -2)],
        [(0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2)],
        [(0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2)],
        [(0, 0), (1, 0), (1, 1), (0, -2), (1, -2)]
    ];

    private static readonly (int X, int Y)[][] IKicks =
    [
        [(0, 0), (-2, 0), (1, 0), (-2, -1), (1, 2)],
        [(0, 0), (2, 0), (-1, 0), (2, 1), (-1, -2)],
        [(0, 0), (-1, 0), (2, 0), (-1, 2), (2, -1)],
        [(0, 0), (1, 0), (-2, 0), (1, -2), (-2, 1)],
        [(0, 0), (2, 0), (-1, 0), (2, 1), (-1, -2)],
        [(0, 0), (-2, 0), (1, 0), (-2, -1), (1, 2)],
        [(0, 0), (1, 0), (-2, 0), (1, -2), (-2, 1)],
        [(0, 0), (-1, 0), (2, 0), (-1, 2), (2, -1)]
    ];

    private readonly Random _random;
    private readonly List<TetrisPieceKind> _bag = [];
    private double _gravityElapsed;
    private int[,] _board = new int[Rows, Columns];
    private int[][] _shape = SpawnShapes[TetrisPieceKind.I];

    public TetrisEngine(Random? random = null)
    {
        _random = random ?? Random.Shared;
        Reset();
    }

    public int[,] Board => _board;
    public TetrisPieceKind CurrentKind { get; private set; }
    public int[][] CurrentShape => _shape;
    public int PieceX { get; private set; }
    public int PieceY { get; private set; }
    public int Rotation { get; private set; }
    public int Score { get; private set; }
    public int Lines { get; private set; }
    public int Level => Lines / 10 + 1;
    public TetrisPieceKind? HeldPiece { get; private set; }
    public TetrisPieceKind NextPiece => PeekNext();
    public bool CanHold { get; private set; } = true;
    public bool IsFinished { get; private set; }

    public int GhostY
    {
        get
        {
            var y = PieceY;
            while (Fits(_shape, PieceX, y + 1))
                y++;
            return y;
        }
    }

    public void Reset()
    {
        _board = new int[Rows, Columns];
        _bag.Clear();
        _gravityElapsed = 0;
        Score = 0;
        Lines = 0;
        HeldPiece = null;
        CanHold = true;
        IsFinished = false;
        SpawnNext();
    }

    public void Advance(TimeSpan elapsed)
    {
        if (IsFinished || elapsed <= TimeSpan.Zero)
            return;

        _gravityElapsed += Math.Min(elapsed.TotalSeconds, 1);
        var interval = Math.Max(0.08, 0.8 * Math.Pow(0.8, Level - 1));
        while (_gravityElapsed >= interval && !IsFinished)
        {
            _gravityElapsed -= interval;
            if (!Move(0, 1))
                LockPiece();
        }
    }

    public bool Move(int dx, int dy)
    {
        if (IsFinished || !Fits(_shape, PieceX + dx, PieceY + dy))
            return false;

        PieceX += dx;
        PieceY += dy;
        if (dy > 0)
            _gravityElapsed = 0;
        return true;
    }

    public void SoftDrop()
    {
        if (Move(0, 1))
            Score++;
        else
            LockPiece();
    }

    public void HardDrop()
    {
        if (IsFinished)
            return;

        var distance = GhostY - PieceY;
        PieceY += distance;
        Score += distance * 2;
        LockPiece();
    }

    public void RotateClockwise()
    {
        if (IsFinished || CurrentKind == TetrisPieceKind.O)
            return;

        var rotated = GameLogic.RotateClockwise(_shape);
        var nextRotation = (Rotation + 1) % 4;
        var kickSet = CurrentKind == TetrisPieceKind.I ? IKicks : JlstzKicks;
        var kickIndex = Rotation * 2;

        foreach (var (x, y) in kickSet[kickIndex])
        {
            if (!Fits(rotated, PieceX + x, PieceY - y))
                continue;

            _shape = rotated;
            PieceX += x;
            PieceY -= y;
            Rotation = nextRotation;
            return;
        }
    }

    public void Hold()
    {
        if (IsFinished || !CanHold)
            return;

        var current = CurrentKind;
        if (HeldPiece is { } held)
        {
            HeldPiece = current;
            Spawn(held);
        }
        else
        {
            HeldPiece = current;
            SpawnNext();
        }

        CanHold = false;
    }

    private void LockPiece()
    {
        for (var y = 0; y < _shape.Length; y++)
        {
            for (var x = 0; x < _shape[y].Length; x++)
            {
                if (_shape[y][x] == 0)
                    continue;

                var row = PieceY + y;
                var column = PieceX + x;
                if (row < 0)
                {
                    IsFinished = true;
                    return;
                }

                _board[row, column] = (int)CurrentKind;
            }
        }

        ClearLines();
        CanHold = true;
        SpawnNext();
    }

    private void ClearLines()
    {
        var target = Rows - 1;
        var cleared = 0;

        for (var row = Rows - 1; row >= 0; row--)
        {
            var full = true;
            for (var column = 0; column < Columns; column++)
                full &= _board[row, column] != 0;

            if (full)
            {
                cleared++;
                continue;
            }

            if (target != row)
                for (var column = 0; column < Columns; column++)
                    _board[target, column] = _board[row, column];
            target--;
        }

        for (var row = target; row >= 0; row--)
            for (var column = 0; column < Columns; column++)
                _board[row, column] = 0;

        if (cleared == 0)
            return;

        var scoringLevel = Level;
        Lines += cleared;
        Score += (cleared switch { 1 => 100, 2 => 300, 3 => 500, 4 => 800, _ => 0 }) * scoringLevel;
    }

    private bool Fits(int[][] shape, int x, int y)
    {
        for (var row = 0; row < shape.Length; row++)
        {
            for (var column = 0; column < shape[row].Length; column++)
            {
                if (shape[row][column] == 0)
                    continue;

                var boardX = x + column;
                var boardY = y + row;
                if (boardX < 0 || boardX >= Columns || boardY >= Rows)
                    return false;
                if (boardY >= 0 && _board[boardY, boardX] != 0)
                    return false;
            }
        }

        return true;
    }

    private void SpawnNext() => Spawn(NextFromBag());

    private void Spawn(TetrisPieceKind kind)
    {
        CurrentKind = kind;
        _shape = SpawnShapes[kind];
        PieceX = 3;
        PieceY = -1;
        Rotation = 0;
        _gravityElapsed = 0;
        if (!Fits(_shape, PieceX, PieceY))
            IsFinished = true;
    }

    private TetrisPieceKind PeekNext()
    {
        EnsureBag();
        return _bag[^1];
    }

    private TetrisPieceKind NextFromBag()
    {
        EnsureBag();
        var index = _bag.Count - 1;
        var piece = _bag[index];
        _bag.RemoveAt(index);
        return piece;
    }

    private void EnsureBag()
    {
        if (_bag.Count > 0)
            return;

        _bag.AddRange(Enum.GetValues<TetrisPieceKind>());
        for (var i = _bag.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (_bag[i], _bag[j]) = (_bag[j], _bag[i]);
        }
    }
}
