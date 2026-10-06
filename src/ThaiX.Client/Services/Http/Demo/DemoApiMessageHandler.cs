namespace ThaiX.Client.Services.Http.Demo;

/// <summary>
/// Short-circuits all HTTP calls with demo envelopes — never touches the network.
/// </summary>
public sealed class DemoApiMessageHandler : HttpMessageHandler
{
    private readonly DemoResponseFactory _factory;

    public DemoApiMessageHandler(DemoResponseFactory factory)
    {
        _factory = factory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await _factory.CreateAsync(request, cancellationToken);
        response.RequestMessage = request;
        return response;
    }
}
