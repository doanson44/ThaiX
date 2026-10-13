using ThaiX.Client.Models.Resumes;


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
