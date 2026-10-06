using ThaiX.Application.Common.Constants;

namespace ThaiX.Presentation.Models;

/// <summary>
/// Unified API response envelope for all endpoints.
/// Provides consistent structure for success and error responses.
/// </summary>
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }
    public ApiMetadata Metadata { get; init; } = new();

    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    public static ApiResponse<T> SuccessResult(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Error = null,
            Metadata = new ApiMetadata
            {
                Message = message,
                Timestamp = DateTime.UtcNow
            }
        };
    }

    /// <summary>
    /// Creates an error response without validation details.
    /// </summary>
    public static ApiResponse<T> ErrorResult(string code, string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Error = new ApiError
            {
                Code = code,
                Message = message,
                Details = null
            },
            Metadata = new ApiMetadata
            {
                Timestamp = DateTime.UtcNow
            }
        };
    }

    /// <summary>
    /// Creates a validation error response with field-level details.
    /// </summary>
    public static ApiResponse<T> ValidationErrorResult(string message, List<ValidationErrorDetail> details)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Error = new ApiError
            {
                Code = ErrorCodes.VALIDATION_FAILED,
                Message = message,
                Details = details
            },
            Metadata = new ApiMetadata
            {
                Timestamp = DateTime.UtcNow
            }
        };
    }
}

/// <summary>
/// Non-generic version for operations that don't return data.
/// </summary>
public sealed class ApiResponse
{
    public bool Success { get; init; }
    public ApiError? Error { get; init; }
    public ApiMetadata Metadata { get; init; } = new();

    public static ApiResponse SuccessResult(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Error = null,
            Metadata = new ApiMetadata
            {
                Message = message,
                Timestamp = DateTime.UtcNow
            }
        };
    }

    public static ApiResponse ErrorResult(string code, string message)
    {
        return new ApiResponse
        {
            Success = false,
            Error = new ApiError
            {
                Code = code,
                Message = message,
                Details = null
            },
            Metadata = new ApiMetadata
            {
                Timestamp = DateTime.UtcNow
            }
        };
    }

    public static ApiResponse ValidationErrorResult(string message, List<ValidationErrorDetail> details)
    {
        return new ApiResponse
        {
            Success = false,
            Error = new ApiError
            {
                Code = ErrorCodes.VALIDATION_FAILED,
                Message = message,
                Details = details
            },
            Metadata = new ApiMetadata
            {
                Timestamp = DateTime.UtcNow
            }
        };
    }
}

/// <summary>
/// Error details for API responses.
/// </summary>
public sealed class ApiError
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public List<ValidationErrorDetail>? Details { get; init; }
}

/// <summary>
/// Field-level validation error detail.
/// </summary>
public sealed class ValidationErrorDetail
{
    public required string Field { get; init; }
    public required string Message { get; init; }
    public string? Code { get; init; }
}

/// <summary>
/// Metadata included in all API responses.
/// </summary>
public class ApiMetadata
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
    public string? Message { get; init; }
    public string Version { get; init; } = "1.0";
}

/// <summary>
/// Paged API response for list operations.
/// </summary>
public sealed class PagedApiResponse<T>
{
    public bool Success { get; init; }
    public List<T>? Data { get; init; }
    public ApiError? Error { get; init; }
    public PagedMetadata Metadata { get; init; } = new();

    public static PagedApiResponse<T> SuccessResult(
        List<T> data,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        return new PagedApiResponse<T>
        {
            Success = true,
            Data = data,
            Error = null,
            Metadata = new PagedMetadata
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                HasPrevious = pageNumber > 1,
                HasNext = pageNumber < (int)Math.Ceiling(totalCount / (double)pageSize),
                Timestamp = DateTime.UtcNow
            }
        };
    }

    public static PagedApiResponse<T> ErrorResult(string code, string message)
    {
        return new PagedApiResponse<T>
        {
            Success = false,
            Data = null,
            Error = new ApiError
            {
                Code = code,
                Message = message,
                Details = null
            },
            Metadata = new PagedMetadata
            {
                Timestamp = DateTime.UtcNow
            }
        };
    }
}

/// <summary>
/// Metadata for paged responses.
/// </summary>
public sealed class PagedMetadata : ApiMetadata
{
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPrevious { get; init; }
    public bool HasNext { get; init; }
}
