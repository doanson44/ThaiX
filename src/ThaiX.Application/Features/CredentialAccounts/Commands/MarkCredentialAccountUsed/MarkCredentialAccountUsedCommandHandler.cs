using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.CredentialAccounts;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.MarkCredentialAccountUsed;

public sealed class MarkCredentialAccountUsedCommandHandler : IRequestHandler<MarkCredentialAccountUsedCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public MarkCredentialAccountUsedCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Guid> Handle(MarkCredentialAccountUsedCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.CredentialAccounts
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (account is null)
        {
            throw new InvalidOperationException("Credential account not found.");
        }

        account.MarkUsed(_currentUser.UserName, _dateTimeProvider.UtcNow);
        _context.CredentialAccountAudits.Add(CredentialAccountAudit.Create(
            account.Id,
            CredentialAccountAuditAction.MarkedUsed,
            _currentUser.UserId,
            _currentUser.UserName));

        await _context.SaveChangesAsync(cancellationToken);

        return account.Id;
    }
}
