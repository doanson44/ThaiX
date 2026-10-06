using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Net;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Presentation.Localization;
using ThaiX.Presentation.Models;
using static ThaiX.Presentation.Resources.ResourceKeys;

namespace ThaiX.Presentation.Middleware;

/// <summary>
/// Global exception handler middleware that catches all unhandled exceptions
/// and converts them to unified API error responses.
/// </summary>
public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment,
        IStringLocalizer<SharedResource> localizer)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
        _localizer = localizer;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleUnauthorizedAccessExceptionAsync(context, ex);
        }
        catch (InvalidOperationException ex)
        {
            await HandleInvalidOperationExceptionAsync(context, ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await HandleDbUpdateConcurrencyExceptionAsync(context, ex);
        }
        catch (OperationFailedException ex)
        {
            await HandleOperationFailedExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleUnexpectedExceptionAsync(context, ex);
        }
    }

    private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        var correlationId = GetCorrelationId(context);

        _logger.LogWarning(exception,
            "Validation failed. CorrelationId: {CorrelationId}",
            correlationId);

        var validationErrors = exception.Errors
            .Select(e => new ValidationErrorDetail
            {
                Field = e.PropertyName,
                Message = e.ErrorMessage,
                Code = e.ErrorCode
            })
            .ToList();

        var response = ApiResponse.ValidationErrorResult(
            T(Error.ValidationFailed, ErrorMessages.VALIDATION_FAILED),
            validationErrors);

        response.Metadata.CorrelationId = correlationId;

        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        await WriteJsonResponseAsync(context, response);
    }

    private async Task HandleUnauthorizedAccessExceptionAsync(HttpContext context, UnauthorizedAccessException exception)
    {
        var correlationId = GetCorrelationId(context);

        _logger.LogWarning(exception,
            "Unauthorized access attempt. CorrelationId: {CorrelationId}",
            correlationId);

        var response = ApiResponse.ErrorResult(
            ErrorCodes.FORBIDDEN,
            T(Error.Forbidden, ErrorMessages.FORBIDDEN));

        response.Metadata.CorrelationId = correlationId;

        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        await WriteJsonResponseAsync(context, response);
    }

    private async Task HandleInvalidOperationExceptionAsync(HttpContext context, InvalidOperationException exception)
    {
        var correlationId = GetCorrelationId(context);

        _logger.LogWarning(exception,
            "Invalid operation. CorrelationId: {CorrelationId}",
            correlationId);

        var response = ApiResponse.ErrorResult(
            ErrorCodes.OPERATION_NOT_ALLOWED,
            exception.Message);

        response.Metadata.CorrelationId = correlationId;

        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        await WriteJsonResponseAsync(context, response);
    }

    private async Task HandleDbUpdateConcurrencyExceptionAsync(HttpContext context, DbUpdateConcurrencyException exception)
    {
        var correlationId = GetCorrelationId(context);

        _logger.LogWarning(exception,
            "Optimistic concurrency conflict. CorrelationId: {CorrelationId}",
            correlationId);

        var response = ApiResponse.ErrorResult(
            ErrorCodes.CONCURRENCY_CONFLICT,
            T(Error.ConcurrencyConflict, ErrorMessages.CONCURRENCY_CONFLICT));

        response.Metadata.CorrelationId = correlationId;

        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await WriteJsonResponseAsync(context, response);
    }

    private async Task HandleUnexpectedExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = GetCorrelationId(context);

        _logger.LogError(exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}",
            correlationId);

        var message = _environment.IsDevelopment() || _environment.IsEnvironment("Testing")
            ? $"{T(Error.InternalError, ErrorMessages.INTERNAL_ERROR)} Details: {exception.Message} --- Full: {exception}"
            : T(Error.InternalError, ErrorMessages.INTERNAL_ERROR);

        var response = ApiResponse.ErrorResult(
            ErrorCodes.INTERNAL_ERROR,
            message);

        response.Metadata.CorrelationId = correlationId;

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        await WriteJsonResponseAsync(context, response);
    }

    private async Task HandleOperationFailedExceptionAsync(HttpContext context, OperationFailedException exception)
    {
        var correlationId = GetCorrelationId(context);
        var statusCode = MapStatusCode(exception.ErrorCode);

        _logger.LogWarning(
            exception,
            "Operation failed with code {ErrorCode}. CorrelationId: {CorrelationId}",
            exception.ErrorCode,
            correlationId);

        var response = ApiResponse.ErrorResult(
            exception.ErrorCode,
            GetLocalizedOperationErrorMessage(exception.ErrorCode, exception.Message));

        response.Metadata.CorrelationId = correlationId;

        context.Response.StatusCode = statusCode;
        await WriteJsonResponseAsync(context, response);
    }

    private static string GetCorrelationId(HttpContext context)
    {
        return context.Items.TryGetValue("CorrelationId", out var correlationId)
            ? correlationId?.ToString() ?? context.TraceIdentifier
            : context.TraceIdentifier;
    }

    private string T(string key, string fallback)
    {
        var localized = _localizer[key];
        return localized.ResourceNotFound ? fallback : localized.Value;
    }

    private string GetLocalizedOperationErrorMessage(string errorCode, string fallbackMessage)
    {
        return errorCode switch
        {
            ErrorCodes.INVALID_CREDENTIALS => T(Error.InvalidCredentials, ErrorMessages.INVALID_CREDENTIALS),
            ErrorCodes.ACCOUNT_LOCKED => T(Error.AccountLocked, ErrorMessages.ACCOUNT_LOCKED),
            ErrorCodes.ACCOUNT_NOT_ALLOWED => T(Error.AccountNotAllowed, ErrorMessages.ACCOUNT_NOT_ALLOWED),
            ErrorCodes.UNAUTHORIZED => T(Error.Unauthorized, ErrorMessages.UNAUTHORIZED),
            ErrorCodes.FORBIDDEN => T(Error.Forbidden, ErrorMessages.FORBIDDEN),
            ErrorCodes.INSUFFICIENT_PERMISSIONS => T(Error.InsufficientPermissions, ErrorMessages.INSUFFICIENT_PERMISSIONS),
            ErrorCodes.INVALID_CLIENT_CREDENTIALS => T(Error.InvalidClientCredentials, ErrorMessages.INVALID_CLIENT_CREDENTIALS),
            ErrorCodes.CLIENT_INACTIVE => T(Error.ClientInactive, ErrorMessages.CLIENT_INACTIVE),
            ErrorCodes.UNAUTHORIZED_SCOPE => T(Error.UnauthorizedScope, ErrorMessages.UNAUTHORIZED_SCOPE),
            ErrorCodes.RESOURCE_NOT_FOUND => T(Error.ResourceNotFound, ErrorMessages.RESOURCE_NOT_FOUND),
            ErrorCodes.DUPLICATE_ENTRY => T(Error.DuplicateEntry, ErrorMessages.RESOURCE_ALREADY_EXISTS),
            ErrorCodes.OPERATION_NOT_ALLOWED => T(Error.OperationNotAllowed, ErrorMessages.OPERATION_NOT_ALLOWED),
            ErrorCodes.CONCURRENCY_CONFLICT => T(Error.ConcurrencyConflict, ErrorMessages.CONCURRENCY_CONFLICT),
            ErrorCodes.INTERNAL_ERROR => T(Error.InternalError, ErrorMessages.INTERNAL_ERROR),
            ErrorCodes.DATABASE_ERROR => T(Error.DatabaseError, ErrorMessages.DATABASE_ERROR),
            ErrorCodes.EXTERNAL_SERVICE_ERROR => T(Error.ExternalServiceError, ErrorMessages.EXTERNAL_SERVICE_ERROR),
            ErrorCodes.RATE_LIMIT_EXCEEDED => T(Error.RateLimitExceeded, ErrorMessages.RATE_LIMIT_EXCEEDED),
            ErrorCodes.TOO_MANY_REQUESTS => T(Error.RateLimitExceeded, ErrorMessages.RATE_LIMIT_EXCEEDED),
            ErrorCodes.CONTACT_IMPORT_INVALID_CSV_TEMPLATE => T(Error.ContactImportInvalidCsvTemplate, "CSV must match the standard import template. Download the template and convert your file to the required format."),
            _ => fallbackMessage
        };
    }

    private static async Task WriteJsonResponseAsync(HttpContext context, object response)
    {
        context.Response.ContentType = "application/json";

        // Avoid writing if response has already started
        if (!context.Response.HasStarted)
        {
            await context.Response.WriteAsJsonAsync(response);
        }
    }

    private static int MapStatusCode(string errorCode)
    {
        return errorCode switch
        {
            ErrorCodes.ACCOUNT_LOCKED => StatusCodes.Status423Locked,
            ErrorCodes.ACCOUNT_NOT_ALLOWED => StatusCodes.Status403Forbidden,
            ErrorCodes.INVALID_CREDENTIALS => StatusCodes.Status401Unauthorized,
            ErrorCodes.UNAUTHORIZED => StatusCodes.Status401Unauthorized,
            ErrorCodes.FORBIDDEN => StatusCodes.Status403Forbidden,
            ErrorCodes.INSUFFICIENT_PERMISSIONS => StatusCodes.Status403Forbidden,
            ErrorCodes.INVALID_CLIENT_CREDENTIALS => StatusCodes.Status401Unauthorized,
            ErrorCodes.CLIENT_INACTIVE => StatusCodes.Status403Forbidden,
            ErrorCodes.UNAUTHORIZED_SCOPE => StatusCodes.Status403Forbidden,
            ErrorCodes.RESOURCE_NOT_FOUND => StatusCodes.Status404NotFound,
            ErrorCodes.DUPLICATE_ENTRY => StatusCodes.Status409Conflict,
            ErrorCodes.CONCURRENCY_CONFLICT => StatusCodes.Status409Conflict,
            ErrorCodes.INTERNAL_ERROR => StatusCodes.Status500InternalServerError,
            ErrorCodes.DATABASE_ERROR => StatusCodes.Status500InternalServerError,
            ErrorCodes.EXTERNAL_SERVICE_ERROR => StatusCodes.Status502BadGateway,
            ErrorCodes.TIMEOUT => StatusCodes.Status504GatewayTimeout,
            ErrorCodes.RATE_LIMIT_EXCEEDED => StatusCodes.Status429TooManyRequests,
            ErrorCodes.TOO_MANY_REQUESTS => StatusCodes.Status429TooManyRequests,
            ErrorCodes.CONTACT_IMPORT_INVALID_CSV_TEMPLATE => StatusCodes.Status400BadRequest,
            _ when errorCode.StartsWith(ErrorCodes.VALIDATION_PREFIX, StringComparison.Ordinal) => StatusCodes.Status400BadRequest,
            _ when errorCode.StartsWith(ErrorCodes.BUSINESS_PREFIX, StringComparison.Ordinal) => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };
    }
}

/// <summary>
/// Extension methods for registering GlobalExceptionHandlerMiddleware.
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
