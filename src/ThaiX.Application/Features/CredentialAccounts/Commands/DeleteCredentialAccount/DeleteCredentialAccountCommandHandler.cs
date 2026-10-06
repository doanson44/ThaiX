using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.CredentialAccounts;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.DeleteCredentialAccount;

public sealed class DeleteCredentialAccountCommandHandler : IRequestHandler<DeleteCredentialAccountCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteCredentialAccountCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(DeleteCredentialAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.CredentialAccounts
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (account is null)
        {
            throw new InvalidOperationException("Credential account not found.");
        }

        account.SoftDelete();
        _context.CredentialAccountAudits.Add(CredentialAccountAudit.Create(
            account.Id,
            CredentialAccountAuditAction.Deleted,
            _currentUser.UserId,
            _currentUser.UserName));

        await _context.SaveChangesAsync(cancellationToken);

        return account.Id;
    }
}
