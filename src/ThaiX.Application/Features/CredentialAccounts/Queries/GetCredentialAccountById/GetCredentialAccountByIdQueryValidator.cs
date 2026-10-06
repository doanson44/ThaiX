using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.CredentialAccounts.Queries.GetCredentialAccountById;

public sealed class GetCredentialAccountByIdQueryValidator : AbstractValidator<GetCredentialAccountByIdQuery>
{
    public GetCredentialAccountByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
