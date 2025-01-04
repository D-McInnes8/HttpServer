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
        var actual = Assert.Single(tree.RootNode.Children);
        Assert.Multiple(() =>
        {
            Assert.Equal("/helloworld/", actual.Prefix);
            Assert.Equal(1, actual.Value);
            Assert.Equal(NodeType.Path, actual.Type);
        });
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
        Assert.Fail();
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
        var actualA = Assert.Single(tree.RootNode.Children);
        Assert.Multiple(() =>
        {
            Assert.Equal(NodeType.Path, actualA.Type);
            Assert.Equal("/v", actualA.Prefix);
            Assert.Equal(2, actualA.Children.Length);
        });

        var actual1 = actualA.Children[1];
        var actual2 = actualA.Children[0];
        Assert.Multiple(() =>
        {
            Assert.Equal("1/", actual1.Prefix);
            Assert.Equal(1, actual1.Value);
            Assert.Equal(NodeType.Path, actual1.Type);
            
            Assert.Equal("2/", actual2.Prefix);
            Assert.Equal(2, actual2.Value);
            Assert.Equal(NodeType.Path, actual2.Type);
        });
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
        Assert.Equal(2, tree.RootNode.Children.Length);
        
        var actual1 = tree.RootNode.Children[1];
        var actual2 = tree.RootNode.Children[0];
        Assert.Multiple(() =>
        {
            Assert.Equal("hello/", actual1.Prefix);
            Assert.Equal(1, actual1.Value);
            Assert.Equal(NodeType.Path, actual1.Type);
            
            Assert.Equal("world/", actual2.Prefix);
            Assert.Equal(2, actual2.Value);
            Assert.Equal(NodeType.Path, actual2.Type);
        });
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
        var actual = Assert.Single(tree.RootNode.Children);
        Assert.Multiple(() =>
        {
            Assert.Equal("/helloworld/", actual.Prefix);
            Assert.Equal(2, actual.Value);
            Assert.Equal(NodeType.Path, actual.Type);
        });
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
        var actualPath = Assert.Single(tree.RootNode.Children);
        Assert.Multiple(() =>
        {
            Assert.Equal("/hello/", actualPath.Prefix);
            Assert.Equal(NodeType.Path, actualPath.Type);
            
            var actualParameter = Assert.Single(actualPath.Children);
            Assert.Multiple(() =>
            {
                Assert.Equal("{name}", actualParameter.Prefix);
                Assert.Equal(NodeType.Parameter, actualParameter.Type);
                
                var actualValue = Assert.Single(actualParameter.Children);
                Assert.Multiple(() =>
                {
                    Assert.Equal(1, actualValue.Value);
                    Assert.Equal(NodeType.Path, actualValue.Type);
                });
            });
        });
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
        var actualPath = Assert.Single(tree.RootNode.Children);
        Assert.Multiple(() =>
        {
            Assert.Equal("/hello/", actualPath.Prefix);
            Assert.Equal(NodeType.Path, actualPath.Type);
            
            var actualWildcard = Assert.Single(actualPath.Children);
            Assert.Multiple(() =>
            {
                Assert.Equal("{*}", actualWildcard.Prefix);
                Assert.Equal(NodeType.Wildcard, actualWildcard.Type);
                
                var actualValue = Assert.Single(actualWildcard.Children);
                Assert.Multiple(() =>
                {
                    Assert.Equal(1, actualValue.Value);
                    Assert.Equal(NodeType.Path, actualValue.Type);
                });
            });
        });
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
    public void AddRoute_ParameterNodeWithChildRoutes_ShouldContainSingleParameterNode()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path1 = new Route("/hello/{name}", HttpRequestMethod.GET);
        var path2 = new Route("/hello/{name}/world", HttpRequestMethod.GET);
        
        // Act
        tree.AddRoute(path1, 1);
        tree.AddRoute(path2, 2);
        
        // Assert
        var actualPath = Assert.Single(tree.RootNode.Children);
        Assert.Multiple(() =>
        {
            Assert.Equal("/hello/", actualPath.Prefix);
            Assert.Equal(NodeType.Path, actualPath.Type);
        });
        
        var actualParameter = Assert.Single(actualPath.Children);
        Assert.Multiple(() =>
        {
            Assert.Equal("{name}", actualParameter.Prefix);
            Assert.Equal(NodeType.Parameter, actualParameter.Type);
        });
        
        var actualChild = Assert.Single(actualParameter.Children);
        Assert.Multiple(() =>
        {
            Assert.Equal("/world/", actualChild.Prefix);
            Assert.Equal(2, actualChild.Value);
            Assert.Equal(NodeType.Path, actualChild.Type);
        });
    }

    [Fact]
    public void AddRoute_MultipleParameterChildNodes_ShouldThrowException()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path1 = new Route("/hello/{name}", HttpRequestMethod.GET);
        var path2 = new Route("/hello/{age}", HttpRequestMethod.GET);
        tree.AddRoute(path1, 1);
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => tree.AddRoute(path2, 2));
    }

    [Fact]
    public void Match_EmptyTree_ShouldReturnNull()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        
        // Act
        var actual = tree.Match(new Route("/helloworld", HttpRequestMethod.GET));
        
        // Assert
        Assert.False(actual.IsMatch);
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
        Assert.Multiple(() =>
        {
            Assert.True(actual.IsMatch);
            Assert.Equal(1, actual.Value);
        });
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
        Assert.Multiple(() =>
        {
            Assert.Equal(1, actual1.Value);
            Assert.Equal(2, actual2.Value);
        });
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
        Assert.Multiple(() =>
        {
            Assert.Equal(1, actual1.Value);
            Assert.Equal(2, actual2.Value);
        });
    }
    
    [Fact]
    public void Match_NoMatchingRoute_ShouldReturnNull()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/helloworld", HttpRequestMethod.GET);
        
        // Act
        var actual = tree.Match(path);
        
        // Assert
        Assert.False(actual.IsMatch);
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
        var actual = tree.Match(path2);
        
        // Assert
        Assert.False(actual.IsMatch);
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
        Assert.Equal(1, actual.Value);
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
        Assert.Multiple(() =>
        {
            Assert.True(actual.IsMatch);
            Assert.Equal("world", actual.Parameters["name"]);
        });
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
        Assert.Multiple(() =>
        {
            Assert.True(actual.IsMatch);
            Assert.Equal("world", actual.Parameters["name"]);
            Assert.Equal("42", actual.Parameters["age"]);
        });
    }
    
    [Fact]
    public void Match_RequestWithParameterFollowedByPath_ShouldNotMatch()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/hello/{name}", HttpRequestMethod.GET);
        tree.AddRoute(path, 1);
        
        // Act
        var actual = tree.Match(new Route("/hello/world/42", HttpRequestMethod.GET));
        
        // Assert
        Assert.False(actual.IsMatch);
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
        Assert.Multiple(() =>
        {
            Assert.True(actual.IsMatch);
            Assert.Equal("world", actual.Parameters["name"]);
        });
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
        Assert.Multiple(() =>
        {
            Assert.True(actual.IsMatch);
            Assert.Empty(actual.Parameters);
            Assert.Equal(2, actual.Value);
        });
    }
    
    [Fact]
    public void Match_RouteWithWildcard_ShouldParseWildcard()
    {
        // Arrange
        var tree = new RoutingRadixTree<int>();
        var path = new Route("/hello/{*}", HttpRequestMethod.GET);
        tree.AddRoute(path, 1);
        
        // Act
        var actual = tree.Match(new Route("/hello/world/42", HttpRequestMethod.GET));
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(actual.IsMatch);
            Assert.Equal("world/42", actual.Parameters["*"]);
        });
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
        Assert.Multiple(() =>
        {
            Assert.True(actual.IsMatch);
            Assert.Empty(actual.Parameters);
            Assert.Equal(2, actual.Value);
        });
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
        Assert.Multiple(() =>
        {
            Assert.True(actual.IsMatch);
            Assert.Equal("world", actual.Parameters["name"]);
            Assert.Equal("42", actual.Parameters["*"]);
        });
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
        Assert.Multiple(() =>
        {
            Assert.True(actual.IsMatch);
            Assert.Equal(1, actual.Value);
        });
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
        Assert.Multiple(() =>
        {
            Assert.True(actual.IsMatch);
            Assert.Equal("world", actual.Parameters["name"]);
            Assert.Equal(1, actual.Value);
        });
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
        Assert.Multiple(() =>
        {
            Assert.True(actual.IsMatch);
            Assert.Equal("world", actual.Parameters["*"]);
            Assert.Equal(1, actual.Value);
        });
    }
}