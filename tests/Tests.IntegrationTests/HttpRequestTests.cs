using System.Net;
using HttpServer;
using HttpServer.Request;
using HttpServer.Response;
using Tests.IntegrationTests.TestExtensions;

namespace Tests.IntegrationTests;

public class HttpRequestTests : IAsyncLifetime
{
    private readonly IHttpWebServer _server = HttpWebServer.CreateBuilder(9999).Build();
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
        _server.MapGet("/", _ => HttpResponse.Ok());
        
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
        _server.MapGet("/test", _ => HttpResponse.Ok());
        
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
        _server.MapGet(route, _ => HttpResponse.Ok());
        
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
    
    [Fact]
    public async Task HttpRequest_MetadataExceedsBufferSize_ShouldStillParseRequest()
    {
        // Arrange
        HttpRequest? actual = null;
        const int numberOfHeaders = 100;
        _server.MapRoute(HttpRequestMethod.POST, "/test", request =>
        {
            actual = request;
            return HttpResponse.Ok();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Post, "/test")
        {
            Content = new StringContent(new string('A', 1000))
        };
        for (int i = 0; i < numberOfHeaders; i++)
        {
            message.Headers.Add($"X-Custom-Header-{i}", new string('A', i));
        }
        var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(actual);

        for (int i = 0; i < numberOfHeaders; i++)
        {
            var headerName = $"X-Custom-Header-{i}";
            var hasHeader = actual.Headers.TryGetValue(headerName, out var headerValue);
            Assert.True(hasHeader);
            Assert.Equal(new string('A', i), headerValue);
        }
    }
}