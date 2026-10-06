using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.CredentialAccounts.Queries.GetCredentialAccounts;

public sealed class GetCredentialAccountsQueryValidator : AbstractValidator<GetCredentialAccountsQuery>
{
    public GetCredentialAccountsQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(200)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));
    }
}
