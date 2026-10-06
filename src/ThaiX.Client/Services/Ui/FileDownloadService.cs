using Microsoft.JSInterop;

namespace ThaiX.Client.Services.Ui;

public sealed class FileDownloadService : IFileDownloadService
{
    private readonly IJSRuntime _jsRuntime;

    public FileDownloadService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public Task DownloadAsync(
        string fileName,
        string contentType,
        byte[] fileContent,
        CancellationToken cancellationToken = default)
    {
        var base64 = Convert.ToBase64String(fileContent);
        return _jsRuntime.InvokeVoidAsync(
            "downloadFileFromBytes",
            cancellationToken,
            fileName,
            contentType,
            base64).AsTask();
    }
}
