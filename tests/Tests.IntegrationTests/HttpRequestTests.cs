using System.Net;
using Application;
using Application.Pipeline.Endpoints;
using Application.Request;
using Application.Response;
using Tests.IntegrationTests.TestExtensions;

namespace Tests.IntegrationTests;

public class HttpRequestTests : IAsyncLifetime
{
    private readonly HttpServer _httpServer = HttpServer.CreateBuilder(9999).Build();
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
        _httpServer.AddEndpointPipeline(options =>
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
        _httpServer.AddEndpointPipeline(options =>
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
        _httpServer.AddEndpointPipeline(options =>
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