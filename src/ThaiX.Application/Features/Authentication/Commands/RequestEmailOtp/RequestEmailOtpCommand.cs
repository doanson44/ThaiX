using MediatR;

namespace ThaiX.Application.Features.Authentication.Commands.RequestEmailOtp;

public sealed record RequestEmailOtpCommand(string Email) : IRequest<Guid>;
