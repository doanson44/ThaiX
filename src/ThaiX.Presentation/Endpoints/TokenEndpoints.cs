using MediatR;
using Microsoft.AspNetCore.Mvc;
using ThaiX.Application.Features.Authentication.Commands.ClientCredentials;
using ThaiX.Application.Features.Authentication.Commands.Login;
using ThaiX.Infrastructure.Security;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

/// <summary>
/// Token issuance endpoints for JWT authentication.
/// Supports both user login and client credentials flows.
/// </summary>
public static class TokenEndpoints
{
    public static void MapTokenEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var tokenGroup = endpoints.MapGroup("/api/token")
            .WithTags("Authentication");

        // User login endpoint (interactive authentication)
        tokenGroup.MapPost("/login", async (
            [FromBody] LoginRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginCommand
            {
                Username = request.Username,
                Password = request.Password,
                RememberMe = request.RememberMe
            };

            var loginResult = await mediator.Send(command, cancellationToken);

            if (loginResult.RequiresTwoFactor)
            {
                var twoFaResponse = ApiResponse<object>.SuccessResult(
                    new { loginResult.RequiresTwoFactor, loginResult.TwoFactorSessionToken });
                twoFaResponse.Metadata.CorrelationId = GetCorrelationId(httpContext);
                return Results.Ok(twoFaResponse);
            }

            var response = ApiResponse<TokenResponse>.SuccessResult(
                new TokenResponse
                {
                    AccessToken = loginResult.Token!.AccessToken,
                    TokenType = loginResult.Token.TokenType,
                    ExpiresIn = loginResult.Token.ExpiresIn,
                    Scope = loginResult.Token.Scope
                });

            response.Metadata.CorrelationId = GetCorrelationId(httpContext);

            return Results.Ok(response);
        })
        .WithName("UserLogin")
        .AllowAnonymous();

        // Client credentials endpoint (M2M authentication)
        tokenGroup.MapPost("/client-credentials", async (
            [FromBody] ClientCredentialsRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new ClientCredentialsCommand
            {
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                Scope = request.Scope
            };

            var tokenDto = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<TokenResponse>.SuccessResult(
                new TokenResponse
                {
                    AccessToken = tokenDto.AccessToken,
                    TokenType = tokenDto.TokenType,
                    ExpiresIn = tokenDto.ExpiresIn,
                    Scope = tokenDto.Scope
                });

            response.Metadata.CorrelationId = GetCorrelationId(httpContext);

            return Results.Ok(response);
        })
        .WithName("ClientCredentials")
        .AllowAnonymous();

        // Token introspection endpoint (optional, for debugging)
        tokenGroup.MapPost("/introspect", (
            [FromBody] IntrospectRequest request,
            JwtTokenService jwtService,
            HttpContext httpContext) =>
        {
            var command = new IntrospectRequest(request.Token);
            var principal = jwtService.ValidateToken(command.Token);

            if (principal == null)
            {
                var inactiveResponse = ApiResponse<IntrospectionResponse>.SuccessResult(
                    new IntrospectionResponse { Active = false });

                inactiveResponse.Metadata.CorrelationId = GetCorrelationId(httpContext);

                return Results.Ok(inactiveResponse);
            }

            var sub = principal.FindFirst("sub")?.Value;
            var actorType = principal.FindFirst("actor_type")?.Value;
            var scopes = principal.FindAll("scope").Select(c => c.Value).ToList();
            var exp = principal.FindFirst("exp")?.Value;
            var clientId = principal.FindFirst("client_id")?.Value;
            var email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            var response = ApiResponse<IntrospectionResponse>.SuccessResult(
                new IntrospectionResponse
                {
                    Active = true,
                    Sub = sub,
                    ActorType = actorType,
                    Scope = string.Join(" ", scopes),
                    Exp = exp,
                    ClientId = clientId,
                    Email = email
                });

            response.Metadata.CorrelationId = GetCorrelationId(httpContext);

            return Results.Ok(response);
        })
        .WithName("TokenIntrospect")
        .RequireAuthorization(); // Require authentication to introspect
    }

    private static string GetCorrelationId(HttpContext context)
    {
        return context.Items.TryGetValue("CorrelationId", out var correlationId)
            ? correlationId?.ToString() ?? context.TraceIdentifier
            : context.TraceIdentifier;
    }
}

/// <summary>
/// Request model for user login.
/// </summary>
public sealed record LoginRequest(string Username, string Password, bool RememberMe = false);

/// <summary>
/// Request model for client credentials flow.
/// </summary>
public sealed record ClientCredentialsRequest(string ClientId, string ClientSecret, string? Scope);

/// <summary>
/// Request model for token introspection.
/// </summary>
public sealed record IntrospectRequest(string Token);

/// <summary>
/// Response model for successful token issuance.
/// </summary>
public sealed record TokenResponse
{
    public required string AccessToken { get; init; }
    public required string TokenType { get; init; }
    public required int ExpiresIn { get; init; }
    public required string Scope { get; init; }
}

/// <summary>
/// Response model for token introspection.
/// </summary>
public sealed record IntrospectionResponse
{
    public bool Active { get; init; }
    public string? Sub { get; init; }
    public string? ActorType { get; init; }
    public string? Scope { get; init; }
    public string? Exp { get; init; }
    public string? ClientId { get; init; }
    public string? Email { get; init; }
}
