using ThaiX.Client.Models.Slack;


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
