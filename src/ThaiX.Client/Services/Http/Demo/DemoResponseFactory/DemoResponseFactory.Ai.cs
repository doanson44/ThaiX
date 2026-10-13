using ThaiX.Client.Models.Ai;
using ThaiX.Client.Models.Slack;


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
