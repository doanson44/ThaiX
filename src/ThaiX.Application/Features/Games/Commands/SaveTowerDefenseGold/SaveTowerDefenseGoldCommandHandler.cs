using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Games.Queries.GetTowerDefenseGold;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Features.Games.Commands.SaveTowerDefenseGold;

public sealed class SaveTowerDefenseGoldCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SaveTowerDefenseGoldCommand>
{
    public async Task Handle(
        SaveTowerDefenseGoldCommand request,
        CancellationToken cancellationToken)
    {
        using var gate = await AsyncLock.GetLockByKey(
            $"games:tower-defense-gold:{request.UserId:N}").LockAsync(cancellationToken);

        var code = GetTowerDefenseGoldQueryHandler.BuildCode(request.UserId);
        var bin = await context.JsonBins
            .FirstOrDefaultAsync(item => item.Code == code, cancellationToken);
        var content = Encoding.UTF8.GetBytes(request.Gold.ToString(System.Globalization.CultureInfo.InvariantCulture));

        if (bin is null)
        {
            bin = JsonBin.Create(
                code,
                "Tower Defense player gold",
                JsonBinCategories.Config,
                content,
                tags: "games,tower-defense,gold",
                referenceId: request.UserId);
            context.JsonBins.Add(bin);
        }
        else
        {
            bin.ReplaceContent(content, "application/json", isCompressed: false);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
