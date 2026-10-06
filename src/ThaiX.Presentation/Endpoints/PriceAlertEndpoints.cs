using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using ThaiX.Application.Features.PriceAlerts.Commands.CreatePriceAlert;
using ThaiX.Application.Features.PriceAlerts.Commands.DeletePriceAlert;
using ThaiX.Application.Features.PriceAlerts.Commands.UpdatePriceAlert;
using ThaiX.Application.Features.PriceAlerts.Queries.GetPriceAlerts;
using ThaiX.Domain.Aggregates.PriceAlerts;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

/// <summary>
/// Endpoints for managing price alerts.
/// </summary>
public static class PriceAlertEndpoints
{
    public static void MapPriceAlertEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/price-alerts")
            .WithTags("Price Alerts");

        // GET /api/price-alerts
        group.MapGet("/", async (
            [AsParameters] PriceAlertQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPriceAlertsQuery
            {
                PageNumber = parameters.PageNumber ?? PriceAlertQueryParameters.DefaultPageNumber,
                PageSize = parameters.PageSize ?? PriceAlertQueryParameters.DefaultPageSize,
                SortBy = string.IsNullOrWhiteSpace(parameters.SortBy)
                    ? PriceAlertQueryParameters.DefaultSortBy
                    : parameters.SortBy.Trim(),
                SortDescending = parameters.SortDescending ?? PriceAlertQueryParameters.DefaultSortDescending,
                SearchTerm = parameters.SearchTerm,
                IsEnabled = parameters.IsEnabled
            };

            var pagedResult = await mediator.Send(query, cancellationToken);
            var response = pagedResult.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.PriceAlertRead)
        .WithName("GetPriceAlerts")
        .WithDescription("Get paginated list of price alerts");



        // POST /api/price-alerts
        group.MapPost("/", async (
            [FromBody] CreatePriceAlertCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var id = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/price-alerts/{id}", response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.PriceAlertWrite)
        .WithName("CreatePriceAlert")
        .WithDescription("Create a new price alert");

        // PUT /api/price-alerts/{id}
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdatePriceAlertRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdatePriceAlertCommand
            {
                Id = id,
                Condition = request.Condition,
                TargetPrice = request.TargetPrice,
                Note = request.Note,
                IsOneTime = request.IsOneTime,
                IsEnabled = request.IsEnabled
            };

            await mediator.Send(command, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.PriceAlertWrite)
        .WithName("UpdatePriceAlert")
        .WithDescription("Update an existing price alert");

        // DELETE /api/price-alerts/{id}
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeletePriceAlertCommand { Id = id }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.PriceAlertDelete)
        .WithName("DeletePriceAlert")
        .WithDescription("Soft-delete a price alert");
    }
}

/// <summary>
/// Query parameters for the price alerts list endpoint.
/// </summary>
public sealed record PriceAlertQueryParameters
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 20;
    public const string DefaultSortBy = "symbol";
    public const bool DefaultSortDescending = false;

    [FromQuery(Name = "pageNumber")]
    [DefaultValue(DefaultPageNumber)]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    [DefaultValue(DefaultPageSize)]
    public int? PageSize { get; init; }

    [FromQuery(Name = "sortBy")]
    [DefaultValue(DefaultSortBy)]
    public string? SortBy { get; init; }

    [FromQuery(Name = "sortDescending")]
    [DefaultValue(DefaultSortDescending)]
    public bool? SortDescending { get; init; }

    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }

    [FromQuery(Name = "isEnabled")]
    public bool? IsEnabled { get; init; }
}

/// <summary>
/// Request body for updating a price alert.
/// </summary>
public sealed record UpdatePriceAlertRequest
{
    public required AlertCondition Condition { get; init; }
    public required decimal TargetPrice { get; init; }
    public string? Note { get; init; }
    public required bool IsOneTime { get; init; }
    public required bool IsEnabled { get; init; }
}

/// <summary>
/// Response for symbol search endpoint.
/// </summary>
public sealed record SymbolSearchResult(IReadOnlyList<string> Symbols, int TotalCount);
