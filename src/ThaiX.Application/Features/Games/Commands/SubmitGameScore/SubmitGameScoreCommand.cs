using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Games.Commands.SubmitGameScore;

public sealed record SubmitGameScoreCommand : IAppSerializableCommand<SubmitGameScoreResultDto>
{
    public required string GameId { get; init; }
    public Guid RunId { get; init; }
    public required string PlayerId { get; init; }
    public Guid? UserId { get; init; }
    public required string PlayerName { get; init; }
    public decimal Score { get; init; }
    public int DurationSeconds { get; init; }
    public string? Mode { get; init; }
}
