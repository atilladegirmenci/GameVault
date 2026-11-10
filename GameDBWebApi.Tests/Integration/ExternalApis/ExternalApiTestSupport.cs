using System.Net;
using System.Text;

namespace GameDBWebApi.Tests.Integration.ExternalApis;

internal static class ExternalApiTestSupport
{
    public static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
}

internal sealed class StubHttpMessageHandler(
    Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }
    public int CallCount { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        CallCount++;
        return await handler(request, cancellationToken);
    }
}
