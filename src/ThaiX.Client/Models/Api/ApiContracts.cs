namespace ThaiX.Client.Models.Api;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }
    public ApiMetadata Metadata { get; init; } = new();
}

public sealed class ApiResponse
{
    public bool Success { get; init; }
    public ApiError? Error { get; init; }
    public ApiMetadata Metadata { get; init; } = new();
}

public sealed class PagedApiResponse<T>
{
    public bool Success { get; init; }
    public List<T>? Data { get; init; }
    public ApiError? Error { get; init; }
    public PagedMetadata Metadata { get; init; } = new();
}

public sealed class ApiError
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public List<ValidationErrorDetail>? Details { get; init; }
}

public sealed class ValidationErrorDetail
{
    public required string Field { get; init; }
    public required string Message { get; init; }
    public string? Code { get; init; }
}

public class ApiMetadata
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }
    public string? Message { get; init; }
    public string Version { get; init; } = "1.0";
}

public sealed class PagedMetadata : ApiMetadata
{
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPrevious { get; init; }
    public bool HasNext { get; init; }
}
