using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Games.Queries.GetTowerDefenseProgress;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Features.Games.Commands.SaveTowerDefenseProgress;

public sealed class SaveTowerDefenseProgressCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SaveTowerDefenseProgressCommand>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task Handle(
        SaveTowerDefenseProgressCommand request,
        CancellationToken cancellationToken)
    {
        using var gate = await AsyncLock.GetLockByKey(
            $"games:tower-defense-progress:{request.UserId:N}").LockAsync(cancellationToken);

        var code = GetTowerDefenseProgressQueryHandler.BuildCode(request.UserId);
        var bin = await context.JsonBins
            .FirstOrDefaultAsync(item => item.Code == code, cancellationToken);
        var content = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request.Progress, JsonOptions));

        if (bin is null)
        {
            bin = JsonBin.Create(
                code,
                "Tower Defense player progress",
                JsonBinCategories.Config,
                content,
                tags: "games,tower-defense,progress",
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
