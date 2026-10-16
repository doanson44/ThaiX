using Microsoft.AspNetCore.Components.Forms;
using ThaiX.Client.Models.PublicHtml;

namespace ThaiX.Client.Services.PublicHtml;

public interface IPublicHtmlPageService
{
    Task<IReadOnlyList<PublicHtmlPageModel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task UploadAsync(
        IBrowserFile file,
        string title,
        string slug,
        CancellationToken cancellationToken = default);

    Task RenameAsync(Guid id, string slug, CancellationToken cancellationToken = default);

    Task SetPublishedAsync(Guid id, bool isPublished, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
