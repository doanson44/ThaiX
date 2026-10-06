using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.CredentialAccounts;

namespace ThaiX.Application.Features.CredentialAccounts.Queries.GetCredentialAccountPassword;

public sealed class GetCredentialAccountPasswordQueryHandler
    : IRequestHandler<GetCredentialAccountPasswordQuery, CredentialAccountPasswordDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICredentialPasswordEncryptionService _encryptionService;
    private readonly ICurrentUserService _currentUser;

    public GetCredentialAccountPasswordQueryHandler(
        IApplicationDbContext context,
        ICredentialPasswordEncryptionService encryptionService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _encryptionService = encryptionService;
        _currentUser = currentUser;
    }

    public async Task<CredentialAccountPasswordDto?> Handle(
        GetCredentialAccountPasswordQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _context.CredentialAccounts
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new
            {
                x.Id,
                x.Username,
                x.PasswordEncrypted
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return null;
        }

        _context.CredentialAccountAudits.Add(CredentialAccountAudit.Create(
            result.Id,
            request.AuditAction,
            _currentUser.UserId,
            _currentUser.UserName));

        await _context.SaveChangesAsync(cancellationToken);

        return new CredentialAccountPasswordDto
        {
            Id = result.Id,
            Username = result.Username,
            Password = _encryptionService.Decrypt(result.PasswordEncrypted)
        };
    }
}
