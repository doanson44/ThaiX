using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Authentication.Commands.RequestEmailOtp;

public sealed record RequestEmailOtpCommand(string Email) : IRequest<EmailOtpChallengeInfo>;
