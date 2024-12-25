using Application;
using Application.Request;
using Application.Response;
using Application.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.UnitTests;

public class RequestHandlerTests
{
    [Fact]
    public void HandleRequest_WithMatchingRoute_ReturnsOkResponse()
    {
        // Arrange
        var routes = new[]
        {
            new Route(HttpRequestMethod.GET, "/helloworld")
        };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var requestHandler = new RequestHandler(routes);
        var httpRequest = new HttpRequest(HttpRequestMethod.GET, "/helloworld");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public void HandleRequest_WithNonMatchingRoute_ReturnsNotFoundResponse()
    {
        // Arrange
        var routes = new[]
        {
            new Route(HttpRequestMethod.GET, "/helloworld")
        };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var requestHandler = new RequestHandler(routes);
        var httpRequest = new HttpRequest(HttpRequestMethod.GET, "/notfound");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void HandleRequest_WithMatchingRouteButDifferentMethod_ReturnsNotFoundResponse()
    {
        // Arrange
        var routes = new[]
        {
            new Route(HttpRequestMethod.GET, "/helloworld")
        };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var requestHandler = new RequestHandler(routes);
        var httpRequest = new HttpRequest(HttpRequestMethod.POST, "/helloworld");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void HandleRequest_WithNonMatchingRouteButSameMethod_ReturnsNotFoundResponse()
    {
        // Arrange
        var routes = new[]
        {
            new Route(HttpRequestMethod.GET, "/helloworld")
        };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var requestHandler = new RequestHandler(routes);
        var httpRequest = new HttpRequest(HttpRequestMethod.GET, "/notfound");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public void HandleRequest_WithMultipleRoutes_ReturnsOkResponse()
    {
        // Arrange
        var routes = new[]
        {
            new Route(HttpRequestMethod.GET, "/"),
            new Route(HttpRequestMethod.GET, "/helloworld")
        };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var requestHandler = new RequestHandler(routes);
        var httpRequest = new HttpRequest(HttpRequestMethod.GET, "/helloworld");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public void HandleRequest_WithMultipleRoutes_ReturnsNotFoundResponse()
    {
        // Arrange
        var routes = new[]
        {
            new Route(HttpRequestMethod.GET, "/"),
            new Route(HttpRequestMethod.GET, "/helloworld")
        };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var requestHandler = new RequestHandler(routes);
        var httpRequest = new HttpRequest(HttpRequestMethod.GET, "/notfound");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public void HandleRequest_WithMultipleRoutes_ReturnsOkResponseForFirstMatchingRoute()
    {
        // Arrange
        var routes = new[]
        {
            new Route(HttpRequestMethod.GET, "/"),
            new Route(HttpRequestMethod.GET, "/helloworld")
        };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var requestHandler = new RequestHandler(routes);
        var httpRequest = new HttpRequest(HttpRequestMethod.GET, "/");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public void HandleRequest_MissingTrailingSlash_ReturnsNotFoundResponse()
    {
        // Arrange
        var routes = new[]
        {
            new Route(HttpRequestMethod.GET, "/helloworld/")
        };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var requestHandler = new RequestHandler(routes);
        var httpRequest = new HttpRequest(HttpRequestMethod.GET, "/helloworld");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public void HandleRequest_WithQueryParameters_ReturnsOkResponse()
    {
        // Arrange
        var routes = new[]
        {
            new Route(HttpRequestMethod.GET, "/helloworld")
        };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var requestHandler = new RequestHandler(routes);
        var httpRequest = new HttpRequest(HttpRequestMethod.GET, "/helloworld?name=World");
        
        // Act
        var response = requestHandler.HandleRequest(httpRequest, serviceProvider);
        
        // Assert
        Assert.Equal(HttpResponseStatusCode.OK, response.StatusCode);
    }
}