using System.Net;
using Application;
using Application.Pipeline.Endpoints;
using Application.Request;
using Application.Response;
using Tests.IntegrationTests.TestExtensions;
using Tests.IntegrationTests.TestPipelines;

namespace Tests.IntegrationTests;

public class HttpPipelineTests : IAsyncLifetime
{
    private readonly HttpServer _httpServer = HttpServer.CreateBuilder(9994).Build();
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
    [InlineData(HttpRequestMethod.GET, "/")]
    [InlineData(HttpRequestMethod.POST, "/")]
    [InlineData(HttpRequestMethod.PUT, "/")]
    [InlineData(HttpRequestMethod.DELETE, "/")]
    [InlineData(HttpRequestMethod.PATCH, "/")]
    [InlineData(HttpRequestMethod.HEAD, "/")]
    [InlineData(HttpRequestMethod.OPTIONS, "/")]
    [InlineData(HttpRequestMethod.TRACE, "/")]
    [InlineData(HttpRequestMethod.GET, "/test")]
    [InlineData(HttpRequestMethod.POST, "/test")]
    [InlineData(HttpRequestMethod.PUT, "/test")]
    [InlineData(HttpRequestMethod.DELETE, "/test")]
    [InlineData(HttpRequestMethod.PATCH, "/test")]
    [InlineData(HttpRequestMethod.HEAD, "/test")]
    [InlineData(HttpRequestMethod.OPTIONS, "/test")]
    [InlineData(HttpRequestMethod.TRACE, "/test")]
    public async Task HttpPipeline_DefaultPipeline_ShouldReturnNotFound(HttpRequestMethod method, string path)
    {
        // Arrange
        var message = new HttpRequestMessage(method.ToHttpMethod(), path);
        
        // Act
        using var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task HttpPipeline_CustomPipeline_ShouldReturnOk()
    {
        // Arrange
        _httpServer.AddPipeline(options =>
        {
            options.UseRouter<TestRouter>();
            options.UseRequestHandler<TestRequestHandler>();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, "/test");
        using var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task HttpPipeline_CustomPipeline_ShouldReturnNotFound()
    {
        // Arrange
        _httpServer.AddPipeline(options =>
        {
            options.UseRouter<TestRouter>();
            options.UseRequestHandler<TestRequestHandler>();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, "/");
        using var response = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    public async Task HttpPipeline_MultiplePipelines_ShouldUseHighestPriority(int priorityA, int priorityB)
    {
        // Arrange
        _httpServer
            .AddEndpointPipeline(options =>
            {
                options.Priority = priorityA;
                options.MapRoute(HttpRequestMethod.GET, "/test", _ => HttpResponse.Ok(priorityA.ToString()));
            })
            .AddEndpointPipeline(options =>
            {
                options.Priority = priorityB;
                options.MapRoute(HttpRequestMethod.GET, "/test", _ => HttpResponse.Ok(priorityB.ToString()));
            });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, "/test");
        using var response = await _httpClient.SendAsync(message);
        var actual = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("1", actual);
    }
    
    [Theory]
    [InlineData(HttpRequestMethod.GET, "/test")]
    [InlineData(HttpRequestMethod.POST, "/test")]
    [InlineData(HttpRequestMethod.PUT, "/test")]
    [InlineData(HttpRequestMethod.DELETE, "/test")]
    [InlineData(HttpRequestMethod.PATCH, "/test")]
    [InlineData(HttpRequestMethod.HEAD, "/test")]
    [InlineData(HttpRequestMethod.OPTIONS, "/test")]
    [InlineData(HttpRequestMethod.TRACE, "/test")]
    public async Task HttpPipeline_EndpointPipelineValidEndpoint_ShouldReturnOk(HttpRequestMethod method, string path)
    {
        // Arrange
        _httpServer.AddEndpointPipeline(options =>
        {
            options.MapRoute(method, path, _ => HttpResponse.Ok());
        });
        
        // Act
        var message = new HttpRequestMessage(method.ToHttpMethod(), path);
        using var actual = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
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
    public async Task HttpPipeline_EndpointPipelineInvalidEndpoint_ShouldReturnNotFound(HttpRequestMethod method)
    {
        // Arrange
        _httpServer.AddEndpointPipeline(options =>
        {
            options.MapRoute(method, "/test", _ => HttpResponse.Ok());
        });
        
        // Act
        var message = new HttpRequestMessage(method.ToHttpMethod(), "/test2");
        using var actual = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.StatusCode);
    }

    [Theory]
    [InlineData(HttpRequestMethod.GET)]
    [InlineData(HttpRequestMethod.POST)]
    [InlineData(HttpRequestMethod.PUT)]
    [InlineData(HttpRequestMethod.DELETE)]
    [InlineData(HttpRequestMethod.PATCH)]
    [InlineData(HttpRequestMethod.OPTIONS)]
    [InlineData(HttpRequestMethod.TRACE)]
    public async Task HttpPipeline_EndpointPipelineSamePathMultipleMethods_ShouldReturnOk(HttpRequestMethod method)
    {
        // Arrange
        _httpServer.AddEndpointPipeline(options =>
        {
            options.MapRoute(HttpRequestMethod.GET, "/test", _ => HttpResponse.Ok("GET"));
            options.MapRoute(HttpRequestMethod.POST, "/test", _ => HttpResponse.Ok("POST"));
            options.MapRoute(HttpRequestMethod.PUT, "/test", _ => HttpResponse.Ok("PUT"));
            options.MapRoute(HttpRequestMethod.DELETE, "/test", _ => HttpResponse.Ok("DELETE"));
            options.MapRoute(HttpRequestMethod.PATCH, "/test", _ => HttpResponse.Ok("PATCH"));
            options.MapRoute(HttpRequestMethod.HEAD, "/test", _ => HttpResponse.Ok("HEAD"));
            options.MapRoute(HttpRequestMethod.OPTIONS, "/test", _ => HttpResponse.Ok("OPTIONS"));
            options.MapRoute(HttpRequestMethod.TRACE, "/test", _ => HttpResponse.Ok("TRACE"));
        });
        
        // Act
        var message = new HttpRequestMessage(method.ToHttpMethod(), "/test");
        using var response = await _httpClient.SendAsync(message);
        var actual = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(method.ToString(), actual);
    }

    [Theory]
    [InlineData(HttpRequestMethod.GET)]
    [InlineData(HttpRequestMethod.POST)]
    [InlineData(HttpRequestMethod.PUT)]
    [InlineData(HttpRequestMethod.DELETE)]
    [InlineData(HttpRequestMethod.PATCH)]
    [InlineData(HttpRequestMethod.OPTIONS)]
    [InlineData(HttpRequestMethod.TRACE)]
    public async Task HttpPipeline_EndpointPipelineSameMethodDifferentPaths_ShouldReturnOk(HttpRequestMethod method)
    {
        // Arrange
        _httpServer.AddEndpointPipeline(options =>
        {
            options.MapRoute(method, "/test1", _ => HttpResponse.Ok("TEST1"));
            options.MapRoute(method, "/test2", _ => HttpResponse.Ok("TEST2"));
        });
        
        // Act
        using var message1 = new HttpRequestMessage(method.ToHttpMethod(), "/test1");
        using var response1 = await _httpClient.SendAsync(message1);
        var actual1 = await response1.Content.ReadAsStringAsync();
        
        using var message2 = new HttpRequestMessage(method.ToHttpMethod(), "/test2");
        using var response2 = await _httpClient.SendAsync(message2);
        var actual2 = await response2.Content.ReadAsStringAsync();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal("TEST1", actual1);
        
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        Assert.Equal("TEST2", actual2);
    }
}