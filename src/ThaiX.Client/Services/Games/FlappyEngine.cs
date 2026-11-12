namespace ThaiX.Client.Services.Games;

public readonly record struct FlappyPipe(double X, double GapCenter, bool Passed = false);

public sealed class FlappyEngine
{
    public const int BoardWidth = 720;
    public const int BoardHeight = 480;
    public const double BirdX = 105;
    public const double BirdRadius = 15;
    public const double PipeWidth = 55;
    public const double PipeCapWidth = 4;
    public const double GapHeight = 170;
    public const double PlayTop = 45;
    public const double FloorY = 468;

    private const double FixedStepSeconds = 1.0 / 60.0;
    private const double MaximumElapsedSeconds = FixedStepSeconds * 5;
    private const int MaximumStepsPerAdvance = 5;
    private const double Gravity = 466.6666666666667;
    private const double FlapVelocity = -216.66666666666666;
    private const double PipeSpeed = 100;
    private const double PipeSpawnInterval = 2.85;
    private const double FirstPipeX = 560;
    private const double SpawnPipeX = 740;
    private const double MinimumGapCenter = 180;
    private const double GapCenterRange = 100;

    private readonly Random _random;
    private readonly List<FlappyPipe> _pipes = [];
    private double _accumulator;
    private double _pipeSpawnElapsed;

    public double BirdY { get; private set; }
    public double BirdVelocity { get; private set; }
    public int Score { get; private set; }
    public bool HasStarted { get; private set; }
    public bool IsFinished { get; private set; }
    public IReadOnlyList<FlappyPipe> Pipes => _pipes;

    public FlappyEngine(Random? random = null)
    {
        _random = random ?? Random.Shared;
        Reset();
    }

    public void Reset()
    {
        _accumulator = 0;
        _pipeSpawnElapsed = 0;
        BirdY = 230;
        BirdVelocity = 0;
        Score = 0;
        HasStarted = false;
        IsFinished = false;
        _pipes.Clear();
        _pipes.Add(new FlappyPipe(FirstPipeX, BoardHeight / 2.0 - 5));
    }

    public void Flap()
    {
        if (IsFinished)
            return;

        HasStarted = true;
        BirdVelocity = FlapVelocity;
    }

    public void Advance(double elapsedSeconds)
    {
        if (!HasStarted || IsFinished || !double.IsFinite(elapsedSeconds) || elapsedSeconds <= 0)
            return;

        _accumulator += Math.Min(elapsedSeconds, MaximumElapsedSeconds);
        var steps = 0;

        while (_accumulator >= FixedStepSeconds && steps < MaximumStepsPerAdvance && !IsFinished)
        {
            Step(FixedStepSeconds);
            _accumulator -= FixedStepSeconds;
            steps++;
        }

        if (steps == MaximumStepsPerAdvance && _accumulator >= FixedStepSeconds)
            _accumulator = 0;
    }

    private void Step(double deltaTime)
    {
        BirdVelocity += Gravity * deltaTime;
        BirdY += BirdVelocity * deltaTime;

        _pipeSpawnElapsed += deltaTime;
        if (_pipeSpawnElapsed >= PipeSpawnInterval)
        {
            _pipeSpawnElapsed -= PipeSpawnInterval;
            var gapCenter = MinimumGapCenter + _random.NextDouble() * GapCenterRange;
            _pipes.Add(new FlappyPipe(SpawnPipeX, gapCenter));
        }

        for (var i = 0; i < _pipes.Count; i++)
        {
            var pipe = _pipes[i] with { X = _pipes[i].X - PipeSpeed * deltaTime };

            if (!pipe.Passed && pipe.X + PipeWidth + PipeCapWidth < BirdX)
            {
                pipe = pipe with { Passed = true };
                Score++;
            }

            _pipes[i] = pipe;

            var gapTop = pipe.GapCenter - GapHeight / 2;
            var gapBottom = pipe.GapCenter + GapHeight / 2;
            var pipeLeft = pipe.X - PipeCapWidth;
            var pipeRight = pipe.X + PipeWidth + PipeCapWidth;

            if (IntersectsPipe(pipeLeft, pipeRight, PlayTop, gapTop)
                || IntersectsPipe(pipeLeft, pipeRight, gapBottom, FloorY))
            {
                IsFinished = true;
                return;
            }
        }

        for (var i = _pipes.Count - 1; i >= 0; i--)
        {
            if (_pipes[i].X < -PipeWidth - PipeCapWidth * 2)
                _pipes.RemoveAt(i);
        }

        if (BirdY - BirdRadius <= PlayTop || BirdY + BirdRadius >= FloorY)
        {
            BirdY = Math.Clamp(BirdY, PlayTop + BirdRadius, FloorY - BirdRadius);
            IsFinished = true;
        }
    }

    private bool IntersectsPipe(double left, double right, double top, double bottom)
    {
        var closestX = Math.Clamp(BirdX, left, right);
        var closestY = Math.Clamp(BirdY, top, bottom);
        var dx = BirdX - closestX;
        var dy = BirdY - closestY;

        return dx * dx + dy * dy <= BirdRadius * BirdRadius;
    }
}
