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
    private static HttpResponseMessage HandleAi(string method) =>
        method == "POST"
            ? DemoEnvelope.SuccessData(new GenerateAiTextResponse
            {
                Provider = "demo",
                Model = "demo-model",
                Text = "Demo AI generated text for offline mode."
            })
            : DemoEnvelope.SuccessData(new { demo = true });

    private static HttpResponseMessage HandleBotCommands(string method) =>
        method == "POST"
            ? DemoEnvelope.SuccessData(new BotCommandExecutionDto
            {
                Status = "Succeeded",
                PlainText = "Demo bot command executed.",
                ExecutionId = Guid.NewGuid().ToString("N")
            })
            : DemoEnvelope.SuccessData(new { demo = true });

}
