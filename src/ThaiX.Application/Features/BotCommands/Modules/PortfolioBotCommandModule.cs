using MediatR;
using System.Globalization;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.AssetPositions.Queries.GetCryptoPositions;
using ThaiX.Application.Features.AssetPositions.Queries.GetStockPositions;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.Portfolios.Queries.GetPortfolios;
using ThaiX.Domain.Common.Constants;

namespace ThaiX.Application.Features.BotCommands.Modules;

public sealed class PortfolioBotCommandModule : IBotCommandModule
{
    private static readonly BotCommandMetadata MetadataValue = new()
    {
        Name = "pf",
        Aliases = ["portfolio", "portfolio-summary"],
        Syntax = "/pf",
        Description = "Show a portfolio summary for the current user.",
        Category = BotCommandCategory.Portfolio,
        RequiredPermissions = [Permissions.PortfolioRead],
        SupportsAsync = false
    };

    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public PortfolioBotCommandModule(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public BotCommandMetadata Metadata => MetadataValue;

    public async Task<BotCommandExecutionDto> ExecuteAsync(BotCommandContext context, CancellationToken cancellationToken)
    {
        var portfolios = await _mediator.Send(new GetPortfoliosQuery
        {
            OwnerId = _currentUser.UserId,
            PageNumber = 1,
            PageSize = 10,
            SortBy = "name",
            SortDescending = false
        }, cancellationToken);

        if (portfolios.TotalCount == 0)
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Completed,
                PlainText = "No portfolios found for this account."
            };
        }

        var canReadPositions = _currentUser.HasPermission(Permissions.PositionRead);
        var rows = new List<IReadOnlyList<string>>();

        foreach (var portfolio in portfolios.Items.Take(5))
        {
            if (!canReadPositions)
            {
                rows.Add([
                    portfolio.Name,
                    portfolio.PortfolioType.ToString(),
                    "n/a",
                    "n/a",
                    "n/a",
                    "n/a"
                ]);
                continue;
            }

            var crypto = await _mediator.Send(new GetCryptoPositionsQuery
            {
                PortfolioId = portfolio.Id,
                IsClosed = false,
                PageNumber = 1,
                PageSize = 100
            }, cancellationToken);

            var stock = await _mediator.Send(new GetStockPositionsQuery
            {
                PortfolioId = portfolio.Id,
                IsClosed = false,
                PageNumber = 1,
                PageSize = 100
            }, cancellationToken);

            var totalInvested = crypto.Items.Sum(x => x.TotalInvested) + stock.Items.Sum(x => x.TotalInvested);
            var totalRealized = crypto.Items.Sum(x => x.RealizedPnl) + stock.Items.Sum(x => x.RealizedPnl);

            rows.Add([
                portfolio.Name,
                portfolio.PortfolioType.ToString(),
                crypto.TotalCount.ToString(CultureInfo.InvariantCulture),
                stock.TotalCount.ToString(CultureInfo.InvariantCulture),
                totalInvested.ToString("0.##", CultureInfo.InvariantCulture),
                totalRealized.ToString("0.##", CultureInfo.InvariantCulture)
            ]);
        }

        var plainText = canReadPositions
            ? $"Found {portfolios.TotalCount} portfolio(s). Showing up to 5 with open positions summary."
            : $"Found {portfolios.TotalCount} portfolio(s). Position details require Position.Read permission.";

        return new BotCommandExecutionDto
        {
            Status = BotCommandExecutionStatus.Completed,
            PlainText = plainText,
            Card = new NotificationCard
            {
                Category = "Portfolio",
                Title = "Portfolio Summary",
                Type = NotificationCardType.Summary,
                Metrics = rows.Select((row, i) =>
                {
                    var name = i < portfolios.Items.Count ? portfolios.Items[i].Name : $"#{i + 1}";
                    var invested = row.Count > 4 ? row[4] : "n/a";
                    var realized = row.Count > 5 ? row[5] : "n/a";
                    var openCrypto = row.Count > 2 ? row[2] : "n/a";
                    var openStock = row.Count > 3 ? row[3] : "n/a";
                    return new NotificationMetric
                    {
                        Label = $"{name} [{((row.Count > 1) ? row[1] : "n/a")}]",
                        Value = $"Crypto:{openCrypto} Stock:{openStock} Invested:{invested} PnL:{realized}"
                    };
                }).ToList()
            }
        };
    }
}
