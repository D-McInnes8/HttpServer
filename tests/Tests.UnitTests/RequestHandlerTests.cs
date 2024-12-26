using Application;
using Application.Request;
using Application.Response;
using Application.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.UnitTests;

public class RequestHandlerTests
{
    private readonly IServiceProvider _serviceProvider;

    public RequestHandlerTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IRouteRegistry, RouteRegistry>();
        services.AddScoped<RoutingPlugin>();
        _serviceProvider = services.BuildServiceProvider();
    }

    private void AddRoute(string path)
    {
        _serviceProvider
            .GetRequiredService<IRouteRegistry>()
            .AddRoute(HttpRequestMethod.GET, path, _ => new HttpResponse(HttpResponseStatusCode.OK));
    }
    
    [Fact]
    public void HandleRequest_WithMatchingRoute_ReturnsOkResponse()
    {
        // Arrange
        AddRoute("/helloworld");
        var requestHandler = new RequestHandler();
        var httpRequest = new HttpRequest(HttpRequestMethod.GET, "/helloworld");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, _serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public void HandleRequest_WithNonMatchingRoute_ReturnsNotFoundResponse()
    {
        // Arrange
        AddRoute("/helloworld");
        var requestHandler = new RequestHandler();
        var httpRequest = new HttpRequest(HttpRequestMethod.GET, "/notfound");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, _serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void HandleRequest_WithMatchingRouteButDifferentMethod_ReturnsNotFoundResponse()
    {
        // Arrange
        AddRoute("/helloworld");
        var requestHandler = new RequestHandler();
        var httpRequest = new HttpRequest(HttpRequestMethod.POST, "/helloworld");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, _serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void HandleRequest_WithNonMatchingRouteButSameMethod_ReturnsNotFoundResponse()
    {
        // Arrange
        AddRoute("/helloworld");
        var requestHandler = new RequestHandler();
        var httpRequest = new HttpRequest(HttpRequestMethod.GET, "/notfound");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, _serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public void HandleRequest_WithMultipleRoutes_ReturnsOkResponse()
    {
        // Arrange
        AddRoute("/");
        AddRoute("/helloworld");
        var requestHandler = new RequestHandler();
        var httpRequest = new HttpRequest(HttpRequestMethod.GET, "/helloworld");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, _serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public void HandleRequest_WithMultipleRoutes_ReturnsNotFoundResponse()
    {
        // Arrange
        AddRoute("/");
        AddRoute("/helloworld");
        var requestHandler = new RequestHandler();
        var httpRequest = new HttpRequest(HttpRequestMethod.GET, "/notfound");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, _serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public void HandleRequest_WithMultipleRoutes_ReturnsOkResponseForFirstMatchingRoute()
    {
        // Arrange
        AddRoute("/");
        AddRoute("/helloworld");
        var requestHandler = new RequestHandler();
        var httpRequest = new HttpRequest(HttpRequestMethod.GET, "/");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, _serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public void HandleRequest_MissingTrailingSlash_ReturnsNotFoundResponse()
    {
        // Arrange
        AddRoute("/helloworld/");
        var requestHandler = new RequestHandler();
        var httpRequest = new HttpRequest(HttpRequestMethod.GET, "/helloworld");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, _serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public void HandleRequest_WithQueryParameters_ReturnsOkResponse()
    {
        // Arrange
        AddRoute("/helloworld");
        var requestHandler = new RequestHandler();
        var httpRequest = new HttpRequest(HttpRequestMethod.GET, "/helloworld?name=World");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, _serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.OK, response.StatusCode);
    }
}