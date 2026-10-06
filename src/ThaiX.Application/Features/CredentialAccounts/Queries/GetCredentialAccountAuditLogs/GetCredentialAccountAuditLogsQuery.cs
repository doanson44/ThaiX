using FluentValidation;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.CredentialAccounts.Queries.GetCredentialAccountAuditLogs;

public sealed record GetCredentialAccountAuditLogsQuery : PagedRequest, IAppQuery<PagedResult<CredentialAccountAuditDto>>
{
    public required Guid CredentialAccountId { get; init; }
}

public sealed record CredentialAccountAuditDto
{
    public required Guid Id { get; init; }

    public required Guid CredentialAccountId { get; init; }

    public required string Action { get; init; }

    public required Guid UserId { get; init; }

    public required string UserName { get; init; }

    public required DateTime CreatedAt { get; init; }
}

public sealed class GetCredentialAccountAuditLogsQueryValidator : AbstractValidator<GetCredentialAccountAuditLogsQuery>
{
    public GetCredentialAccountAuditLogsQueryValidator()
    {
        RuleFor(x => x.CredentialAccountId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
