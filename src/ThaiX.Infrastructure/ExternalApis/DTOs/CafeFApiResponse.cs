namespace ThaiX.Infrastructure.ExternalApis.DTOs;

/// <summary>
/// Base response wrapper from CafeF APIs.
/// </summary>
/// <typeparam name="T">Data type.</typeparam>
public sealed class CafeFApiResponse<T>
{
    public T? Data { get; set; }
    public string? Message { get; set; }
    public bool Success { get; set; }
}
