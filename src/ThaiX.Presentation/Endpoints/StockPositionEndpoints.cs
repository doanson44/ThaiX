using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using ThaiX.Application.Features.AssetPositions.Commands.AddStockTransaction;
using ThaiX.Application.Features.AssetPositions.Commands.CreateStockPosition;
using ThaiX.Application.Features.AssetPositions.Commands.DeleteStockPosition;
using ThaiX.Application.Features.AssetPositions.Commands.UpdateStockPositionTargets;
using ThaiX.Application.Features.AssetPositions.Queries.GetPositionTransactions;
using ThaiX.Application.Features.AssetPositions.Queries.GetStockPositions;
using ThaiX.Domain.Aggregates.AssetPositions;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class StockPositionEndpoints
{
    public static void MapStockPositionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/stock-positions")
            .WithTags("Stock Positions");

        // GET /api/stock-positions?portfolioId=&isClosed=
        group.MapGet("/", async (
            [AsParameters] StockPositionQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetStockPositionsQuery
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

        // GET /api/stock-positions/{id}/transactions
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
                AssetType = PositionAssetType.Stock,
                PageNumber = paging.PageNumber ?? 1,
                PageSize = paging.PageSize ?? 20
            }, ct);
            var response = result.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.PositionRead);

        // POST /api/stock-positions
        group.MapPost("/", async (CreateStockPositionCommand command, IMediator mediator, HttpContext httpContext, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/stock-positions/{id}", response);
        })
        .RequireAuthorization(Permissions.PositionWrite);

        // POST /api/stock-positions/{id}/transactions
        group.MapPost("/{id:guid}/transactions", async (Guid id, AddStockTransactionRequest request, IMediator mediator, HttpContext httpContext, CancellationToken ct) =>
        {
            await mediator.Send(new AddStockTransactionCommand
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

        // PUT /api/stock-positions/{id}/targets
        group.MapPut("/{id:guid}/targets", async (Guid id, UpdateStockPositionTargetsRequest request, IMediator mediator, HttpContext httpContext, CancellationToken ct) =>
        {
            await mediator.Send(new UpdateStockPositionTargetsCommand
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

        // DELETE /api/stock-positions/{id}
        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, HttpContext httpContext, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteStockPositionCommand(id), ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.PositionDelete);
    }
}

public sealed record StockPositionQueryParameters
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

public sealed record AddStockTransactionRequest(
    TransactionType TransactionType,
    decimal Quantity,
    decimal Price,
    decimal? Fee,
    DateTime TransactedAt,
    string? Note,
    string? ExternalRef);

public sealed record UpdateStockPositionTargetsRequest(decimal? TargetPrice, decimal? StopLoss, string? Note);
