namespace ThaiX.Client.Services.Ui;

public interface IFileDownloadService
{
    Task DownloadAsync(
        string fileName,
        string contentType,
        byte[] fileContent,
        CancellationToken cancellationToken = default);
}
