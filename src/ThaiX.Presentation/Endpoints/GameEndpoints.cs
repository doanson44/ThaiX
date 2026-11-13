using System.Security.Claims;
using MediatR;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Games;
using ThaiX.Application.Features.Games.Commands.SubmitGameScore;
using ThaiX.Application.Features.Games.Commands.SaveTowerDefenseGold;
using ThaiX.Application.Features.Games.Queries.GetTowerDefenseGold;
using ThaiX.Application.Features.Games.Queries.GetGameLeaderboard;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/games").WithTags("Games");

        group.MapGet("/tower-defense/gold", async (
            ClaimsPrincipal user,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (!TryGetUserId(user, out var userId))
                return Results.Unauthorized();

            var gold = await mediator.Send(new GetTowerDefenseGoldQuery(userId), cancellationToken);
            var response = ApiResponse<int?>.SuccessResult(gold);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization();

        group.MapPut("/tower-defense/gold", async (
            SaveTowerDefenseGoldRequest request,
            ClaimsPrincipal user,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (!TryGetUserId(user, out var userId))
                return Results.Unauthorized();

            await mediator.Send(new SaveTowerDefenseGoldCommand(userId, request.Gold), cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization();

        group.MapGet("/leaderboards/{gameId}", async (
            string gameId,
            string? mode,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            httpContext.Response.Headers["Cache-Control"] = "no-store";
            var result = await mediator.Send(
                new GetGameLeaderboardQuery(gameId, mode),
                cancellationToken);
            var response = ApiResponse<GameLeaderboardDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .AllowAnonymous()
        .RequireRateLimiting("GamesLeaderboard");

        group.MapPost("/guest-session", (
            IGuestIdentityTokenService tokenService,
            HttpContext httpContext) =>
        {
            httpContext.Response.Headers["Cache-Control"] = "no-store";
            var session = tokenService.Create();
            var response = ApiResponse<GuestIdentityToken>.SuccessResult(session);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .AllowAnonymous()
        .RequireRateLimiting("GamesLeaderboard");

        group.MapPost("/scores", async (
            SubmitGameScoreRequest request,
            ClaimsPrincipal user,
            IGuestIdentityTokenService guestTokenService,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            httpContext.Response.Headers["Cache-Control"] = "no-store";
            var authenticated = user.Identity?.IsAuthenticated == true;
            string playerId;
            string playerName;
            Guid? accountUserId = null;

            if (authenticated)
            {
                var subject = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue("sub");
                if (!Guid.TryParse(subject, out var userId))
                    return Results.Unauthorized();

                accountUserId = userId;
                playerId = $"user:{userId:N}";
                var accountEmail = user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email");
                var accountName = accountEmail?.Split('@', 2)[0].Trim() ?? string.Empty;
                var safeAccountName = new string(accountName
                    .Where(character => char.IsLetterOrDigit(character) || " ._+-".Contains(character))
                    .Take(64)
                    .ToArray());
                playerName = safeAccountName.Length >= 2 ? safeAccountName : "Player";
            }
            else
            {
                if (!Guid.TryParse(request.GuestPlayerId, out var guestId)
                    || !guestTokenService.IsValid(guestId, request.GuestToken))
                {
                    var error = ApiResponse.ErrorResult(
                        ErrorCodes.GAMES_GUEST_SESSION_INVALID, "A valid guest session is required.");
                    error.Metadata.CorrelationId = httpContext.GetCorrelationId();
                    return Results.Json(error, statusCode: StatusCodes.Status401Unauthorized);
                }

                playerId = $"guest:{guestId:N}";
                playerName = request.GuestPlayerName ?? string.Empty;
            }

            var result = await mediator.Send(new SubmitGameScoreCommand
            {
                GameId = request.GameId,
                RunId = request.RunId,
                PlayerId = playerId,
                UserId = accountUserId,
                PlayerName = playerName,
                Score = request.Score,
                DurationSeconds = request.DurationSeconds,
                Mode = request.Mode
            }, cancellationToken);

            var response = ApiResponse<SubmitGameScoreResultDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .AllowAnonymous()
        .RequireRateLimiting("GamesLeaderboard");
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
    {
        var subject = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");
        return Guid.TryParse(subject, out userId);
    }
}

public sealed record SaveTowerDefenseGoldRequest(int Gold);

public sealed record SubmitGameScoreRequest(
    string GameId,
    Guid RunId,
    decimal Score,
    int DurationSeconds,
    string? Mode,
    string? GuestPlayerId,
    string? GuestToken,
    string? GuestPlayerName);
