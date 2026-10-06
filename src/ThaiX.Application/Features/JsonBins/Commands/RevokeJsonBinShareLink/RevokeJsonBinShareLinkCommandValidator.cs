using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.JsonBins.Commands.RevokeJsonBinShareLink;

public sealed class RevokeJsonBinShareLinkCommandValidator
    : AbstractValidator<RevokeJsonBinShareLinkCommand>
{
    public RevokeJsonBinShareLinkCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
