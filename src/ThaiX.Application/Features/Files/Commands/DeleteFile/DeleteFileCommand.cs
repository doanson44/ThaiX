using MediatR;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Files.Commands.DeleteFile;

/// <summary>
/// Deletes a file from storage and removes the FileAttachment record.
/// </summary>
public sealed record DeleteFileCommand : IAppCommand<Unit>
{
    public Guid FileId { get; init; }
}
