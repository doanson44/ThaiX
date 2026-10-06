using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.JsonBins.Commands.ExpireJsonBin;

public sealed class ExpireJsonBinCommandValidator : AbstractValidator<ExpireJsonBinCommand>
{
    public ExpireJsonBinCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
