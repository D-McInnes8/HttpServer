using HttpServer.Request;
using HttpServer.Routing;
using Xunit.Abstractions;

namespace Tests.UnitTests;

public class RoutingRadixTreeTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RoutingRadixTreeTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void AddRoute_Single_ShouldContainRoute()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/helloworld", HttpRequestMethod.GET);
        
        // Act
        tree.AddRoute(path, 1);
        
        // Assert
        Assert.True(tree.Contains(path));
    }

    [Fact]
    public void AddRoute_MultipleSamePathDifferentMethod_ShouldContainBothRoutes()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path1 = new Route("/helloworld", HttpRequestMethod.GET);
        var path2 = new Route("/helloworld", HttpRequestMethod.POST);
        
        // Act
        tree.AddRoute(path1, 1);
        tree.AddRoute(path2, 2);
        
        // Assert
        Assert.True(tree.Contains(path1));
        Assert.True(tree.Contains(path2));
    }

    [Fact]
    public void AddRoute_MultipleSameMethodDifferentPaths_ShouldContainBothRoutes()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path1 = new Route("/v1", HttpRequestMethod.GET);
        var path2 = new Route("/v2", HttpRequestMethod.GET);
        
        // Act
        tree.AddRoute(path1, 1);
        tree.AddRoute(path2, 2);
        
        // Assert
        Assert.True(tree.Contains(path1));
        Assert.True(tree.Contains(path2));
    }

    [Fact]
    public void AddRoute_MultipleWithNoOverlap_ShouldContainBothRoutes()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path1 = new Route("hello", HttpRequestMethod.GET);
        var path2 = new Route("world", HttpRequestMethod.GET);
        
        // Act
        tree.AddRoute(path1, 1);
        tree.AddRoute(path2, 2);
        
        // Assert
        Assert.True(tree.Contains(path1));
        Assert.True(tree.Contains(path2));
    }

    [Fact]
    public void AddRoute_DuplicatePaths_ShouldOverwriteExisting()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/helloworld", HttpRequestMethod.GET);
        tree.AddRoute(path, 1);
        
        // Act
        tree.AddRoute(path, 2);
        
        // Assert
        Assert.Equal(2, tree.Match(path));
    }
    
    [Fact]
    public void AddRoute_WithParameter_ShouldContainRoute()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/hello/{name}", HttpRequestMethod.GET);
        
        // Act
        tree.AddRoute(path, 1);
        
        // Assert
        Assert.True(tree.Contains(path));
    }
    
    [Fact]
    public void AddRoute_WithWildcard_ShouldContainRoute()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/hello/{*}", HttpRequestMethod.GET);
        
        // Act
        tree.AddRoute(path, 1);
        
        // Assert
        Assert.True(tree.Contains(path));
    }

    [Theory]
    [InlineData("/hello/{*}/world")]
    [InlineData("/hello/{*}/world/{*}")]
    [InlineData("/hello/{*}/world/{name}")]
    [InlineData("/hello/{*}/{name}")]
    [InlineData("/hello/{*}/{name}/{*}")]
    public void AddRoute_WithWildcardFollowedByPath_ShouldThrowException(string path)
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var route = new Route(path, HttpRequestMethod.GET);
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => tree.AddRoute(route, 1));
    }

    [Fact]
    public void Match_EmptyTree_ShouldReturnNull()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        
        // Act
        int? actual = tree.Match(new Route("/helloworld", HttpRequestMethod.GET));
        
        // Assert
        Assert.Null(actual);
    }
    
    [Fact]
    public void Match_SingleMatchingRoute_ShouldReturnRoute()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/helloworld", HttpRequestMethod.GET);
        tree.AddRoute(path, 1);
        
        // Act
        var actual = tree.Match(path);
        
        // Assert
        Assert.Equal(1, actual);
    }

    [Fact]
    public void Match_MultipleSamePathDifferentMethod_ShouldReturnCorrectRoute()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path1 = new Route("/helloworld", HttpRequestMethod.GET);
        var path2 = new Route("/helloworld", HttpRequestMethod.POST);
        
        tree.AddRoute(path1, 1);
        tree.AddRoute(path2, 2);
        
        // Act
        var actual1 = tree.Match(path1);
        var actual2 = tree.Match(path2);
        
        // Assert
        Assert.Equal(1, actual1);
        Assert.Equal(2, actual2);
    }

    [Fact]
    public void Match_MultipleSameMethodDifferentPath_ShouldReturnCorrectRoute()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path1 = new Route("/v1", HttpRequestMethod.GET);
        var path2 = new Route("/v2", HttpRequestMethod.GET);
        
        tree.AddRoute(path1, 1);
        tree.AddRoute(path2, 2);
        
        // Act
        var actual1 = tree.Match(path1);
        var actual2 = tree.Match(path2);
        
        // Assert
        Assert.Equal(1, actual1);
        Assert.Equal(2, actual2);
    }
    
    [Fact]
    public void Match_NoMatchingRoute_ShouldReturnNull()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/helloworld", HttpRequestMethod.GET);
        
        // Act
        int? actual = tree.Match(path);
        
        // Assert
        Assert.Null(actual);
    }
    
    [Fact]
    public void Match_NoMatchingMethod_ShouldReturnNull()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path1 = new Route("/helloworld", HttpRequestMethod.GET);
        var path2 = new Route("/helloworld", HttpRequestMethod.POST);
        
        tree.AddRoute(path1, 1);
        
        // Act
        int? actual = tree.Match(path2);
        
        // Assert
        Assert.Null(actual);
    }
    
    [Fact]
    public void Match_RoutesWithSamePrefix_ShouldReturnCorrectRoute()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path1 = new Route("/helloworld", HttpRequestMethod.GET);
        var path2 = new Route("/hello", HttpRequestMethod.GET);
        
        tree.AddRoute(path1, 1);
        tree.AddRoute(path2, 2);
        
        // Act
        var actual = tree.Match(path1);
        
        // Assert
        Assert.Equal(1, actual);
    }

    [Fact]
    public void Match_RouteWithParameter_ShouldParseParameter()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/hello/{name}", HttpRequestMethod.GET);
        tree.AddRoute(path, 1);
        
        // Act
        var actual = tree.Match(new Route("/hello/world", HttpRequestMethod.GET));
        
        // Assert
        Assert.Equal(1, actual);
    }
    
    [Fact]
    public void Match_RouteWithMultipleParameters_ShouldParseParameters()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/hello/{name}/{age}", HttpRequestMethod.GET);
        tree.AddRoute(path, 1);
        
        // Act
        var actual = tree.Match(new Route("/hello/world/42", HttpRequestMethod.GET));
        
        // Assert
        Assert.Equal(1, actual);
    }
    
    [Fact]
    public void Match_RouteWithParametersAndStaticPath_ShouldParseParameters()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/hello/{name}/world", HttpRequestMethod.GET);
        tree.AddRoute(path, 1);
        
        // Act
        var actual = tree.Match(new Route("/hello/world/world", HttpRequestMethod.GET));
        
        // Assert
        Assert.Equal(1, actual);
    }
    
    [Fact]
    public void Match_RouteMatchesOnParameterAndStaticPath_ShouldUseStaticPath()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path1 = new Route("/hello/{name}/world", HttpRequestMethod.GET);
        var path2 = new Route("/hello/world/world", HttpRequestMethod.GET);
        
        tree.AddRoute(path1, 1);
        tree.AddRoute(path2, 2);
        
        // Act
        var actual = tree.Match(new Route("/hello/world/world", HttpRequestMethod.GET));
        
        // Assert
        Assert.Equal(2, actual);
    }
    
    [Fact]
    public void Match_RouteWithWildcard_ShouldParseWildcard()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/hello/{*}", HttpRequestMethod.GET);
        tree.AddRoute(path, 1);
        
        // Act
        var actual = tree.Match(new Route("/hello/world", HttpRequestMethod.GET));
        
        // Assert
        Assert.Equal(1, actual);
    }
    
    [Fact]
    public void Match_RouteMatchesOnWildcardAndStaticPath_ShouldUseStaticPath()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path1 = new Route("/hello/{*}", HttpRequestMethod.GET);
        var path2 = new Route("/hello/world", HttpRequestMethod.GET);
        
        tree.AddRoute(path1, 1);
        tree.AddRoute(path2, 2);
        
        // Act
        var actual = tree.Match(new Route("/hello/world", HttpRequestMethod.GET));
        
        // Assert
        Assert.Equal(2, actual);
    }

    [Fact]
    public void Match_RouteWithParameterFollowedByWildcard_ShouldReturnCorrectRoute()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/hello/{name}/{*}", HttpRequestMethod.GET);
        tree.AddRoute(path, 1);
        
        // Act
        var actual = tree.Match(new Route("/hello/world/42", HttpRequestMethod.GET));
        
        // Assert
        Assert.Equal(1, actual);
    }

    [Fact]
    public void Match_BaseRoute_ShouldReturnCorrectRoute()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/", HttpRequestMethod.GET);
        tree.AddRoute(path, 1);
        
        // Act
        var actual = tree.Match(new Route("/", HttpRequestMethod.GET));
        
        // Assert
        Assert.Equal(1, actual);
    }
    
    [Fact]
    public void Match_BaseRouteWithParameter_ShouldReturnCorrectRoute()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/{name}", HttpRequestMethod.GET);
        tree.AddRoute(path, 1);
        
        // Act
        var actual = tree.Match(new Route("/world", HttpRequestMethod.GET));
        
        // Assert
        Assert.Equal(1, actual);
    }
    
    [Fact]
    public void Match_BaseRouteWithWildcard_ShouldReturnCorrectRoute()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/{*}", HttpRequestMethod.GET);
        tree.AddRoute(path, 1);
        
        // Act
        var actual = tree.Match(new Route("/world", HttpRequestMethod.GET));
        
        // Assert
        Assert.Equal(1, actual);
    }
    
    [Fact]
    public void Contains_NoMatchingRoute_ShouldReturnFalse()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/helloworld", HttpRequestMethod.GET);
        
        // Act
        bool actual = tree.Contains(path);
        
        // Assert
        Assert.False(actual);
    }
    
    [Fact]
    public void Contains_NoMatchingMethod_ShouldReturnFalse()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path1 = new Route("/helloworld", HttpRequestMethod.GET);
        var path2 = new Route("/helloworld", HttpRequestMethod.POST);
        
        tree.AddRoute(path1, 1);
        
        // Act
        bool actual = tree.Contains(path2);
        
        // Assert
        Assert.False(actual);
    }
    
    [Fact]
    public void Contains_MatchingRoute_ShouldReturnTrue()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/helloworld", HttpRequestMethod.GET);
        tree.AddRoute(path, 1);
        
        // Act
        var actual = tree.Contains(path);
        
        // Assert
        Assert.True(actual);
    }
    
    [Fact]
    public void Contains_MatchingMethod_ShouldReturnTrue()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/helloworld", HttpRequestMethod.GET);
        tree.AddRoute(path, 1);
        
        // Act
        var actual = tree.Contains(path);
        
        // Assert
        Assert.True(actual);
    }
    
    [Fact]
    public void Contains_MultipleSamePathDifferentMethod_ShouldReturnTrue()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path1 = new Route("/helloworld", HttpRequestMethod.GET);
        var path2 = new Route("/helloworld", HttpRequestMethod.POST);
        
        tree.AddRoute(path1, 1);
        tree.AddRoute(path2, 2);
        
        // Act
        var actual1 = tree.Contains(path1);
        var actual2 = tree.Contains(path2);
        
        // Assert
        Assert.True(actual1);
        Assert.True(actual2);
    }
    
    [Fact]
    public void Contains_MultipleSameMethodDifferentPath_ShouldReturnTrue()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path1 = new Route("/v1", HttpRequestMethod.GET);
        var path2 = new Route("/v2", HttpRequestMethod.GET);
        
        tree.AddRoute(path1, 1);
        tree.AddRoute(path2, 2);
        
        // Act
        var actual1 = tree.Contains(path1);
        var actual2 = tree.Contains(path2);
        
        // Assert
        Assert.True(actual1);
        Assert.True(actual2);
    }
}