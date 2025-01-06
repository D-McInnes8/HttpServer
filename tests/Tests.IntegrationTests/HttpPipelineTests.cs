using System.Net;
using HttpServer;
using HttpServer.Request;
using HttpServer.Response;
using Tests.IntegrationTests.TestExtensions;
using Tests.IntegrationTests.TestPipelines;

namespace Tests.IntegrationTests;

public class HttpPipelineTests : IAsyncLifetime
{
    private readonly IHttpWebServer _server = HttpWebServer.CreateBuilder(9994).Build();
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
    
    [Theory]
    [InlineData(HttpRequestMethod.GET, "/")]
    [InlineData(HttpRequestMethod.POST, "/")]
    [InlineData(HttpRequestMethod.PUT, "/")]
    [InlineData(HttpRequestMethod.DELETE, "/")]
    [InlineData(HttpRequestMethod.PATCH, "/")]
    [InlineData(HttpRequestMethod.HEAD, "/")]
    [InlineData(HttpRequestMethod.OPTIONS, "/")]
    [InlineData(HttpRequestMethod.TRACE, "/")]
    [InlineData(HttpRequestMethod.GET, "/test")]
    [InlineData(HttpRequestMethod.POST, "/test")]
    [InlineData(HttpRequestMethod.PUT, "/test")]
    [InlineData(HttpRequestMethod.DELETE, "/test")]
    [InlineData(HttpRequestMethod.PATCH, "/test")]
    [InlineData(HttpRequestMethod.HEAD, "/test")]
    [InlineData(HttpRequestMethod.OPTIONS, "/test")]
    [InlineData(HttpRequestMethod.TRACE, "/test")]
    public async Task HttpPipeline_DefaultPipeline_ShouldReturnNotFound(HttpRequestMethod method, string path)
    {
        // Arrange
        var message = new HttpRequestMessage(method.ToHttpMethod(), path);
        
        // Act
        using var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}