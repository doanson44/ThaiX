using MediatR;

namespace ThaiX.Application.Features.Games.Queries.GetGameLeaderboard;

public sealed record GetGameLeaderboardQuery(string GameId, string? Mode)
    : IRequest<GameLeaderboardDto>;
