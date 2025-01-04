using System.Net;
using HttpServer;
using HttpServer.Request;
using HttpServer.Response;

namespace Tests.IntegrationTests;

public class HttpRequestHandlerTests : IAsyncLifetime
{
    private readonly IHttpWebServer _server = HttpWebServer.CreateBuilder(9995).Build();
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
    public async Task HttpRequestHandler_ExceptionThrown_ShouldReturn500InternalServerError()
    {
        // Arrange
        _server.MapRoute(HttpRequestMethod.GET, "/test", _ => throw new Exception());
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}