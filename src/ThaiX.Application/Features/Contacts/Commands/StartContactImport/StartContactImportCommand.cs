using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.StartContactImport;

/// <summary>
/// Starts an asynchronous contact import. Creates a ContactImportJob (Pending) and enqueues processing.
/// Returns the job id for status polling.
/// </summary>
public sealed record StartContactImportCommand : IAppCommand<Guid>
{
    /// <summary>
    /// Full path to the saved CSV file (caller saves the upload to this path before sending the command).
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Batch size (optional, default 500).
    /// </summary>
    public int? BatchSize { get; init; }
}
