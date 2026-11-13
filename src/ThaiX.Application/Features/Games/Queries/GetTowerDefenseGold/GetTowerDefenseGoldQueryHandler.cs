using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Games.Queries.GetTowerDefenseGold;

public sealed class GetTowerDefenseGoldQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetTowerDefenseGoldQuery, int?>
{
    public async Task<int?> Handle(
        GetTowerDefenseGoldQuery request,
        CancellationToken cancellationToken)
    {
        var code = BuildCode(request.UserId);
        var content = await context.JsonBins
            .AsNoTracking()
            .Where(item => item.Code == code)
            .Select(item => item.Content)
            .FirstOrDefaultAsync(cancellationToken);

        if (content is null || content.Length == 0)
            return null;

        return int.TryParse(Encoding.UTF8.GetString(content), out var gold) && gold >= 0
            ? gold
            : throw new InvalidOperationException("Stored tower defense gold is invalid.");
    }

    internal static string BuildCode(Guid userId) => $"GAMES_TD_GOLD_{userId:N}".ToUpperInvariant();
}
