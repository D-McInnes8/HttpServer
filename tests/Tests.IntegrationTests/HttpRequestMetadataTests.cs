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
}