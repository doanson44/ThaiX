using FluentValidation;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.CredentialAccounts;

namespace ThaiX.Application.Features.CredentialAccounts.Queries.GetCredentialAccountPassword;

public sealed record GetCredentialAccountPasswordQuery : IAppQuery<CredentialAccountPasswordDto?>
{
    public required Guid Id { get; init; }

    public required CredentialAccountAuditAction AuditAction { get; init; }
}

public sealed record CredentialAccountPasswordDto
{
    public required Guid Id { get; init; }

    public required string Username { get; init; }

    public required string Password { get; init; }
}

public sealed class GetCredentialAccountPasswordQueryValidator : AbstractValidator<GetCredentialAccountPasswordQuery>
{
    public GetCredentialAccountPasswordQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.AuditAction)
            .Must(x => x is CredentialAccountAuditAction.PasswordViewed or CredentialAccountAuditAction.PasswordCopied)
            .WithErrorCode(ErrorCodes.INVALID_FORMAT);
    }
}
