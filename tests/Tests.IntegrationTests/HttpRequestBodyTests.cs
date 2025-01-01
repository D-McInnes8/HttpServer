using HttpServer;
using HttpServer.Pipeline.Endpoints;
using HttpServer.Request;
using HttpServer.Response;
using NSubstitute;

namespace Tests.IntegrationTests;

public class HttpRequestBodyTests : IAsyncLifetime
{
    private readonly HttpWebWebServer _httpWebWebServer = HttpWebWebServer.CreateBuilder(9997).Build();
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task InitializeAsync()
    {
        _httpClient.BaseAddress = new Uri($"http://localhost:{_httpWebWebServer.Port}");
        await _httpWebWebServer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        await _httpWebWebServer.StopAsync();
    }
    
    [Fact]
    public async Task HttpRequestBody_RequestWithPlainTextBody_ShouldParseBody()
    {
        // Arrange
        HttpRequest? actual = null;
        _httpWebWebServer.AddEndpointPipeline(options =>
        {
            options.MapRoute(HttpRequestMethod.POST, "/test", request =>
            {
                actual = request;
                return HttpResponse.Ok();
            });
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Post, "/test")
        {
            Content = new StringContent("Hello, World!")
        };
        _ = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.NotNull(actual);
        Assert.Multiple(() =>
        {
            Assert.Equal("Hello, World!", actual.Body);
            Assert.Equal("text/plain; charset=utf-8", actual.ContentType);
        });
    }
}