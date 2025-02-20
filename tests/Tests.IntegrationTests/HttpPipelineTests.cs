using System.Net;
using HttpServer;
using HttpServer.Pipeline;
using HttpServer.Pipeline.Registry;
using HttpServer.Request;
using HttpServer.Response;
using HttpServer.Routing;
using Microsoft.Extensions.DependencyInjection;
using Tests.IntegrationTests.TestPipelines;

namespace Tests.IntegrationTests;

public class HttpPipelineTests : IAsyncLifetime
{
    private readonly IHttpWebServer _server = HttpWebServer.CreateBuilder(0).Build();
    private readonly HttpClient _httpClient = new HttpClient();
    private IPipelineRegistry? _pipelineRegistry;

    public async Task InitializeAsync()
    {
        await _server.StartAsync();
        _httpClient.BaseAddress = new Uri($"http://localhost:{_server.Port}");
        _pipelineRegistry = _server.Services.GetRequiredService<IPipelineRegistry>();
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        await _server.StopAsync();
    }
    
    [Fact]
    public async Task HttpGlobalPipeline_DefaultGlobalPipeline_ShouldExecuteRequest()
    {
        // Arrange
        _server.MapGet("/", _ => HttpResponse.Ok());
        
        // Act
        var response = await _httpClient.GetAsync("/");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task HttpGlobalPipeline_ThrowsException_ShouldReturn500Response()
    {
        // Arrange
        _server.MapRoute(HttpRequestMethod.GET, "/test", _ => throw new InvalidOperationException());
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public void HttpPipeline_AddPipeline_AccessibleInPipelineRegistry()
    {
        // Arrange
        _server.AddPipeline("test", _ => { });
        
        // Act
        var actual = Assert.IsAssignableFrom<IRequestPipeline>(_pipelineRegistry?.GetPipeline("test"));
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.NotNull(actual);
            Assert.Equal("test", actual.Name);
        });
    }

    [Fact]
    public void HttpPipeline_AddPipeline_PluginIsAccessibleInPipelineRegistry()
    {
        // Arrange
        _server.AddPipeline("test", options =>
        {
            options.AddPlugin<TestPlugin>();
        });
        
        // Act
        var pipeline = Assert.IsAssignableFrom<IRequestPipeline>(_pipelineRegistry?.GetPipeline("test"));
        
        // Assert
        var actual = Assert.Single(pipeline.Options.Plugins);
        Assert.Equal(typeof(TestPlugin), actual);
    }
    
    [Fact]
    public void HttpPipeline_AddPipeline_CustomPipelineOptions()
    {
        // Arrange
        _server.AddPipeline<TestPipelineOptions>("test", options =>
        {
            options.AddPlugin<TestPlugin>();
            options.UseTestPlugin = true;
        });
        
        // Act
        var pipeline = Assert.IsAssignableFrom<IRequestPipeline>(_pipelineRegistry?.GetPipeline("test"));
        
        // Assert
        Assert.NotNull(pipeline);
        var actual = Assert.IsType<TestPipelineOptions>(pipeline.Options);
        Assert.True(actual.UseTestPlugin);
    }
    
    [Fact]
    public async Task HttpPipeline_PluginReturnsErrorResponse_ShouldReturnErrorResponse()
    {
        // Arrange
        _server.AddPipeline("test", options =>
        {
            options.AddPlugin<TestPlugin>();
        });
        _server.MapGet(
            path: "/",
            handler: _ => HttpResponse.Ok(),
            pipelineName: "test");
        
        // Act
        var response = await _httpClient.GetAsync("/");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task HttpPipeline_NoPlugins_ShouldExecuteRequestHandler()
    {
        // Arrange
        _server.AddPipeline("test", _ => { });
        _server.MapGet(
            path: "/",
            handler: _ => HttpResponse.Ok("Hello, World!"),
            pipelineName: "test");
        
        // Act
        var response = await _httpClient.GetAsync("/");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var actual = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello, World!", actual);
    }
}