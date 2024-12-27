using Application;
using Application.Request;
using Application.Response;
using NSubstitute;

namespace Tests.IntegrationTests;

public class HttpRequestMetadataTests : IAsyncLifetime
{
    private readonly HttpServer _httpServer = HttpServer.CreateBuilder(9998).Build();
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
    [InlineData("query", "Hello")]
    [InlineData("name", "World")]
    [InlineData("age", "42")]
    [InlineData("city", "New York")]
    public async Task HttpRequestMetadata_RequestWithSingleQueryParameter_ShouldParseQueryParameter(string param, string value)
    {
        // Arrange
        HttpRequest? actual = null;
        _httpServer.AddRoute(HttpRequestMethod.GET, "/test", (request) =>
        {
            actual = request;
            return new HttpResponse(HttpResponseStatusCode.OK);
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
        _httpServer.AddRoute(HttpRequestMethod.GET, "/test", (request) =>
        {
            actual = request;
            return new HttpResponse(HttpResponseStatusCode.OK);
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
        _httpServer.AddRoute(HttpRequestMethod.GET, "/test", (request) =>
        {
            Assert.Equal(headerValue, request.Headers[headerName]);
            return new HttpResponse(HttpResponseStatusCode.OK);
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, "/test");
        message.Headers.Add(headerName, headerValue);
        var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }
    
    [Fact]
    public async Task HttpRequestMetadata_RequestWithMultipleHeaders_ShouldParseHeaders()
    {
        // Arrange & Assert
        _httpServer.AddRoute(HttpRequestMethod.GET, "/test", (request) =>
        {
            Assert.Equal("en-US", request.Headers["Accept-Language"]);
            Assert.Equal("Bearer 123456", request.Headers["Authorization"]);
            return new HttpResponse(HttpResponseStatusCode.OK);
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, "/test");
        message.Headers.Add("Accept-Language", "en-US");
        message.Headers.Add("Authorization", "Bearer 123456");
        var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }
}