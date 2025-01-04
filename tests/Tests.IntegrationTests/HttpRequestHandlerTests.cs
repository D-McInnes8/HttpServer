using System.Net;
using HttpServer;
using HttpServer.Pipeline.Endpoints;
using HttpServer.Request;
using HttpServer.Response;

namespace Tests.IntegrationTests;

public class HttpRequestHandlerTests : IAsyncLifetime
{
    private readonly IHttpWebServer _httpWebWebServer = HttpWebServer.CreateBuilder(9995).Build();
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
    public async Task HttpRequestHandler_ExceptionThrown_ShouldReturn500InternalServerError()
    {
        // Arrange
        _httpWebWebServer.AddEndpointPipeline(options =>
        {
            options.MapRoute(HttpRequestMethod.GET, "/test", _ => throw new Exception());
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}