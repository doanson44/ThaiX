using MediatR;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.TcbsTop10.Queries.GetTcbsTop10Portfolios;

namespace ThaiX.Application.Features.BotCommands.Modules;

public sealed class TcbsBotCommandModule : IBotCommandModule
{
    private static readonly BotCommandMetadata MetadataValue = new()
    {
        Name = "tcbs",
        Aliases = ["tcbs-top10"],
        Syntax = "/tcbs",
        Description = "Show TCBS Top 10 current stock holdings.",
        Category = BotCommandCategory.MarketData,
        RequiredPermissions = [],
        SupportsAsync = false
    };

    private readonly IMediator _mediator;

    public TcbsBotCommandModule(IMediator mediator)
    {
        _mediator = mediator;
    }

    public BotCommandMetadata Metadata => MetadataValue;

    public async Task<BotCommandExecutionDto> ExecuteAsync(BotCommandContext context, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTcbsTop10PortfoliosQuery(), cancellationToken);

        if (result.CurrentHoldings.Count == 0)
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Completed,
                PlainText = "TCBS Top 10: no holdings data available."
            };
        }

        var plainText = $"TCBS Top 10 Holdings: {string.Join(", ", result.CurrentHoldings)}";

        var rows = result.CurrentHoldings
            .Select((ticker, i) => (IReadOnlyList<string>)new[] { (i + 1).ToString(), ticker })
            .ToList();

        return new BotCommandExecutionDto
        {
            Status = BotCommandExecutionStatus.Completed,
            PlainText = plainText,
            Card = new NotificationCard
            {
                Category = "Market Data",
                Title = "TCBS Top 10 Holdings",
                Type = NotificationCardType.Metric,
                Metrics = result.CurrentHoldings
                    .Select((ticker, i) => new NotificationMetric { Label = $"#{i + 1}", Value = ticker })
                    .ToList()
            }
        };
    }
}
