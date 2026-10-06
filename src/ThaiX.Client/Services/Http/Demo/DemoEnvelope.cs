using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ThaiX.Client.Models.Api;
using ThaiX.Client.Services.Api;

namespace ThaiX.Client.Services.Http.Demo;

internal static class DemoEnvelope
{
    public static HttpResponseMessage Json(object body, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var json = JsonSerializer.Serialize(body, ApiJsonOptions.Default);
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    public static HttpResponseMessage SuccessData<T>(T data) =>
        Json(new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Metadata = new ApiMetadata { Message = "Demo mode" }
        });

    public static HttpResponseMessage Success() =>
        Json(new ApiResponse
        {
            Success = true,
            Metadata = new ApiMetadata { Message = "Demo mode" }
        });

    public static HttpResponseMessage Failure(
        string code,
        string message,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest) =>
        Json(
            new ApiResponse
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            },
            statusCode);

    public static HttpResponseMessage Paged<T>(IReadOnlyList<T> items, int pageNumber, int pageSize)
    {
        var total = items.Count;
        var page = Math.Max(1, pageNumber);
        // pageSize <= 0 (e.g. -1) means return the full set — used by some external-data clients.
        List<T> slice;
        int size;
        if (pageSize <= 0)
        {
            slice = items.ToList();
            size = total == 0 ? 1 : total;
        }
        else
        {
            size = pageSize;
            slice = items.Skip((page - 1) * size).Take(size).ToList();
        }

        var totalPages = (int)Math.Ceiling(total / (double)size);

        return Json(new PagedApiResponse<T>
        {
            Success = true,
            Data = slice,
            Metadata = new PagedMetadata
            {
                TotalCount = total,
                PageNumber = page,
                PageSize = size,
                TotalPages = Math.Max(1, totalPages),
                HasPrevious = page > 1,
                HasNext = page < totalPages,
                Message = "Demo mode"
            }
        });
    }

    public static HttpResponseMessage EmptyPaged(int pageNumber = 1, int pageSize = 10) =>
        Paged(Array.Empty<object>(), pageNumber, pageSize);

    public static HttpResponseMessage Text(string text, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(text, Encoding.UTF8)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("text/plain") }
            }
        };
    }
}
