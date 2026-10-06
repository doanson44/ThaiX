using MediatR;
using ThaiX.Application.Features.JsonBins.Commands.CreateJsonBin;
using ThaiX.Application.Features.JsonBins.Commands.DeleteJsonBin;
using ThaiX.Application.Features.JsonBins.Commands.ExpireJsonBin;
using ThaiX.Application.Features.JsonBins.Commands.GenerateJsonBinShareLink;
using ThaiX.Application.Features.JsonBins.Commands.RevokeJsonBinShareLink;
using ThaiX.Application.Features.JsonBins.Commands.UpdateJsonBin;
using ThaiX.Application.Features.JsonBins.Queries.GetJsonBinByCode;
using ThaiX.Application.Features.JsonBins.Queries.GetJsonBinById;
using ThaiX.Application.Features.JsonBins.Queries.GetJsonBinContent;
using ThaiX.Application.Features.JsonBins.Queries.GetJsonBins;
using ThaiX.Application.Features.JsonBins.Queries.ValidateJsonBinCode;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class JsonBinEndpoints
{
    public static void MapJsonBinEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/json-bins")
            .WithTags("JsonBins");

        group.MapGet("/code-exists", async (
            string code,
            Guid? excludeId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ValidateJsonBinCodeQuery
            {
                Code = code,
                ExcludeId = excludeId
            }, ct);

            var response = ApiResponse<ValidateJsonBinCodeResult>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.JsonBinRead);

        group.MapGet("/by-code/{code}", async (
            string code,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetJsonBinByCodeQuery { Code = code }, ct);
            if (result is null)
                return Results.NotFound();

            var response = ApiResponse<JsonBinDetailDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.JsonBinRead);

        group.MapGet("/", async (
            [AsParameters] JsonBinQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetJsonBinsQuery
            {
                PageNumber = parameters.PageNumber ?? 1,
                PageSize = parameters.PageSize ?? 50,
                SearchTerm = parameters.SearchTerm,
                Category = parameters.Category,
                ReferenceId = parameters.ReferenceId,
                IncludeExpired = parameters.IncludeExpired,
                SortBy = parameters.SortBy,
                SortDescending = parameters.SortDescending ?? false
            }, ct);

            var response = result.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.JsonBinRead);

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetJsonBinByIdQuery { Id = id }, ct);
            if (result is null)
                return Results.NotFound();

            var response = ApiResponse<JsonBinDetailDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.JsonBinRead);

        group.MapGet("/{id:guid}/content", async (
            Guid id,
            bool? decompress,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetJsonBinContentQuery
            {
                Id = id,
                Decompress = decompress ?? true
            }, ct);

            if (result is null)
                return Results.NotFound();

            return Results.File(result.Content, result.ContentType, fileDownloadName: $"{id}.bin");
        })
        .RequireAuthorization(Permissions.JsonBinRead);

        group.MapPost("/", async (
            CreateJsonBinCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var code = await mediator.Send(command, ct);
            var response = ApiResponse<string>.SuccessResult(code);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/json-bins/by-code/{Uri.EscapeDataString(code)}", response);
        })
        .RequireAuthorization(Permissions.JsonBinWrite);

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateJsonBinRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var code = await mediator.Send(new UpdateJsonBinCommand
            {
                Id = id,
                Code = request.Code,
                Name = request.Name,
                Category = request.Category,
                ContentJson = request.ContentJson,
                ContentType = request.ContentType,
                Compress = request.Compress,
                Tags = request.Tags,
                ReferenceId = request.ReferenceId,
                ExpiredAtUtc = request.ExpiredAtUtc,
                ClearExpiration = request.ClearExpiration
            }, ct);

            var response = ApiResponse<string>.SuccessResult(code);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.JsonBinWrite);

        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            await mediator.Send(new DeleteJsonBinCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.JsonBinWrite);

        group.MapPost("/{id:guid}/expire", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            await mediator.Send(new ExpireJsonBinCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.JsonBinWrite);

        group.MapPost("/{id:guid}/share", async (
            Guid id,
            GenerateShareLinkRequest? request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GenerateJsonBinShareLinkCommand
            {
                Id = id,
                ShareExpiresAtUtc = request?.ShareExpiresAtUtc
            }, ct);

            var response = ApiResponse<GenerateJsonBinShareLinkResult>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.JsonBinWrite);

        group.MapDelete("/{id:guid}/share", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            await mediator.Send(new RevokeJsonBinShareLinkCommand { Id = id }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.JsonBinWrite);
    }
}
