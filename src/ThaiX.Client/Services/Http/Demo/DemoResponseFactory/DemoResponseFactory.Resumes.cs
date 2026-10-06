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
    private HttpResponseMessage HandleResumes(string method, string path)
    {
        if (path.Contains("/ai/polish-text", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/ai/generate-summary", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(new ResumeAiTextResponse { Text = "Demo polished resume text." });
        }

        if (path.Contains("/ai/optimize-for-job", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/ai/check-consistency", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(new ResumeAiReportResponse { Report = "Demo resume AI report: looks consistent." });
        }

        if (method == "GET" &&
            (path.EndsWith("/mine", StringComparison.OrdinalIgnoreCase) ||
             path.Contains("/public/", StringComparison.OrdinalIgnoreCase)))
        {
            return _store.WithLock(() => DemoEnvelope.SuccessData(_store.Resume));
        }

        if (method == "PUT")
        {
            return DemoEnvelope.SuccessData(_store.Resume.Id);
        }

        return DemoEnvelope.SuccessData(_store.Resume);
    }

}
