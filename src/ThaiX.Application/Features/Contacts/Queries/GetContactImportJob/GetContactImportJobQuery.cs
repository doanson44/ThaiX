using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Queries.GetContactImportJob;

/// <summary>
/// Returns the status and result of an asynchronous contact import job.
/// </summary>
public sealed record GetContactImportJobQuery : IAppQuery<ContactImportJobDto?>
{
    public Guid JobId { get; init; }
}
