using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.CredentialAccounts;

namespace ThaiX.Application.Features.CredentialAccounts.Commands.CreateCredentialAccount;

public sealed class CreateCredentialAccountCommandHandler : IRequestHandler<CreateCredentialAccountCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICredentialPasswordEncryptionService _encryptionService;
    private readonly ICurrentUserService _currentUser;

    public CreateCredentialAccountCommandHandler(
        IApplicationDbContext context,
        ICredentialPasswordEncryptionService encryptionService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _encryptionService = encryptionService;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateCredentialAccountCommand request, CancellationToken cancellationToken)
    {
        var normalizedUsername = request.Username.Trim();
        var exists = await _context.CredentialAccounts
            .AnyAsync(x => x.Username == normalizedUsername, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Credential account username already exists.");
        }

        var credentialAccount = CredentialAccount.Create(
            normalizedUsername,
            _encryptionService.Encrypt(request.Password),
            request.Description);

        _context.CredentialAccounts.Add(credentialAccount);
        _context.CredentialAccountAudits.Add(CredentialAccountAudit.Create(
            credentialAccount.Id,
            CredentialAccountAuditAction.Created,
            _currentUser.UserId,
            _currentUser.UserName));

        await _context.SaveChangesAsync(cancellationToken);
        return credentialAccount.Id;
    }
}
