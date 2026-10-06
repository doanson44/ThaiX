using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.JsonBins.Commands.DeleteJsonBin;

public sealed class DeleteJsonBinCommandValidator : AbstractValidator<DeleteJsonBinCommand>
{
    public DeleteJsonBinCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
