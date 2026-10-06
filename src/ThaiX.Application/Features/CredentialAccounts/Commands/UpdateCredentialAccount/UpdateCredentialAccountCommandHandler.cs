using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.CredentialAccounts;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.UpdateCredentialAccount;

public sealed class UpdateCredentialAccountCommandHandler : IRequestHandler<UpdateCredentialAccountCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateCredentialAccountCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(UpdateCredentialAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.CredentialAccounts
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (account is null)
        {
            throw new InvalidOperationException("Credential account not found.");
        }

        var normalizedUsername = request.Username.Trim();
        var usernameExists = await _context.CredentialAccounts
            .AnyAsync(x => x.Id != request.Id && x.Username == normalizedUsername, cancellationToken);

        if (usernameExists)
        {
            throw new InvalidOperationException("Credential account username already exists.");
        }

        account.UpdateInformation(normalizedUsername, request.Description);
        _context.CredentialAccountAudits.Add(CredentialAccountAudit.Create(
            account.Id,
            CredentialAccountAuditAction.Updated,
            _currentUser.UserId,
            _currentUser.UserName));

        await _context.SaveChangesAsync(cancellationToken);

        return request.Id;
    }
}
