using System.Net;
using HttpServer;
using HttpServer.Request;
using HttpServer.Response;
using HttpServer.Routing;

namespace Tests.IntegrationTests.TestExtensions;

/// <summary>
/// Contains <see cref="IHttpWebServer"/> extensions methods for capturing requests.
/// </summary>
public static class HttpWebServerExtensions
{
    /// <summary>
    /// Sends a GET request to the server and captures the request.
    /// </summary>
    /// <param name="server">The <see cref="IHttpWebServer"/> to send the request to.</param>
    /// <param name="route">The route to send the request to.</param>
    /// <returns>The captured <see cref="HttpRequest"/>.</returns>
    public static async Task<HttpRequest> GetAsyncAndCaptureRequest(this IHttpWebServer server, string route)
    {
        HttpRequest? request = null;
        server.MapGet(route, ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });

        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri($"http://localhost:{server.Port}");
        _ = await httpClient.GetAsync(route);

        Assert.NotNull(request);
        return request;
    }

    /// <summary>
    /// Sends a POST request to the server and captures the request.
    /// </summary>
    /// <param name="server">The <see cref="IHttpWebServer"/> to send the request to.</param>
    /// <param name="route">The route to send the request to.</param>
    /// <param name="content">The <see cref="HttpContent"/> to send with the request.</param>
    /// <returns>The captured <see cref="HttpRequest"/>.</returns>
    public static async Task<HttpRequest> PostAsyncAndCaptureRequest(this IHttpWebServer server, string route, HttpContent content)
    {
        HttpRequest? request = null;
        server.MapPost(route, ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri($"http://localhost:{server.Port}");
        using var response = await httpClient.PostAsync(route, content);
        
        Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(request);
        return request;
    }
}