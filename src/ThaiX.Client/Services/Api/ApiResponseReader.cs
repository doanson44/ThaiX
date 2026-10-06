using System.Net;
using System.Net.Http.Json;
using ThaiX.Client.Models.Api;

namespace ThaiX.Client.Services.Api;

/// <summary>
/// Reads API envelope responses and throws ApiException for error responses.
/// </summary>
public static class ApiResponseReader
{
    public static async Task<TData> ReadSuccessDataAsync<TData>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        // Check for non-success status codes first
        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponseAsync(response, cancellationToken);
        }

        ApiResponse<TData>? envelope;
        try
        {
            envelope = await response.Content.ReadFromJsonAsync<ApiResponse<TData>>(
                ApiJsonOptions.Default,
                cancellationToken);
        }
        catch (System.Text.Json.JsonException)
        {
            throw new ApiException(
                code: "CLIENT_INVALID_JSON",
                message: "Response is not valid JSON.",
                statusCode: response.StatusCode);
        }

        if (envelope is null)
        {
            throw new ApiException(
                code: "CLIENT_INVALID_RESPONSE",
                message: "Invalid response payload.",
                statusCode: response.StatusCode);
        }

        if (!envelope.Success)
        {
            ThrowApiException(response.StatusCode, envelope.Error);
        }

        if (envelope.Data is null)
        {
            throw new ApiException(
                code: "CLIENT_EMPTY_DATA",
                message: "Response data is empty.",
                statusCode: response.StatusCode);
        }

        return envelope.Data;
    }

    public static async Task ReadSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        // Check for non-success status codes first
        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponseAsync(response, cancellationToken);
        }

        ApiResponse? envelope;
        try
        {
            envelope = await response.Content.ReadFromJsonAsync<ApiResponse>(
                ApiJsonOptions.Default,
                cancellationToken);
        }
        catch (System.Text.Json.JsonException)
        {
            throw new ApiException(
                code: "CLIENT_INVALID_JSON",
                message: "Response is not valid JSON.",
                statusCode: response.StatusCode);
        }

        if (envelope is null)
        {
            throw new ApiException(
                code: "CLIENT_INVALID_RESPONSE",
                message: "Invalid response payload.",
                statusCode: response.StatusCode);
        }

        if (!envelope.Success)
        {
            ThrowApiException(response.StatusCode, envelope.Error);
        }
    }

    public static async Task<PagedApiResponse<TData>> ReadPagedSuccessAsync<TData>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        // Check for non-success status codes first
        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponseAsync(response, cancellationToken);
        }

        PagedApiResponse<TData>? envelope;
        try
        {
            envelope = await response.Content.ReadFromJsonAsync<PagedApiResponse<TData>>(
                ApiJsonOptions.Default,
                cancellationToken);
        }
        catch (System.Text.Json.JsonException)
        {
            throw new ApiException(
                code: "CLIENT_INVALID_JSON",
                message: "Response is not valid JSON.",
                statusCode: response.StatusCode);
        }

        if (envelope is null)
        {
            throw new ApiException(
                code: "CLIENT_INVALID_RESPONSE",
                message: "Invalid response payload.",
                statusCode: response.StatusCode);
        }

        if (!envelope.Success)
        {
            ThrowApiException(response.StatusCode, envelope.Error);
        }

        return envelope;
    }

    private static async Task HandleErrorResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        // Try to read as API error envelope
        try
        {
            var errorEnvelope = await response.Content.ReadFromJsonAsync<ApiResponse>(
                ApiJsonOptions.Default,
                cancellationToken);

            if (errorEnvelope?.Error is not null)
            {
                ThrowApiException(response.StatusCode, errorEnvelope.Error);
            }
        }
        catch (System.Text.Json.JsonException)
        {
            // Not JSON, provide generic error message based on status code
        }

        // Fallback error message based on status code
        var message = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Authentication required. Please log in.",
            HttpStatusCode.Forbidden => "You do not have permission to access this resource.",
            HttpStatusCode.NotFound => "The requested resource was not found.",
            HttpStatusCode.BadRequest => "Invalid request.",
            _ => $"Request failed with status {(int)response.StatusCode}."
        };

        throw new ApiException(
            code: "HTTP_" + response.StatusCode.ToString().ToUpperInvariant(),
            message: message,
            statusCode: response.StatusCode);
    }

    private static void ThrowApiException(HttpStatusCode statusCode, ApiError? error)
    {
        var validationErrors = error?.Details?
            .Select(detail => $"{detail.Field}: {detail.Message}")
            .ToList();

        throw new ApiException(
            code: error?.Code ?? "CLIENT_UNKNOWN_ERROR",
            message: error?.Message ?? "Unknown API error.",
            statusCode: statusCode,
            validationErrors: validationErrors);
    }
}
