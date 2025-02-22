using System.Net;
using HttpServer;
using HttpServer.Request;
using HttpServer.Response;
using HttpServer.Routing;
using Tests.IntegrationTests.TestExtensions;

namespace Tests.IntegrationTests;

public class HttpRequestTests : IAsyncLifetime
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
    
    [Fact]
    public async Task HttpRequest_MetadataExceedsBufferSize_ShouldStillParseRequest()
    {
        // Arrange
        HttpRequest? actual = null;
        const int numberOfHeaders = 100;
        _server.MapRoute(HttpRequestMethod.POST, "/test", ctx =>
        {
            actual = ctx.Request;
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
    
    [Theory]
    [InlineData("John Doe")]
    [InlineData("john.doe@example.com")]
    [InlineData("New York")]
    [InlineData("Hello World")]
    [InlineData("你好")]
    public async Task HttpRequest_UrlEncodingQueryParameters_ShouldDecodeParameters(string plainText)
    {
        // Arrange
        _server.MapGet("/api/v1/users/{userId}", context =>
        {
            var userId = context.RouteParameters["userId"];
            return HttpResponse.Ok(userId);
        });

        // Act
        var encodedText = Uri.EscapeDataString(plainText);
        var message = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/users/{encodedText}");
        var response = await _httpClient.SendAsync(message);
        var actual = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(plainText, actual);
    }
    
    [Theory]
    [InlineData("name=John+Doe")]
    [InlineData("email=john.doe+example.com")]
    [InlineData("city=New+York")]
    [InlineData("query=Hello+World")]
    [InlineData("multiple=Hello+World+from+New+York")]
    public async Task HttpRequest_UrlEncodingSpacePlus_ShouldDecodeParameters(string query)
    {
        // Arrange
        _server.MapGet("/api/v1/search", context =>
        {
            var key = context.Request.QueryParameters.GetKey(0);
            var value = context.Request.QueryParameters.Get(key);
            var queryParameter = $"{key}={value}";
            return HttpResponse.Ok(queryParameter);
        });

        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/search?{query}");
        var response = await _httpClient.SendAsync(message);
        var actual = await response.Content.ReadAsStringAsync();

        // Assert
        var expected = query.Replace("+", " ");
        Assert.Equal(expected, actual);
    }
}