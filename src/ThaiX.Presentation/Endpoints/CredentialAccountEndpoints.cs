using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using ThaiX.Application.Features.CredentialAccounts.Commands.ChangeCredentialAccountPassword;
using ThaiX.Application.Features.CredentialAccounts.Commands.CreateCredentialAccount;
using ThaiX.Application.Features.CredentialAccounts.Commands.DeleteCredentialAccount;
using ThaiX.Application.Features.CredentialAccounts.Commands.MarkCredentialAccountUsed;
using ThaiX.Application.Features.CredentialAccounts.Commands.ResetCredentialAccountUsed;
using ThaiX.Application.Features.CredentialAccounts.Commands.UpdateCredentialAccount;
using ThaiX.Application.Features.CredentialAccounts.Queries.GetCredentialAccountAuditLogs;
using ThaiX.Application.Features.CredentialAccounts.Queries.GetCredentialAccountById;
using ThaiX.Application.Features.CredentialAccounts.Queries.GetCredentialAccountPassword;
using ThaiX.Application.Features.CredentialAccounts.Queries.GetCredentialAccounts;
using ThaiX.Domain.Aggregates.CredentialAccounts;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class CredentialAccountEndpoints
{
    public static void MapCredentialAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/credential-accounts")
            .WithTags("Credential Accounts");

        group.MapGet("/", async (
            [AsParameters] CredentialAccountQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCredentialAccountsQuery
            {
                PageNumber = parameters.PageNumber ?? 1,
                PageSize = parameters.PageSize ?? 20,
                SearchTerm = parameters.SearchTerm,
                IsUsed = parameters.IsUsed,
                SortBy = parameters.SortBy,
                SortDescending = parameters.SortDescending ?? false
            }, ct);

            return Results.Ok(result.ToPagedApiResponse(httpContext.GetCorrelationId()));
        })
        .RequireAuthorization(Permissions.CredentialAccountRead);

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCredentialAccountByIdQuery { Id = id }, ct);
            if (result is null)
            {
                return Results.NotFound();
            }

            var response = ApiResponse<CredentialAccountDetailDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.CredentialAccountRead);

        group.MapGet("/{id:guid}/audit-logs", async (
            Guid id,
            [AsParameters] CredentialAccountAuditQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCredentialAccountAuditLogsQuery
            {
                CredentialAccountId = id,
                PageNumber = parameters.PageNumber ?? 1,
                PageSize = parameters.PageSize ?? 20
            }, ct);

            return Results.Ok(result.ToPagedApiResponse(httpContext.GetCorrelationId()));
        })
        .RequireAuthorization(Permissions.CredentialAccountRead);

        group.MapPost("/", async (
            CreateCredentialAccountRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var id = await mediator.Send(new CreateCredentialAccountCommand
            {
                Username = request.Username,
                Password = request.Password,
                Description = request.Description
            }, ct);

            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/credential-accounts/{id}", response);
        })
        .RequireAuthorization(Permissions.CredentialAccountWrite);

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateCredentialAccountRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            await mediator.Send(new UpdateCredentialAccountCommand
            {
                Id = id,
                Username = request.Username,
                Description = request.Description
            }, ct);

            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.CredentialAccountWrite);

        group.MapPut("/{id:guid}/password", async (
            Guid id,
            ChangeCredentialAccountPasswordRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            await mediator.Send(new ChangeCredentialAccountPasswordCommand
            {
                Id = id,
                Password = request.Password
            }, ct);

            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.CredentialAccountWrite);

        group.MapPost("/{id:guid}/mark-used", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            await mediator.Send(new MarkCredentialAccountUsedCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.CredentialAccountWrite);

        group.MapPost("/{id:guid}/reset-used", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            await mediator.Send(new ResetCredentialAccountUsedCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.CredentialAccountWrite);

        group.MapPost("/{id:guid}/password/view", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCredentialAccountPasswordQuery
            {
                Id = id,
                AuditAction = CredentialAccountAuditAction.PasswordViewed
            }, ct);

            if (result is null)
            {
                return Results.NotFound();
            }

            var response = ApiResponse<CredentialAccountPasswordDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.CredentialAccountViewPassword);

        group.MapPost("/{id:guid}/password/copy", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCredentialAccountPasswordQuery
            {
                Id = id,
                AuditAction = CredentialAccountAuditAction.PasswordCopied
            }, ct);

            if (result is null)
            {
                return Results.NotFound();
            }

            var response = ApiResponse<CredentialAccountPasswordDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.CredentialAccountViewPassword);

        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            await mediator.Send(new DeleteCredentialAccountCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.CredentialAccountDelete);
    }
}

public sealed record CreateCredentialAccountRequest
{
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string? Description { get; init; }
}

public sealed record UpdateCredentialAccountRequest
{
    public string Username { get; init; } = string.Empty;

    public string? Description { get; init; }
}

public sealed record ChangeCredentialAccountPasswordRequest
{
    public string Password { get; init; } = string.Empty;
}

public sealed record CredentialAccountQueryParameters
{
    [FromQuery(Name = "pageNumber")]
    [DefaultValue(1)]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    [DefaultValue(20)]
    public int? PageSize { get; init; }

    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }

    [FromQuery(Name = "sortBy")]
    public string? SortBy { get; init; }

    [FromQuery(Name = "sortDescending")]
    public bool? SortDescending { get; init; }

    [FromQuery(Name = "isUsed")]
    public bool? IsUsed { get; init; }
}

public sealed record CredentialAccountAuditQueryParameters
{
    [FromQuery(Name = "pageNumber")]
    [DefaultValue(1)]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    [DefaultValue(20)]
    public int? PageSize { get; init; }
}
