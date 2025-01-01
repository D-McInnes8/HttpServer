using System.Net;
using HttpServer;
using HttpServer.Pipeline.Endpoints;
using HttpServer.Request;
using HttpServer.Response;
using Tests.IntegrationTests.TestExtensions;

namespace Tests.IntegrationTests;

public class HttpRequestTests : IAsyncLifetime
{
    private readonly HttpWebWebServer _httpWebWebServer = HttpWebWebServer.CreateBuilder(9999).Build();
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

    [Theory]
    [InlineData(HttpRequestMethod.GET)]
    [InlineData(HttpRequestMethod.POST)]
    [InlineData(HttpRequestMethod.PUT)]
    [InlineData(HttpRequestMethod.DELETE)]
    [InlineData(HttpRequestMethod.PATCH)]
    [InlineData(HttpRequestMethod.HEAD)]
    [InlineData(HttpRequestMethod.OPTIONS)]
    [InlineData(HttpRequestMethod.TRACE)]
    //[InlineData(HttpRequestMethod.CONNECT)]
    public async Task HttpRequestRouting_RequestWithValidRoute_ShouldReturnOk(HttpRequestMethod httpRequestMethod)
    {
        // Arrange
        _httpWebWebServer.AddEndpointPipeline(options =>
        {
            options.MapRoute(httpRequestMethod, "/", _ => new HttpResponse(HttpResponseStatusCode.OK));
        });
        
        // Act
        var message = new HttpRequestMessage(httpRequestMethod.ToHttpMethod(), "/");
        var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        });
    }
    
    [Theory]
    [InlineData(HttpRequestMethod.GET)]
    [InlineData(HttpRequestMethod.POST)]
    [InlineData(HttpRequestMethod.PUT)]
    [InlineData(HttpRequestMethod.DELETE)]
    [InlineData(HttpRequestMethod.PATCH)]
    [InlineData(HttpRequestMethod.HEAD)]
    [InlineData(HttpRequestMethod.OPTIONS)]
    [InlineData(HttpRequestMethod.TRACE)]
    //[InlineData(HttpRequestMethod.CONNECT)]
    public async Task HttpRequestRouting_RequestWithInvalidRoute_ShouldReturnOk(HttpRequestMethod httpRequestMethod)
    {
        // Arrange
        _httpWebWebServer.AddEndpointPipeline(options =>
        {
            options.MapRoute(httpRequestMethod, "/test", _ => new HttpResponse(HttpResponseStatusCode.OK));
        });
        
        // Act
        var message = new HttpRequestMessage(httpRequestMethod.ToHttpMethod(), "/");
        var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        });
    }

    [Theory]
    [InlineData("/test", "query=Hello")]
    [InlineData("/test", "query=Hello&name=World")]
    [InlineData("/test", "query=Hello&name=World&age=42")]
    [InlineData("/test", "query=Hello&name=World&age=42&city=New York")]
    public async Task HttpRequestRouting_RequestWithQueryParameters_ShouldReturnOk(string route, string queryParameters)
    {
        // Arrange
        _httpWebWebServer.AddEndpointPipeline(options =>
        {
            options.MapRoute(HttpRequestMethod.GET, route, _ => new HttpResponse(HttpResponseStatusCode.OK));
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, $"{route}?{queryParameters}");
        var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        });
    }
}