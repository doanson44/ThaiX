using MediatR;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Identity.Commands.UnlinkContactFromUser;

/// <summary>
/// Unlinks the contact currently linked to the given user. Removes UserContactLink and UserProfile.
/// </summary>
public sealed record UnlinkContactFromUserCommand(Guid UserId) : IAppCommand<Unit>;
