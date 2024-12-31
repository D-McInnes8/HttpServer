using System.Net;
using Application;
using Application.Pipeline.Endpoints;
using Application.Request;
using Application.Response;

namespace Tests.IntegrationTests;

public class HttpRequestHandlerTests : IAsyncLifetime
{
    private readonly HttpWebWebServer _httpWebWebServer = HttpWebWebServer.CreateBuilder(9995).Build();
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