using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.Notes;

namespace ThaiX.Client.Services.Notes;

public interface INoteService
{
    Task<PagedApiResponse<NoteDto>> GetListAsync(
        NotesListRequest request,
        CancellationToken cancellationToken = default);

    Task<NoteDto> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        CreateNoteRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid id,
        UpdateNoteRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task TogglePinAsync(Guid id, CancellationToken cancellationToken = default);

    Task ToggleArchiveAsync(Guid id, CancellationToken cancellationToken = default);
}
