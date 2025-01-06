using System.Net;
using HttpServer;
using HttpServer.Request;
using HttpServer.Response;
using HttpServer.Routing;
using Tests.IntegrationTests.TestExtensions;

namespace Tests.IntegrationTests;

public class HttpRouterTests : IAsyncLifetime
{
    private readonly IHttpWebServer _server = HttpWebServer.CreateBuilder(9993).Build();
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
    public async Task HttpRouter_SimpleRoute_ShouldReturnOk(HttpRequestMethod httpRequestMethod)
    {
        // Arrange
        _server.MapGet("/api", _ => HttpResponse.Ok());
        
        // Act
        var message = new HttpRequestMessage(httpRequestMethod.ToHttpMethod(), "/api");
        var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        });
    }
    
    [Theory]
    [InlineData("/api/v1/users")]
    [InlineData("/api/v1/users/1")]
    [InlineData("/api-users")]
    public async Task HttpRouter_SimpleRoutes_ShouldReturnNotFoundForSubPaths(string requestUri)
    {
        // Arrange
        _server.MapGet("/api", _ => HttpResponse.Ok());
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        });
    }

    [Theory]
    [InlineData(HttpRequestMethod.POST)]
    [InlineData(HttpRequestMethod.PUT)]
    [InlineData(HttpRequestMethod.DELETE)]
    [InlineData(HttpRequestMethod.PATCH)]
    [InlineData(HttpRequestMethod.HEAD)]
    [InlineData(HttpRequestMethod.OPTIONS)]
    [InlineData(HttpRequestMethod.TRACE)]
    public async Task HttpRouter_SimpleRoute_ShouldNotReturnMethodNotAllowedForDifferentMethods(HttpRequestMethod httpRequestMethod)
    {
        // Arrange
        _server.MapGet("/api", _ => HttpResponse.Ok());
        
        // Act
        var message = new HttpRequestMessage(httpRequestMethod.ToHttpMethod(), "/api");
        var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        });
    }
    
    [Theory]
    [InlineData("/api", HttpStatusCode.OK)]
    [InlineData("/api/v1/users", HttpStatusCode.OK)]
    [InlineData("/api/v1/users/1", HttpStatusCode.OK)]
    [InlineData("/api/v1/users/1/posts", HttpStatusCode.OK)]
    [InlineData("/api/v1/users/1/posts/1", HttpStatusCode.OK)]
    [InlineData("/api/v1/users/1/posts/1/comments", HttpStatusCode.OK)]
    [InlineData("/api/v1/users/1/posts/1/comments/1", HttpStatusCode.OK)]
    [InlineData("/api/v1/comments", HttpStatusCode.OK)]
    [InlineData("/api/v1/comments/1", HttpStatusCode.OK)]
    [InlineData("/api/v1/posts", HttpStatusCode.OK)]
    [InlineData("/api/v1/posts/1", HttpStatusCode.OK)]
    [InlineData("/api/v2/users", HttpStatusCode.OK)]
    [InlineData("/api/v2/users/1", HttpStatusCode.OK)]
    public async Task HttpRouter_ComplexNestedRoutes_ShouldReturnCorrectStatusCode(string requestUri, HttpStatusCode expected)
    {
        // Arrange
        _server.MapGet("/api", _ => HttpResponse.Ok());
        _server.MapGet("/api/v1/users", _ => HttpResponse.Ok());
        _server.MapGet("/api/v1/users/{userId}", _ => HttpResponse.Ok());
        _server.MapGet("/api/v1/users/{userId}/posts", _ => HttpResponse.Ok());
        _server.MapGet("/api/v1/users/{userId}/posts/{postId}", _ => HttpResponse.Ok());
        _server.MapGet("/api/v1/users/{userId}/posts/{postId}/comments", _ => HttpResponse.Ok());
        _server.MapGet("/api/v1/users/{userId}/posts/{postId}/comments/{commentId}", _ => HttpResponse.Ok());
        _server.MapGet("/api/v1/comments", _ => HttpResponse.Ok());
        _server.MapGet("/api/v1/comments/{commentId}", _ => HttpResponse.Ok());
        _server.MapGet("/api/v1/posts", _ => HttpResponse.Ok());
        _server.MapGet("/api/v1/posts/{postId}", _ => HttpResponse.Ok());
        _server.MapGet("/api/v2/users", _ => HttpResponse.Ok());
        _server.MapGet("/api/v2/users/{userId}", _ => HttpResponse.Ok());
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(expected, response.StatusCode);
        });
    }

    [Theory]
    [InlineData("david")]
    [InlineData("david123")]
    [InlineData("david-123")]
    [InlineData("david_123")]
    [InlineData("david.123")]
    [InlineData("david@123")]
    [InlineData("david#123")]
    [InlineData("david$123")]
    [InlineData("david%123")]
    [InlineData("david^123")]
    [InlineData("david&123")]
    [InlineData("david*123")]
    [InlineData("david(123")]
    [InlineData("david)123")]
    [InlineData("david+123")]
    [InlineData("david=123")]
    public async Task HttpRouter_RouteWithSingleParameter_ShouldReturnOk(string parameter)
    {
        // Arrange
        _server.MapGet("/api/v1/users/{userId}", _ => HttpResponse.Ok());
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/users/{parameter}");;
        var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        });
    }
    
    [Theory]
    [InlineData("david")]
    [InlineData("david123")]
    [InlineData("david-123")]
    [InlineData("david_123")]
    [InlineData("david.123")]
    [InlineData("david@123")]
    [InlineData("david#123")]
    [InlineData("david$123")]
    [InlineData("david%123")]
    [InlineData("david^123")]
    [InlineData("david&123")]
    [InlineData("david*123")]
    [InlineData("david(123")]
    [InlineData("david)123")]
    [InlineData("david+123")]
    [InlineData("david=123")]
    public async Task HttpRouter_RouteWithSingleParameter_ShouldParseParameter(string expected)
    {
        // Arrange
        _server.MapGet("/api/v1/users/{userId}", context =>
        {
            var userId = context.RouteParameters["userId"];
            return HttpResponse.Ok(userId);
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/users/{expected}");;
        var response = await _httpClient.SendAsync(message);
        var actual = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/api")]
    [InlineData("/api/v1/users")]
    [InlineData("/api/v1/users/1")]
    [InlineData("/api/v1/users/1/posts")]
    [InlineData("/api/v1/users/1/posts/1")]
    [InlineData("/api/v1/users/1/posts/1/comments")]
    [InlineData("/api/v1/users/1/posts/1/comments/1")]
    [InlineData("/api/v1/comments")]
    [InlineData("/api/v1/comments/1")]
    [InlineData("/api/v1/posts")]
    [InlineData("/api/v1/posts/1")]
    [InlineData("/api/v2/users")]
    [InlineData("/api/v2/users/1")]
    [InlineData("/api/v3/user/-")]
    [InlineData("/api/v3/user/_")]
    [InlineData("/api/v3/user/.")]
    [InlineData("/api/v3/user/@")]
    [InlineData("/api/v3/user/#")]
    [InlineData("/api/v3/user/$")]
    [InlineData("/api/v3/user/%")]
    [InlineData("/api/v3/user/^")]
    [InlineData("/api/v3/user/&")]
    [InlineData("/api/v3/user/*")]
    [InlineData("/api/v3/user/(123")]
    [InlineData("/api/v3/user/)123")]
    [InlineData("/api/v3/user/+123")]
    [InlineData("/api/v3/user/=123")]
    public async Task HttpRouter_RouteWithWildcard_ShouldReturnOk(string path)
    {
        // Arrange
        _server.MapGet("/{*}", _ => HttpResponse.Ok());
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        });
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/api")]
    [InlineData("/api/v1/users")]
    [InlineData("/api/v1/users/1")]
    [InlineData("/api/v1/users/1/posts")]
    [InlineData("/api/v1/users/1/posts/1")]
    [InlineData("/api/v1/users/1/posts/1/comments")]
    [InlineData("/api/v1/users/1/posts/1/comments/1")]
    [InlineData("/api/v1/comments")]
    [InlineData("/api/v1/comments/1")]
    [InlineData("/api/v1/posts")]
    [InlineData("/api/v1/posts/1")]
    [InlineData("/api/v2/users")]
    [InlineData("/api/v2/users/1")]
    [InlineData("/api/v3/user/-")]
    [InlineData("/api/v3/user/_")]
    [InlineData("/api/v3/user/.")]
    [InlineData("/api/v3/user/@")]
    [InlineData("/api/v3/user/#")]
    [InlineData("/api/v3/user/$")]
    [InlineData("/api/v3/user/%")]
    [InlineData("/api/v3/user/^")]
    [InlineData("/api/v3/user/&")]
    [InlineData("/api/v3/user/*")]
    [InlineData("/api/v3/user/(123")]
    [InlineData("/api/v3/user/)123")]
    [InlineData("/api/v3/user/+123")]
    [InlineData("/api/v3/user/=123")]
    public async Task HttpRouter_RouteWithWildcard_ShouldParseWildcardPath(string expected)
    {
        // Arrange
        _server.MapGet("/{*}", context =>
        {
            var path = context.RouteParameters.Wildcard;
            return HttpResponse.Ok(path);
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, expected);
        var response = await _httpClient.SendAsync(message);
        var actual = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.Equal(expected[1..], actual);
    }
}