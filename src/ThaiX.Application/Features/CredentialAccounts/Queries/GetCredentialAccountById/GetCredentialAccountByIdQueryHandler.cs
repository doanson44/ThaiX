using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.CredentialAccounts.Queries.GetCredentialAccountById;

public sealed class GetCredentialAccountByIdQueryHandler
    : IRequestHandler<GetCredentialAccountByIdQuery, CredentialAccountDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetCredentialAccountByIdQueryHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<CredentialAccountDetailDto?> Handle(
        GetCredentialAccountByIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _context.CredentialAccounts
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new CredentialAccountDetailDto
            {
                Id = x.Id,
                Username = x.Username,
                Description = x.Description,
                IsUsed = x.IsUsed,
                UsedAt = x.UsedAt,
                UsedBy = x.UsedBy,
                UsageCount = x.UsageCount,
                LastUsedAgo = string.Empty,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return result is null
            ? null
            : result with { LastUsedAgo = CredentialAccountLastUsedFormatter.Format(result.UsedAt, _dateTimeProvider.UtcNow) };
    }
}
