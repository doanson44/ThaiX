using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.CredentialAccounts;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.ChangeCredentialAccountPassword;

public sealed class ChangeCredentialAccountPasswordCommandHandler : IRequestHandler<ChangeCredentialAccountPasswordCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICredentialPasswordEncryptionService _encryptionService;
    private readonly ICurrentUserService _currentUser;

    public ChangeCredentialAccountPasswordCommandHandler(
        IApplicationDbContext context,
        ICredentialPasswordEncryptionService encryptionService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _encryptionService = encryptionService;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(ChangeCredentialAccountPasswordCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.CredentialAccounts
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (account is null)
        {
            throw new InvalidOperationException("Credential account not found.");
        }

        account.UpdatePassword(_encryptionService.Encrypt(request.Password));
        _context.CredentialAccountAudits.Add(CredentialAccountAudit.Create(
            account.Id,
            CredentialAccountAuditAction.PasswordChanged,
            _currentUser.UserId,
            _currentUser.UserName));

        await _context.SaveChangesAsync(cancellationToken);

        return account.Id;
    }
}
