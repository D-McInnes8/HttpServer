using Application.Request;

namespace Tests.IntegrationTests.TestExtensions;

public static class HttpRequestMethodExtensions
{
    public static HttpMethod ToHttpMethod(this HttpRequestMethod requestMethod)
    {
        return requestMethod switch
        {
            HttpRequestMethod.GET => HttpMethod.Get,
            HttpRequestMethod.POST => HttpMethod.Post,
            HttpRequestMethod.PUT => HttpMethod.Put,
            HttpRequestMethod.DELETE => HttpMethod.Delete,
            HttpRequestMethod.PATCH => HttpMethod.Patch,
            HttpRequestMethod.HEAD => HttpMethod.Head,
            HttpRequestMethod.OPTIONS => HttpMethod.Options,
            HttpRequestMethod.TRACE => HttpMethod.Trace,
            HttpRequestMethod.CONNECT => HttpMethod.Connect,
            _ => throw new ArgumentOutOfRangeException(nameof(requestMethod), requestMethod, null)
        };
    }
}