using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using ThaiX.Client.Models.PublicHtml;

namespace ThaiX.Client.Services.PublicHtml;

public sealed class PublicHtmlPageService(HttpClient http) : IPublicHtmlPageService
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    public async Task<IReadOnlyList<PublicHtmlPageModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync("api/public-html", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<PublicHtmlPageModel>>(cancellationToken)
            ?? [];
    }

    public async Task UploadAsync(
        IBrowserFile file,
        string title,
        string slug,
        CancellationToken cancellationToken = default)
    {
        using var fileContent = new StreamContent(file.OpenReadStream(MaxFileSize));
        using var form = new MultipartFormDataContent();
        form.Add(fileContent, "file", file.Name);
        form.Add(new StringContent(title), "title");
        form.Add(new StringContent(slug), "slug");

        using var response = await http.PostAsync("api/public-html", form, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RenameAsync(Guid id, string slug, CancellationToken cancellationToken = default)
    {
        using var response = await http.PatchAsJsonAsync(
            $"api/public-html/{id}/slug",
            new { slug },
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetPublishedAsync(Guid id, bool isPublished, CancellationToken cancellationToken = default)
    {
        using var response = await http.PutAsJsonAsync(
            $"api/public-html/{id}/publish",
            new { isPublished },
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/public-html/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
