namespace ThaiX.Application.Common.Interfaces;

/// <summary>
/// Builds public URLs for stored files. Implementation uses storage base URL (e.g. /files).
/// </summary>
public interface IFileUrlProvider
{
    /// <summary>
    /// Returns the public URL for the given storage key.
    /// </summary>
    string GetUrl(string storageKey);
}
