using System.Net.Http.Headers;
using System.Net.Http.Json;
using ThaiX.Client.Models.Blog;
using ThaiX.Client.Services.Api;

namespace ThaiX.Client.Services.Files;

public sealed class FileService : IFileService
{
    private const string BasePath = "api/files";
    private readonly HttpClient _httpClient;

    public FileService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UploadedFileDto> GetFileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"{BasePath}/{id}", cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<UploadedFileDto>(response, cancellationToken);
    }

    public async Task DeleteFileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{BasePath}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }

    public async Task<UploadedFileDto> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);

        using var form = new MultipartFormDataContent();
        form.Add(streamContent, "file", fileName);

        using var response = await _httpClient.PostAsync($"{BasePath}/upload", form, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        }

        var result = await response.Content.ReadFromJsonAsync<UploadedFileDto>(
            ApiJsonOptions.Default,
            cancellationToken);

        if (result is null)
        {
            throw new ApiException(
                code: "CLIENT_INVALID_RESPONSE",
                message: "Invalid file upload response.",
                statusCode: response.StatusCode);
        }

        return result;
    }
}
