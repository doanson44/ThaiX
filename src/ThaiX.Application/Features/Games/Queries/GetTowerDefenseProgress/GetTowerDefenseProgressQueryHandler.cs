using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Games.Queries.GetTowerDefenseProgress;

public sealed class GetTowerDefenseProgressQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetTowerDefenseProgressQuery, TowerDefenseProgressDto?>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<TowerDefenseProgressDto?> Handle(
        GetTowerDefenseProgressQuery request,
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

        return JsonSerializer.Deserialize<TowerDefenseProgressDto>(
            Encoding.UTF8.GetString(content), JsonOptions)
            ?? throw new InvalidOperationException("Stored tower defense progress is invalid.");
    }

    internal static string BuildCode(Guid userId) => $"GAMES_TD_PROGRESS_{userId:N}".ToUpperInvariant();
}
