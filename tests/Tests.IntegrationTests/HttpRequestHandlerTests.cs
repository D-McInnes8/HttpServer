using System.Net;
using Application;
using Application.Request;

namespace Tests.IntegrationTests;

public class HttpRequestHandlerTests : IAsyncLifetime
{
    private readonly HttpServer _httpServer = new HttpServer(9995);
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task InitializeAsync()
    {
        _httpClient.BaseAddress = new Uri($"http://localhost:{_httpServer.Port}");
        await _httpServer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        await _httpServer.StopAsync();
    }
    
    [Fact]
    public async Task HttpRequestHandler_ExceptionThrown_ShouldReturn500InternalServerError()
    {
        // Arrange
        _httpServer.AddRoute(HttpRequestMethod.GET, "/test", (_) => throw new Exception());
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}