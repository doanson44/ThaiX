using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using ThaiX.Application.Features.AssetPositions.Commands.AddCryptoTransaction;
using ThaiX.Application.Features.AssetPositions.Commands.CreateCryptoPosition;
using ThaiX.Application.Features.AssetPositions.Commands.DeleteCryptoPosition;
using ThaiX.Application.Features.AssetPositions.Commands.UpdateCryptoPositionTargets;
using ThaiX.Application.Features.AssetPositions.Queries.GetCryptoPositions;
using ThaiX.Application.Features.AssetPositions.Queries.GetPositionTransactions;
using ThaiX.Domain.Aggregates.AssetPositions;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class CryptoPositionEndpoints
{
    public static void MapCryptoPositionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/crypto-positions")
            .WithTags("Crypto Positions");

        // GET /api/crypto-positions?portfolioId=&isClosed=
        group.MapGet("/", async (
            [AsParameters] CryptoPositionQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCryptoPositionsQuery
            {
                PortfolioId = parameters.PortfolioId,
                IsClosed = parameters.IsClosed,
                PageNumber = parameters.PageNumber ?? 1,
                PageSize = parameters.PageSize ?? 20
            }, ct);
            var response = result.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.PositionRead);

        // GET /api/crypto-positions/{id}/transactions
        group.MapGet("/{id:guid}/transactions", async (
            Guid id,
            [AsParameters] PageQueryParameters paging,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetPositionTransactionsQuery
            {
                PositionId = id,
                AssetType = PositionAssetType.Crypto,
                PageNumber = paging.PageNumber ?? 1,
                PageSize = paging.PageSize ?? 20
            }, ct);
            var response = result.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.PositionRead);

        // POST /api/crypto-positions
        group.MapPost("/", async (CreateCryptoPositionCommand command, IMediator mediator, HttpContext httpContext, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/crypto-positions/{id}", response);
        })
        .RequireAuthorization(Permissions.PositionWrite);

        // POST /api/crypto-positions/{id}/transactions
        group.MapPost("/{id:guid}/transactions", async (Guid id, AddCryptoTransactionRequest request, IMediator mediator, HttpContext httpContext, CancellationToken ct) =>
        {
            await mediator.Send(new AddCryptoTransactionCommand
            {
                PositionId = id,
                TransactionType = request.TransactionType,
                Quantity = request.Quantity,
                Price = request.Price,
                Fee = request.Fee ?? 0,
                TransactedAt = request.TransactedAt,
                Note = request.Note,
                ExternalRef = request.ExternalRef
            }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.PositionWrite);

        // PUT /api/crypto-positions/{id}/targets
        group.MapPut("/{id:guid}/targets", async (Guid id, UpdateCryptoPositionTargetsRequest request, IMediator mediator, HttpContext httpContext, CancellationToken ct) =>
        {
            await mediator.Send(new UpdateCryptoPositionTargetsCommand
            {
                Id = id,
                TargetPrice = request.TargetPrice,
                StopLoss = request.StopLoss,
                Note = request.Note
            }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.PositionWrite);

        // DELETE /api/crypto-positions/{id}
        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, HttpContext httpContext, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteCryptoPositionCommand(id), ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.PositionDelete);
    }
}

public sealed record CryptoPositionQueryParameters
{
    [FromQuery(Name = "portfolioId")]
    public Guid PortfolioId { get; init; }

    [FromQuery(Name = "isClosed")]
    public bool? IsClosed { get; init; }

    [FromQuery(Name = "pageNumber")]
    [DefaultValue(1)]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    [DefaultValue(20)]
    public int? PageSize { get; init; }
}

public sealed record PageQueryParameters
{
    [FromQuery(Name = "pageNumber")]
    [DefaultValue(1)]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    [DefaultValue(20)]
    public int? PageSize { get; init; }
}

public sealed record AddCryptoTransactionRequest(
    TransactionType TransactionType,
    decimal Quantity,
    decimal Price,
    decimal? Fee,
    DateTime TransactedAt,
    string? Note,
    string? ExternalRef);

public sealed record UpdateCryptoPositionTargetsRequest(decimal? TargetPrice, decimal? StopLoss, string? Note);
