using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Common.Interfaces;

/// <summary>
/// Provides access to JsonBin payloads without exposing infrastructure details.
/// </summary>
public interface IJsonBinService
{
    Task<Guid> SaveAsync(string name, JsonBinCategories category, Stream content, bool compress, CancellationToken cancellationToken = default);

    Task<Stream> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
