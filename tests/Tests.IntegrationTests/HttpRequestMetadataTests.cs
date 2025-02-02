using HttpServer;
using HttpServer.Request;
using HttpServer.Response;
using HttpServer.Routing;

namespace Tests.IntegrationTests;

public class HttpRequestMetadataTests : IAsyncLifetime
{
    private readonly IHttpWebServer _server = HttpWebServer.CreateBuilder(0).Build();
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task InitializeAsync()
    {
        await _server.StartAsync();
        _httpClient.BaseAddress = new Uri($"http://localhost:{_server.Port}");
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        await _server.StopAsync();
    }

    [Theory]
    [InlineData("query", "Hello")]
    [InlineData("name", "World")]
    [InlineData("age", "42")]
    [InlineData("city", "New York")]
    public async Task HttpRequestMetadata_RequestWithSingleQueryParameter_ShouldParseQueryParameter(string param, string value)
    {
        // Arrange
        HttpRequest? actual = null;
        _server.MapGet("/test", ctx =>
        {
            actual = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, $"/test?{param}={value}");
        _ = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.NotNull(actual);
        Assert.Equal(value, actual.QueryParameters[param]);
    }
    
    [Fact]
    public async Task HttpRequestMetadata_RequestWithQueryParameters_ShouldParseQueryParameters()
    {
        // Arrange
        HttpRequest? actual = null;
        _server.MapGet("/test", ctx =>
        {
            actual = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, "/test?query=Hello&name=World&age=42&city=New York");
        _ = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.NotNull(actual);
        Assert.Multiple(() =>
        {
            Assert.Equal("Hello", actual.QueryParameters["query"]);
            Assert.Equal("World", actual.QueryParameters["name"]);
            Assert.Equal("42", actual.QueryParameters["age"]);
            Assert.Equal("New York", actual.QueryParameters["city"]);
        });
    }

    [Theory]
    [InlineData("Accept-Language", "en-US")]
    [InlineData("Authorization", "Bearer 123456")]
    [InlineData("Accept", "text/html")]
    [InlineData("Host", "localhost:9999")]
    [InlineData("Referer", "http://example.com/")]
    [InlineData("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)")]
    [InlineData("X-Custom-Header", "CustomValue")]
    [InlineData("X-Number-Header", "1234567890")]
    [InlineData("X-Space-Header", "Value with spaces")]
    [InlineData("X-Special-Header", "Value-With-Special_Characters!@#")]
    public async Task HttpRequestMetadata_RequestWithSingleHeader_ShouldParseHeader(string headerName, string headerValue)
    {
        // Arrange & Assert
        HttpRequest? actual = null;
        _server.MapGet("/test", ctx =>
        {
            actual = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, "/test");
        message.Headers.Add(headerName, headerValue);
        var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Equal(headerValue, actual?.Headers[headerName]);
        Assert.True(response.IsSuccessStatusCode);
    }
    
    [Fact]
    public async Task HttpRequestMetadata_RequestWithMultipleHeaders_ShouldParseHeaders()
    {
        // Arrange & Assert
        HttpRequest? actual = null;
        _server.MapGet("/test", ctx =>
        {
            actual = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, "/test");
        message.Headers.Add("Accept-Language", "en-US");
        message.Headers.Add("Authorization", "Bearer 123456");
        var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal("en-US", actual?.Headers["Accept-Language"]);
            Assert.Equal("Bearer 123456", actual?.Headers["Authorization"]);
            Assert.True(response.IsSuccessStatusCode);
        });
    }
}