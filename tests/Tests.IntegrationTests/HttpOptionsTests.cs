using System.Net;
using HttpServer;
using HttpServer.Request;
using HttpServer.Response;
using HttpServer.Routing;

namespace Tests.IntegrationTests;

public class HttpOptionsTests : IAsyncLifetime
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
    public async Task HttpOptionsRequest_WithWildcard_ShouldReturnResponse()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "*");
        
        // Act
        var response = await _httpClient.SendAsync(request);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Theory]
    [InlineData(HttpRequestMethod.GET, "/")]
    [InlineData(HttpRequestMethod.GET, "/api/resource")]
    [InlineData(HttpRequestMethod.POST, "/api/resource")]
    [InlineData(HttpRequestMethod.PUT, "/api/resource")]
    [InlineData(HttpRequestMethod.DELETE, "/api/resource")]
    [InlineData(HttpRequestMethod.PATCH, "/api/resource")]
    [InlineData(HttpRequestMethod.HEAD, "/api/resource")]
    public async Task HttpOptionsRequest_WithRoute_ShouldReturnCorrectHeaders(HttpRequestMethod httpRequestMethod, string requestUri)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, requestUri);
        _server.MapRoute(httpRequestMethod, requestUri, _ => HttpResponse.Ok());

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Content.Headers.Contains("Allow"));
        });
        
        var allowHeader = response.Content.Headers.GetValues("Allow").FirstOrDefault();
        Assert.Multiple(() =>
        {
            Assert.NotNull(allowHeader);
            Assert.Contains(httpRequestMethod.ToString(), allowHeader);
        });
    }
    
    [Fact]
    public async Task HttpOptionsRequest_WithMultipleEndpoints_ShouldReturnAllMethods()
    {
        // Arrange
        _server.MapRoute(HttpRequestMethod.GET, "/api/resource", _ => HttpResponse.Ok());
        _server.MapRoute(HttpRequestMethod.POST, "/api/resource", _ => HttpResponse.Ok());
        _server.MapRoute(HttpRequestMethod.PUT, "/api/resource", _ => HttpResponse.Ok());
        _server.MapRoute(HttpRequestMethod.DELETE, "/api/resource", _ => HttpResponse.Ok());
        _server.MapRoute(HttpRequestMethod.PATCH, "/api/resource", _ => HttpResponse.Ok());
        _server.MapRoute(HttpRequestMethod.HEAD, "/api/resource", _ => HttpResponse.Ok());
        
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/resource");

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Content.Headers.Contains("Allow"));
        });
        
        var allowHeader = response.Content.Headers.GetValues("Allow").ToArray();
        Assert.Multiple(() =>
        {
            Assert.NotNull(allowHeader);
            Assert.Contains(HttpRequestMethod.GET.ToString(), allowHeader);
            Assert.Contains(HttpRequestMethod.POST.ToString(), allowHeader);
            Assert.Contains(HttpRequestMethod.PUT.ToString(), allowHeader);
            Assert.Contains(HttpRequestMethod.DELETE.ToString(), allowHeader);
            Assert.Contains(HttpRequestMethod.PATCH.ToString(), allowHeader);
            Assert.Contains(HttpRequestMethod.HEAD.ToString(), allowHeader);
        });
    }
    
    [Fact (Skip = "Cors headers are not implemented yet")]
    public async Task HttpOptionsRequest_WithCors_ShouldReturnCorsHeaders()
    {
        // Arrange
        _server.MapGet("/api/resource", _ => HttpResponse.Ok());
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/resource");
        request.Headers.Add("Origin", "http://localhost");

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
            Assert.True(response.Headers.Contains("Access-Control-Allow-Methods"));
            Assert.True(response.Headers.Contains("Access-Control-Allow-Headers"));
        });
    }
    
    [Fact]
    public async Task HttpOptionsRequest_NoCorsHeadersWhenNoOrigin()
    {
        // Arrange
        _server.MapGet("/api/resource", _ => HttpResponse.Ok());
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/resource");

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
            Assert.False(response.Headers.Contains("Access-Control-Allow-Methods"));
            Assert.False(response.Headers.Contains("Access-Control-Allow-Headers"));
        });
    }
}