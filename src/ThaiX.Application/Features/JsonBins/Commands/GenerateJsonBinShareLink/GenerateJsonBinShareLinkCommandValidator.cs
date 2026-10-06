using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.JsonBins.Commands.GenerateJsonBinShareLink;

public sealed class GenerateJsonBinShareLinkCommandValidator
    : AbstractValidator<GenerateJsonBinShareLinkCommand>
{
    public GenerateJsonBinShareLinkCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.ShareExpiresAtUtc)
            .Must(dt => dt is null || dt > DateTime.UtcNow)
            .WithErrorCode(ErrorCodes.INVALID_RANGE)
            .WithMessage("ShareExpiresAtUtc must be in the future when provided.");
    }
}
