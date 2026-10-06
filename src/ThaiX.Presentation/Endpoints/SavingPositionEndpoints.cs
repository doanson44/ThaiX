using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using ThaiX.Application.Features.AssetPositions.Commands.CreateSavingPosition;
using ThaiX.Application.Features.AssetPositions.Commands.DeleteSavingPosition;
using ThaiX.Application.Features.AssetPositions.Commands.UpdateSavingPosition;
using ThaiX.Application.Features.AssetPositions.Commands.WithdrawSavingPosition;
using ThaiX.Application.Features.AssetPositions.Queries.GetSavingPositions;
using ThaiX.Domain.Aggregates.AssetPositions;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class SavingPositionEndpoints
{
    public static void MapSavingPositionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/saving-positions")
            .WithTags("Saving Positions");

        // GET /api/saving-positions?portfolioId=&status=
        group.MapGet("/", async (
            [AsParameters] SavingPositionQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetSavingPositionsQuery
            {
                PortfolioId = parameters.PortfolioId,
                Status = parameters.Status,
                PageNumber = parameters.PageNumber ?? 1,
                PageSize = parameters.PageSize ?? 20
            }, ct);
            var response = result.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.PositionRead);

        // POST /api/saving-positions
        group.MapPost("/", async (CreateSavingPositionCommand command, IMediator mediator, HttpContext httpContext, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/saving-positions/{id}", response);
        })
        .RequireAuthorization(Permissions.PositionWrite);

        // PUT /api/saving-positions/{id}
        group.MapPut("/{id:guid}", async (Guid id, UpdateSavingPositionRequest request, IMediator mediator, HttpContext httpContext, CancellationToken ct) =>
        {
            await mediator.Send(new UpdateSavingPositionCommand
            {
                Id = id,
                BankName = request.BankName,
                AccountNumber = request.AccountNumber,
                PrincipalAmount = request.PrincipalAmount,
                InterestRate = request.InterestRate,
                InterestType = request.InterestType,
                DepositDate = request.DepositDate,
                MaturityDate = request.MaturityDate,
                Note = request.Note
            }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.PositionWrite);

        // POST /api/saving-positions/{id}/withdraw
        group.MapPost("/{id:guid}/withdraw", async (Guid id, WithdrawRequest request, IMediator mediator, HttpContext httpContext, CancellationToken ct) =>
        {
            await mediator.Send(new WithdrawSavingPositionCommand(id, request.WithdrawalDate), ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.PositionWrite);

        // DELETE /api/saving-positions/{id}
        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, HttpContext httpContext, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteSavingPositionCommand(id), ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.PositionDelete);
    }
}

public sealed record SavingPositionQueryParameters
{
    [FromQuery(Name = "portfolioId")]
    public Guid PortfolioId { get; init; }

    [FromQuery(Name = "status")]
    public SavingStatus? Status { get; init; }

    [FromQuery(Name = "pageNumber")]
    [DefaultValue(1)]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    [DefaultValue(20)]
    public int? PageSize { get; init; }
}

public sealed record UpdateSavingPositionRequest(
    string BankName,
    string? AccountNumber,
    decimal PrincipalAmount,
    decimal InterestRate,
    InterestType InterestType,
    DateOnly DepositDate,
    DateOnly? MaturityDate,
    string? Note);

public sealed record WithdrawRequest(DateOnly WithdrawalDate);
