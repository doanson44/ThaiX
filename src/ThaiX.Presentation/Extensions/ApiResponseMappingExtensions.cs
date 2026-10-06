using ThaiX.Application.Common.Models;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Extensions;

/// <summary>
/// Extension methods for mapping Application layer results to Presentation layer API responses.
/// </summary>
public static class ApiResponseMappingExtensions
{
    /// <summary>
    /// Converts a PagedResult from Application layer to PagedApiResponse for API layer.
    /// Preserves pagination metadata and wraps in unified API response format.
    /// </summary>
    public static PagedApiResponse<T> ToPagedApiResponse<T>(
        this PagedResult<T> pagedResult,
        string? message = null)
    {
        return PagedApiResponse<T>.SuccessResult(
            pagedResult.Items.ToList(),
            pagedResult.TotalCount,
            pagedResult.PageNumber,
            pagedResult.PageSize);
    }

    /// <summary>
    /// Converts a PagedResult to PagedApiResponse with correlation ID.
    /// </summary>
    public static PagedApiResponse<T> ToPagedApiResponse<T>(
        this PagedResult<T> pagedResult,
        string correlationId,
        string? message = null)
    {
        var response = pagedResult.ToPagedApiResponse(message);
        response.Metadata.CorrelationId = correlationId;
        return response;
    }

    /// <summary>
    /// Gets correlation ID from HttpContext.
    /// </summary>
    public static string GetCorrelationId(this HttpContext context)
    {
        return context.Items.TryGetValue("CorrelationId", out var correlationId)
            ? correlationId?.ToString() ?? context.TraceIdentifier
            : context.TraceIdentifier;
    }
}
