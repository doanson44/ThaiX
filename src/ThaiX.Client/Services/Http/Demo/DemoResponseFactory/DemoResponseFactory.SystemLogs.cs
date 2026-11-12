using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ThaiX.Client.Models.SystemLogs;

namespace ThaiX.Client.Services.Http.Demo;

public sealed partial class DemoResponseFactory
{
    private static readonly string DemoLogFileName = "thaix-demo.log";

    private HttpResponseMessage HandleSystemLogs(string method, string path)
    {
        if (method != "GET")
        {
            return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 2)
        {
            var now = DateTime.UtcNow;
            return DemoEnvelope.Json(new[]
            {
                new SystemLogFileDto(DemoLogFileName, DateOnly.FromDateTime(now), 82, now)
            });
        }

        if (segments.Length == 3 && segments[2].Equals(DemoLogFileName, StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.Json(new SystemLogFilePreviewDto(
                DemoLogFileName,
                "ThaiX demo log\nVisual test environment is running in demo mode.\n",
                false));
        }

        if (segments.Length == 4 &&
            segments[2].Equals(DemoLogFileName, StringComparison.OrdinalIgnoreCase) &&
            segments[3].Equals("download", StringComparison.OrdinalIgnoreCase))
        {
            var content = Encoding.UTF8.GetBytes(
                "ThaiX demo log\nVisual test environment is running in demo mode.\n");
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(content)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain")
            {
                CharSet = "utf-8"
            };
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = DemoLogFileName
            };
            return response;
        }

        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }
}
