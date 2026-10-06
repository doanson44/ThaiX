using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ThaiX.Application.Common.Constants;
using ThaiX.Domain.Common.Constants;
using ThaiX.Infrastructure.Identity;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

/// <summary>
/// API client management endpoints for machine-to-machine integrations.
/// </summary>
public static class ApiClientEndpoints
{
    public static void MapApiClientEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/api-clients")
            .WithTags("API Clients");

        group.MapGet("/", async (
            ApiClientService apiClientService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var clients = await apiClientService.GetAllAsync(cancellationToken);
            var response = ApiResponse<IReadOnlyList<ApiClientResponse>>.SuccessResult(
                clients.Select(MapResponse).ToList());
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.ApiClientRead)
        .WithName("GetApiClients")
        .WithDescription("Get all API clients used for machine-to-machine authentication.");

        group.MapGet("/available-scopes", (
            HttpContext httpContext) =>
        {
            var scopes = Permissions.GetAll()
                .OrderBy(static permission => permission, StringComparer.Ordinal)
                .ToList();

            var response = ApiResponse<IReadOnlyList<string>>.SuccessResult(scopes);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.ApiClientRead)
        .WithName("GetApiClientAvailableScopes")
        .WithDescription("Get all permission codes that can be granted to API clients.");

        group.MapGet("/{id:guid}", async (
            Guid id,
            ApiClientService apiClientService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var client = await apiClientService.GetByIdAsync(id, cancellationToken);
            if (client is null)
            {
                return NotFoundResult(httpContext, "API client was not found.");
            }

            var response = ApiResponse<ApiClientResponse>.SuccessResult(MapResponse(client));
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.ApiClientRead)
        .WithName("GetApiClientById")
        .WithDescription("Get a single API client by ID.");

        group.MapPost("/", async (
            [FromBody] CreateApiClientRequest request,
            ApiClientService apiClientService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var validationErrors = ValidateCreateRequest(request);
            if (validationErrors.Count > 0)
            {
                var errorResponse = ApiResponse<CreateApiClientResponse>.ValidationErrorResult(
                    "Validation failed.",
                    validationErrors);
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            if (!TryGetCurrentUserId(httpContext.User, out var createdBy))
            {
                return ForbiddenResult(
                    httpContext,
                    "Only user identities can create API clients.");
            }

            var normalizedScopes = NormalizeScopes(request.Scopes);
            var (client, plaintextSecret) = await apiClientService.CreateAsync(
                request.ClientId.Trim(),
                request.Name.Trim(),
                NormalizeOptional(request.Description),
                normalizedScopes,
                createdBy,
                cancellationToken);

            var response = ApiResponse<CreateApiClientResponse>.SuccessResult(
                MapCreatedResponse(client, plaintextSecret));
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/api-clients/{client.Id}", response);
        })
        .RequireAuthorization(Permissions.ApiClientWrite)
        .WithName("CreateApiClient")
        .WithDescription("Create a new API client and return its secret once.");

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateApiClientRequest request,
            ApiClientService apiClientService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var validationErrors = ValidateUpdateRequest(request);
            if (validationErrors.Count > 0)
            {
                var errorResponse = ApiResponse<ApiClientResponse>.ValidationErrorResult(
                    "Validation failed.",
                    validationErrors);
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var client = await apiClientService.GetByIdAsync(id, cancellationToken);
            if (client is null)
            {
                return NotFoundResult(httpContext, "API client was not found.");
            }

            var updated = await apiClientService.UpdateDetailsAsync(
                id,
                request.Name.Trim(),
                NormalizeOptional(request.Description),
                cancellationToken);

            var response = ApiResponse<ApiClientResponse>.SuccessResult(MapResponse(updated));
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.ApiClientWrite)
        .WithName("UpdateApiClient")
        .WithDescription("Update API client name and description.");

        group.MapPut("/{id:guid}/scopes", async (
            Guid id,
            [FromBody] UpdateApiClientScopesRequest request,
            ApiClientService apiClientService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var validationErrors = ValidateScopes(request.Scopes);
            if (validationErrors.Count > 0)
            {
                var errorResponse = ApiResponse<ApiClientResponse>.ValidationErrorResult(
                    "Validation failed.",
                    validationErrors);
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var client = await apiClientService.GetByIdAsync(id, cancellationToken);
            if (client is null)
            {
                return NotFoundResult(httpContext, "API client was not found.");
            }

            var updated = await apiClientService.UpdateScopesAsync(id, NormalizeScopes(request.Scopes), cancellationToken);

            var response = ApiResponse<ApiClientResponse>.SuccessResult(MapResponse(updated));
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.ApiClientManageScopes)
        .WithName("UpdateApiClientScopes")
        .WithDescription("Replace the scopes granted to an API client.");

        group.MapPost("/{id:guid}/regenerate-secret", async (
            Guid id,
            ApiClientService apiClientService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var client = await apiClientService.GetByIdAsync(id, cancellationToken);
            if (client is null)
            {
                return NotFoundResult(httpContext, "API client was not found.");
            }

            var plaintextSecret = await apiClientService.RegenerateSecretAsync(id, cancellationToken);
            var response = ApiResponse<RegenerateApiClientSecretResponse>.SuccessResult(
                new RegenerateApiClientSecretResponse
                {
                    Id = client.Id,
                    ClientId = client.ClientId,
                    ClientSecret = plaintextSecret
                });
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.ApiClientWrite)
        .WithName("RegenerateApiClientSecret")
        .WithDescription("Regenerate the client secret and return it once.");

        group.MapPost("/{id:guid}/activate", async (
            Guid id,
            ApiClientService apiClientService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var client = await apiClientService.GetByIdAsync(id, cancellationToken);
            if (client is null)
            {
                return NotFoundResult(httpContext, "API client was not found.");
            }

            var updated = await apiClientService.ActivateAsync(id, cancellationToken);

            var response = ApiResponse<ApiClientResponse>.SuccessResult(MapResponse(updated));
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.ApiClientWrite)
        .WithName("ActivateApiClient")
        .WithDescription("Activate an API client so it can authenticate.");

        group.MapPost("/{id:guid}/deactivate", async (
            Guid id,
            ApiClientService apiClientService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var client = await apiClientService.GetByIdAsync(id, cancellationToken);
            if (client is null)
            {
                return NotFoundResult(httpContext, "API client was not found.");
            }

            var updated = await apiClientService.DeactivateAsync(id, cancellationToken);

            var response = ApiResponse<ApiClientResponse>.SuccessResult(MapResponse(updated));
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.ApiClientWrite)
        .WithName("DeactivateApiClient")
        .WithDescription("Deactivate an API client so it can no longer authenticate.");

        group.MapDelete("/{id:guid}", async (
            Guid id,
            ApiClientService apiClientService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var client = await apiClientService.GetByIdAsync(id, cancellationToken);
            if (client is null)
            {
                return NotFoundResult(httpContext, "API client was not found.");
            }

            await apiClientService.DeleteAsync(id, cancellationToken);

            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.ApiClientDelete)
        .WithName("DeleteApiClient")
        .WithDescription("Soft-delete an API client.");
    }

    private static ApiClientResponse MapResponse(Domain.Aggregates.ApiClient.ApiClient client)
    {
        return new ApiClientResponse
        {
            Id = client.Id,
            ClientId = client.ClientId,
            Name = client.Name,
            Description = client.Description,
            IsActive = client.IsActive,
            Scopes = client.Scopes.OrderBy(static scope => scope, StringComparer.Ordinal).ToArray(),
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt
        };
    }

    private static CreateApiClientResponse MapCreatedResponse(
        Domain.Aggregates.ApiClient.ApiClient client,
        string plaintextSecret)
    {
        return new CreateApiClientResponse
        {
            Id = client.Id,
            ClientId = client.ClientId,
            Name = client.Name,
            Description = client.Description,
            IsActive = client.IsActive,
            Scopes = client.Scopes.OrderBy(static scope => scope, StringComparer.Ordinal).ToArray(),
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt,
            ClientSecret = plaintextSecret
        };
    }

    private static List<ValidationErrorDetail> ValidateCreateRequest(CreateApiClientRequest request)
    {
        var errors = new List<ValidationErrorDetail>();

        if (string.IsNullOrWhiteSpace(request.ClientId))
        {
            errors.Add(new ValidationErrorDetail
            {
                Field = nameof(request.ClientId),
                Message = "ClientId is required.",
                Code = ErrorCodes.REQUIRED_FIELD
            });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add(new ValidationErrorDetail
            {
                Field = nameof(request.Name),
                Message = "Name is required.",
                Code = ErrorCodes.REQUIRED_FIELD
            });
        }

        errors.AddRange(ValidateScopes(request.Scopes));

        return errors;
    }

    private static List<ValidationErrorDetail> ValidateUpdateRequest(UpdateApiClientRequest request)
    {
        var errors = new List<ValidationErrorDetail>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add(new ValidationErrorDetail
            {
                Field = nameof(request.Name),
                Message = "Name is required.",
                Code = ErrorCodes.REQUIRED_FIELD
            });
        }

        return errors;
    }

    private static List<ValidationErrorDetail> ValidateScopes(IEnumerable<string>? scopes)
    {
        var errors = new List<ValidationErrorDetail>();
        var availableScopes = Permissions.GetAll().ToHashSet(StringComparer.Ordinal);

        foreach (var scope in scopes ?? Array.Empty<string>())
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                errors.Add(new ValidationErrorDetail
                {
                    Field = "Scopes",
                    Message = "Scopes cannot contain empty values.",
                    Code = ErrorCodes.INVALID_FORMAT
                });
                continue;
            }

            var normalizedScope = scope.Trim();
            if (!availableScopes.Contains(normalizedScope))
            {
                errors.Add(new ValidationErrorDetail
                {
                    Field = "Scopes",
                    Message = $"Unknown scope '{normalizedScope}'.",
                    Code = ErrorCodes.INVALID_FORMAT
                });
            }
        }

        return errors;
    }

    private static string[] NormalizeScopes(IEnumerable<string>? scopes)
    {
        return (scopes ?? Array.Empty<string>())
            .Where(static scope => !string.IsNullOrWhiteSpace(scope))
            .Select(static scope => scope.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static scope => scope, StringComparer.Ordinal)
            .ToArray();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool TryGetCurrentUserId(ClaimsPrincipal user, out Guid userId)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out userId);
    }

    private static IResult NotFoundResult(HttpContext httpContext, string message)
    {
        var response = ApiResponse.ErrorResult(ErrorCodes.RESOURCE_NOT_FOUND, message);
        response.Metadata.CorrelationId = httpContext.GetCorrelationId();
        return Results.NotFound(response);
    }

    private static IResult ForbiddenResult(HttpContext httpContext, string message)
    {
        var response = ApiResponse.ErrorResult(ErrorCodes.FORBIDDEN, message);
        response.Metadata.CorrelationId = httpContext.GetCorrelationId();
        return Results.Json(response, statusCode: StatusCodes.Status403Forbidden);
    }
}

public sealed record CreateApiClientRequest
{
    public string ClientId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public IReadOnlyCollection<string>? Scopes { get; init; }
}

public sealed record UpdateApiClientRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed record UpdateApiClientScopesRequest
{
    public IReadOnlyCollection<string>? Scopes { get; init; }
}

public record ApiClientResponse
{
    public required Guid Id { get; init; }
    public required string ClientId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required bool IsActive { get; init; }
    public required IReadOnlyCollection<string> Scopes { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime? UpdatedAt { get; init; }
}

public sealed record CreateApiClientResponse : ApiClientResponse
{
    public required string ClientSecret { get; init; }
}

public sealed record RegenerateApiClientSecretResponse
{
    public required Guid Id { get; init; }
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
}
