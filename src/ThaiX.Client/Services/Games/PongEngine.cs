namespace ThaiX.Client.Services.Games;

public sealed class PongEngine
{
    public const int BoardWidth = 720;
    public const int BoardHeight = 480;
    public const int PaddleHeight = 100;
    public const int PaddleWidth = 12;
    public const int BallRadius = 9;
    public const int WinningScore = 5;

    private const double FixedStepSeconds = 1.0 / 60.0;
    private const double MaximumElapsedSeconds = FixedStepSeconds * 5;
    private const int MaximumStepsPerAdvance = 5;
    private const double InitialBallSpeed = 320;
    private const double MaximumBallSpeed = 520;
    private const double PlayerPaddleSpeed = 480;
    private const double AiPaddleSpeed = 260;
    private const double MaximumBounceAngle = Math.PI / 3;
    private const double SpeedIncreaseFactor = 1.06;
    private const double AiReactionDelaySeconds = 0.1;
    private const double AiDeadZone = 12;
    private const double PlayerPaddleX = 38;
    private const double AiPaddleX = BoardWidth - 50;
    private const double PlayerCollisionX = PlayerPaddleX + PaddleWidth + BallRadius;
    private const double AiCollisionX = AiPaddleX - BallRadius;

    private readonly Random _random;
    private readonly int _winningScore;
    private double _accumulator;
    private double _aiReactionElapsed;
    private double _aiTargetY;
    private double _ballSpeed;

    public double PlayerPaddleY { get; private set; }
    public double AiPaddleY { get; private set; }
    public double BallX { get; private set; }
    public double BallY { get; private set; }
    public double BallVelocityX { get; private set; }
    public double BallVelocityY { get; private set; }
    public double BallSpeed => _ballSpeed;
    public int RallyHits { get; private set; }
    public int PlayerScore { get; private set; }
    public int AiScore { get; private set; }
    public bool IsFinished { get; private set; }

    public PongEngine(Random? random = null, int winningScore = WinningScore)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(winningScore);
        _random = random ?? Random.Shared;
        _winningScore = winningScore;
        Reset();
    }

    public void Reset()
    {
        _accumulator = 0;
        _aiReactionElapsed = 0;
        _aiTargetY = BoardHeight / 2.0;
        PlayerPaddleY = AiPaddleY = _aiTargetY;
        PlayerScore = AiScore = 0;
        RallyHits = 0;
        IsFinished = false;
        Serve();
    }

    public void Advance(double elapsedSeconds, int playerDirection)
    {
        if (IsFinished || !double.IsFinite(elapsedSeconds) || elapsedSeconds <= 0)
            return;

        _accumulator += Math.Min(elapsedSeconds, MaximumElapsedSeconds);
        var steps = 0;

        while (_accumulator >= FixedStepSeconds && steps < MaximumStepsPerAdvance && !IsFinished)
        {
            Step(FixedStepSeconds, Math.Clamp(playerDirection, -1, 1));
            _accumulator -= FixedStepSeconds;
            steps++;
        }

        if (steps == MaximumStepsPerAdvance && _accumulator >= FixedStepSeconds)
            _accumulator = 0;
    }

    private void Step(double deltaTime, int playerDirection)
    {
        PlayerPaddleY = MovePaddle(PlayerPaddleY, playerDirection * PlayerPaddleSpeed * deltaTime);
        UpdateAi(deltaTime);

        var previousX = BallX;
        var previousY = BallY;
        var nextX = previousX + BallVelocityX * deltaTime;
        var nextY = previousY + BallVelocityY * deltaTime;

        if (BallVelocityX < 0 && previousX > PlayerCollisionX && nextX <= PlayerCollisionX)
        {
            var collisionFraction = (previousX - PlayerCollisionX) / (previousX - nextX);
            var collisionY = ReflectY(previousY + BallVelocityY * deltaTime * collisionFraction);

            if (Hits(PlayerPaddleY, collisionY))
            {
                BallX = PlayerCollisionX;
                BallY = collisionY;
                BounceFromPaddle(PlayerPaddleY, 1);
                AdvanceBall(deltaTime * (1 - collisionFraction));
                return;
            }
        }
        else if (BallVelocityX > 0 && previousX < AiCollisionX && nextX >= AiCollisionX)
        {
            var collisionFraction = (AiCollisionX - previousX) / (nextX - previousX);
            var collisionY = ReflectY(previousY + BallVelocityY * deltaTime * collisionFraction);

            if (Hits(AiPaddleY, collisionY))
            {
                BallX = AiCollisionX;
                BallY = collisionY;
                BounceFromPaddle(AiPaddleY, -1);
                AdvanceBall(deltaTime * (1 - collisionFraction));
                return;
            }
        }

        BallX = nextX;
        BallY = ReflectY(nextY);

        if (BallX < 0)
        {
            AiScore++;
            CompletePoint();
        }
        else if (BallX > BoardWidth)
        {
            PlayerScore++;
            CompletePoint();
        }
    }

    private void UpdateAi(double deltaTime)
    {
        _aiReactionElapsed += deltaTime;
        if (_aiReactionElapsed >= AiReactionDelaySeconds)
        {
            _aiReactionElapsed -= AiReactionDelaySeconds;
            _aiTargetY = BallVelocityX > 0
                ? PredictBallYAt(AiCollisionX)
                : BoardHeight / 2.0;
        }

        var targetPaddleY = _aiTargetY - PaddleHeight / 2.0;
        var difference = targetPaddleY - AiPaddleY;
        if (Math.Abs(difference) <= AiDeadZone)
            return;

        var movement = Math.Clamp(difference, -AiPaddleSpeed * deltaTime, AiPaddleSpeed * deltaTime);
        AiPaddleY = MovePaddle(AiPaddleY, movement);
    }

    private double PredictBallYAt(double targetX)
    {
        if (BallVelocityX <= 0)
            return (BoardHeight - PaddleHeight) / 2.0 + PaddleHeight / 2.0;

        var timeToTarget = Math.Max(0, (targetX - BallX) / BallVelocityX);
        return ReflectY(BallY + BallVelocityY * timeToTarget);
    }

    private void BounceFromPaddle(double paddleY, int horizontalDirection)
    {
        var paddleCenter = paddleY + PaddleHeight / 2.0;
        var offset = Math.Clamp((BallY - paddleCenter) / (PaddleHeight / 2.0), -1, 1);
        var angle = offset * MaximumBounceAngle;

        RallyHits++;
        _ballSpeed = Math.Min(_ballSpeed * SpeedIncreaseFactor, MaximumBallSpeed);
        BallVelocityX = horizontalDirection * _ballSpeed * Math.Cos(angle);
        BallVelocityY = _ballSpeed * Math.Sin(angle);
    }

    private void AdvanceBall(double deltaTime)
    {
        BallX += BallVelocityX * deltaTime;
        BallY = ReflectY(BallY + BallVelocityY * deltaTime);
    }

    private void CompletePoint()
    {
        if (PlayerScore >= _winningScore || AiScore >= _winningScore)
        {
            IsFinished = true;
            return;
        }

        _aiTargetY = BoardHeight / 2.0;
        _aiReactionElapsed = 0;
        RallyHits = 0;
        Serve();
    }

    private void Serve()
    {
        BallX = BoardWidth / 2.0;
        BallY = BoardHeight / 2.0;
        _ballSpeed = InitialBallSpeed;

        var angle = (_random.NextDouble() * 2 - 1) * (Math.PI / 9);
        var horizontalDirection = PlayerScore > AiScore ? -1 : 1;
        BallVelocityX = horizontalDirection * _ballSpeed * Math.Cos(angle);
        BallVelocityY = _ballSpeed * Math.Sin(angle);
    }

    private static double MovePaddle(double currentY, double movement) =>
        Math.Clamp(currentY + movement, 0, BoardHeight - PaddleHeight);

    private static bool Hits(double paddleY, double ballY) =>
        ballY >= paddleY - BallRadius && ballY <= paddleY + PaddleHeight + BallRadius;

    private static double ReflectY(double y)
    {
        var minimum = (double)BallRadius;
        var maximum = BoardHeight - BallRadius;
        var range = maximum - minimum;
        var offset = (y - minimum) % (range * 2);

        if (offset < 0)
            offset += range * 2;

        return offset <= range
            ? minimum + offset
            : maximum - (offset - range);
    }
}
