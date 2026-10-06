using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ThaiX.Client.Models.Ai;
using ThaiX.Client.Models.ApiClients;
using ThaiX.Client.Models.AssetPositions;
using ThaiX.Client.Models.Auth;
using ThaiX.Client.Models.Blog;
using ThaiX.Client.Models.Contacts;
using ThaiX.Client.Models.CredentialAccounts;
using ThaiX.Client.Models.ExpenseTracker;
using ThaiX.Client.Models.ExternalData;
using ThaiX.Client.Models.JsonBins;
using ThaiX.Client.Models.Lottery;
using ThaiX.Client.Models.MasterData;
using ThaiX.Client.Models.MarketScanner;
using ThaiX.Client.Models.Notes;
using ThaiX.Client.Models.Notifications;
using ThaiX.Client.Models.Portfolios;
using ThaiX.Client.Models.PriceAlerts;
using ThaiX.Client.Models.Resumes;
using ThaiX.Client.Models.Slack;
using ThaiX.Client.Models.Trading;
using ThaiX.Client.Models.Users;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Notifications;


namespace ThaiX.Client.Services.Http.Demo;

public sealed partial class DemoResponseFactory
{
    private HttpResponseMessage HandleSlackTelegram(string method, string path)
    {
        if (path.Contains("/channels", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            var channels = Enumerable.Range(1, DemoDataGenerators.DefaultListCount)
                .Select(i => new ChannelInfoDto { Id = $"C{i:D5}", Name = $"demo-channel-{i}" })
                .ToList();
            return DemoEnvelope.SuccessData((IReadOnlyList<ChannelInfoDto>)channels);
        }

        if (path.Contains("/execute", StringComparison.OrdinalIgnoreCase) && method == "POST")
        {
            return DemoEnvelope.SuccessData(new BotCommandExecutionDto
            {
                Status = "Succeeded",
                PlainText = "Demo Slack/Telegram command executed.",
                ExecutionId = Guid.NewGuid().ToString("N")
            });
        }

        if (path.Contains("/executions/", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            var executionId = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "demo";
            return DemoEnvelope.SuccessData(new BotAsyncExecutionStatusDto
            {
                ExecutionId = Uri.UnescapeDataString(executionId),
                Status = "Completed",
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5),
                CompletedAtUtc = DateTime.UtcNow,
                PlainText = "Demo execution completed."
            });
        }

        return method == "POST"
            ? DemoEnvelope.SuccessData(new { demo = true })
            : DemoEnvelope.SuccessData(new { demo = true, path });
    }

}
