using HttpServer;
using HttpServer.Request;
using HttpServer.Response;

namespace Tests.IntegrationTests;

public class HttpRequestBodyTests : IAsyncLifetime
{
    private readonly IHttpWebServer _server = HttpWebServer.CreateBuilder(9997).Build();
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task InitializeAsync()
    {
        _httpClient.BaseAddress = new Uri($"http://localhost:{_server.Port}");
        await _server.StartAsync();
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        await _server.StopAsync();
    }
    
    [Fact]
    public async Task HttpRequestBody_RequestWithPlainTextBody_ShouldParseBody()
    {
        // Arrange
        HttpRequest? actual = null;
        _server.MapRoute(HttpRequestMethod.POST, "/test", request =>
        {
            actual = request;
            return HttpResponse.Ok();
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
            Assert.Equal("text/plain; charset=utf-8", actual.ContentType?.Value);
            //Assert.Equal("text/plain; charset=utf-8", actual.ContentType);
        });
    }

    [Fact]
    public void HttpRequestBody_RequestWithLargeBody_ShouldProcessRequest()
    {
        // Arrange
        HttpRequest? actual = null;
        var expected = new string('A', 500_000);
        _server.MapRoute(HttpRequestMethod.POST, "/test", request =>
        {
            actual = request;
            return HttpResponse.Ok();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Post, "/test")
        {
            Content = new StringContent(expected)
        };
        var response = _httpClient.Send(message);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(actual);
            Assert.Equal(expected, actual.Body);
        });
    }
}