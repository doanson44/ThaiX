using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.CredentialAccounts.Queries.GetCredentialAccountAuditLogs;

public sealed class GetCredentialAccountAuditLogsQueryHandler
    : IRequestHandler<GetCredentialAccountAuditLogsQuery, PagedResult<CredentialAccountAuditDto>>
{
    private readonly Common.Interfaces.IApplicationDbContext _context;

    public GetCredentialAccountAuditLogsQueryHandler(Common.Interfaces.IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<CredentialAccountAuditDto>> Handle(
        GetCredentialAccountAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.CredentialAccountAudits
            .AsNoTracking()
            .Where(x => x.CredentialAccountId == request.CredentialAccountId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new CredentialAccountAuditDto
            {
                Id = x.Id,
                CredentialAccountId = x.CredentialAccountId,
                Action = x.Action.ToString(),
                UserId = x.UserId,
                UserName = x.UserName,
                CreatedAt = x.CreatedAt
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
