using HttpServer;
using HttpServer.Request;
using HttpServer.Response;
using HttpServer.Routing;

namespace Tests.IntegrationTests;

public class HttpRequestQueryParameterTests : IAsyncLifetime
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
    public async Task HttpRequestQueryParameter_SingleQueryParameter_ShouldParseQueryParameter(string param, string value)
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
    public async Task HttpRequestQueryParameter_WithSpecialCharacters_ShouldDecodeQueryParameter()
    {
        // Arrange
        HttpRequest? actual = null;
        _server.MapGet("/test", ctx =>
        {
            actual = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, "/test?query=Hello%20World");
        _ = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.NotNull(actual);
        Assert.Equal("Hello World", actual.QueryParameters["query"]);
    }
    
    [Fact]
    public async Task HttpRequestQueryParameter_MultipleValues_ShouldCombineParameters()
    {
        // Arrange
        HttpRequest? actual = null;
        _server.MapGet("/test", ctx =>
        {
            actual = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, "/test?query=Hello&query=World&query=from&query=New%20York");
        _ = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.NotNull(actual);
        Assert.Equal("Hello,World,from,New York", actual.QueryParameters["query"]);
    }
    
    [Fact]
    public async Task HttpRequestQueryParameter_MultipleValuesDifferentKeys_ShouldParseQueryParameters()
    {
        // Arrange
        HttpRequest? actual = null;
        _server.MapGet("/test", ctx =>
        {
            actual = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, "/test?query=Hello&name=World&age=42&city=New%20York");
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
    
    [Fact]
    public async Task HttpRequestQueryParameter_EmptyQueryParameter_ShouldBeEmpty()
    {
        // Arrange
        HttpRequest? actual = null;
        _server.MapGet("/test", ctx =>
        {
            actual = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, "/test?");
        _ = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.NotNull(actual);
        Assert.Empty(actual.QueryParameters);
    }
}