using System.Net.Http.Headers;
using HttpServer;
using HttpServer.Response;
using HttpServer.Routing;

namespace Tests.IntegrationTests;

public class HttpRequestCachingTests : IAsyncLifetime
{
    private readonly IHttpWebServer _server = HttpWebServer.CreateBuilder(0).Build();
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task InitializeAsync()
    {
        await _server.StartAsync();
        _httpClient.BaseAddress = new Uri($"http://localhost:{_server.Port}");
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        await _server.StopAsync();
    }

    [Fact]
    public async Task HttpRequestCachingTests_WithIfNoneMatch_ShouldReturnEmptyResponse()
    {
        // Arrange
        _server.MapGet("/cached-resource", ctx =>
        {
            // Simulate a cached response
            var eTag = ctx.Request.Headers["If-None-Match"];
            return eTag == "\"etag-value\""
                ? HttpResponse.NotModified("\"etag-value\"")
                : HttpResponse.Ok("Cached Content");
        });
        var request = new HttpRequestMessage(HttpMethod.Get, "/cached-resource");
        request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue("\"etag-value\""));
        
        // Act
        var response = await _httpClient.SendAsync(request);
        
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotModified, response.StatusCode);
        Assert.Empty(response.Content.Headers);
        Assert.Empty(response.Headers);
        Assert.Equal(0, response.Content.Headers.ContentLength);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
    }
}