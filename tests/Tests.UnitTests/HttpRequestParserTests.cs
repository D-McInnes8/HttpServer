using System.Text;
using HttpServer;
using HttpServer.Request;
using HttpServer.Request.Parser;

namespace Tests.UnitTests;

public class HttpRequestParserTests
{
    [Theory]
    [InlineData("GET", "/", false)]
    [InlineData("POST", "/submit", true)]
    [InlineData("PUT", "/update", true)]
    [InlineData("DELETE", "/delete", false)]
    [InlineData("PATCH", "/patch", true)]
    [InlineData("HEAD", "/head", false)]
    [InlineData("OPTIONS", "/options", false)]
    [InlineData("CONNECT", "/connect", false)]
    [InlineData("TRACE", "/trace", false)]
    public async Task RequestHttpMethods_ParsesCorrectly(string method, string path, bool hasBody)
    {
        // Arrange
        var body = hasBody ? "Body content" : string.Empty;
        var request = $"{method} {path} HTTP/1.1\r\nHost: localhost\r\nContent-Length: {body.Length}\r\n\r\n{body}";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        
        // Act
        var httpRequest = await HttpRequestParser.Parse(stream);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal(Enum.Parse<HttpRequestMethod>(method), httpRequest.Method);
            Assert.Equal(path, httpRequest.Path);
            Assert.Equal(hasBody, httpRequest.HasBody);
        });
    }

    [Theory]
    [InlineData("GET", "/", "HTTP/1.0")]
    [InlineData("POST", "/submit", "HTTP/1.1")]
    [InlineData("PUT", "/update", "HTTP/2.0")]
    public async Task RequestHttpVersion_ParsesCorrectly(string method, string path, string httpVersion)
    {
        // Arrange
        var request = $"{method} {path} {httpVersion}\r\nHost: localhost\r\n\r\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));

        // Act
        var httpRequest = await HttpRequestParser.Parse(stream);

        // Assert
        Assert.Equal(httpVersion, httpRequest.HttpVersion);
    }
    
    [Theory]
    [InlineData("GET", "/", "HTTP/1.1")]
    [InlineData("GET", "/index.html", "HTTP/1.1")]
    [InlineData("GET", "/api/v1/resource", "HTTP/1.1")]
    [InlineData("GET", "/api/v1/resource/123", "HTTP/1.1")]
    [InlineData("GET", "/api/v1/resource?query=param", "HTTP/1.1")]
    [InlineData("GET", "/api/v1/resource#fragment", "HTTP/1.1")]
    public async Task RequestPaths_ParsesCorrectly(string method, string path, string httpVersion)
    {
        // Arrange
        var request = $"{method} {path} {httpVersion}\r\nHost: localhost\r\n\r\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));

        // Act
        var httpRequest = await HttpRequestParser.Parse(stream);

        // Assert
        Assert.Equal(path, httpRequest.Path);
    }

    [Fact]
    public async Task RequestWithCommonHeaders_ParsesCorrectly()
    {
        // Arrange
        const string request = "GET / HTTP/1.1\r\nHost: localhost\r\nUser-Agent: xUnit\r\nAccept: #1#*\r\n\r\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        
        // Act
        var httpRequest = await HttpRequestParser.Parse(stream);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal(HttpRequestMethod.GET, httpRequest.Method);
            Assert.Equal("/", httpRequest.Path);
            Assert.False(httpRequest.HasBody);
            Assert.Equal("localhost", httpRequest.Headers["Host"]);
            Assert.Equal("xUnit", httpRequest.Headers["User-Agent"]);
            Assert.Equal("#1#*", httpRequest.Headers["Accept"]);
        });
    }
    
    [Fact]
    public async Task RequestWithBody_ParsesCorrectly()
    {
        // Arrange
        const string request = "POST /submit HTTP/1.1\r\nHost: localhost\r\nContent-Length: 11\r\n\r\nHello World";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        
        // Act
        var httpRequest = await HttpRequestParser.Parse(stream);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal(HttpRequestMethod.POST, httpRequest.Method);
            Assert.Equal("/submit", httpRequest.Path);
            Assert.True(httpRequest.HasBody);
            Assert.Equal("Hello World", httpRequest.Body);
        });
    }
    
    [Fact]
    public async Task RequestWithEmptyBody_ParsesCorrectly()
    {
        // Arrange
        const string request = "POST /submit HTTP/1.1\r\nHost: localhost\r\nContent-Length: 0\r\n\r\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));

        // Act
        var httpRequest = await HttpRequestParser.Parse(stream);

        // Assert
        Assert.False(httpRequest.HasBody);
    }
    
    [Theory]
    [InlineData("GET / HTTP/1.1\r\nHost: localhost\r\n\r\n", "localhost")]
    [InlineData("GET / HTTP/1.1\nHost: localhost\n\n", "localhost")]
    [InlineData("GET / HTTP/1.1\rHost: localhost\r\r", "localhost")]
    public async Task RequestWithDifferentNewLineCharacters_ParsesCorrectly(string request, string expectedHost)
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        
        // Act
        var httpRequest = await HttpRequestParser.Parse(stream);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal(HttpRequestMethod.GET, httpRequest.Method);
            Assert.Equal("/", httpRequest.Path);
            Assert.Equal("HTTP/1.1", httpRequest.HttpVersion);
            Assert.Equal(expectedHost, httpRequest.Headers["Host"]);
        });
    }

    [Fact]
    public async Task RequestWithContentTypeHeader_ShouldSetContentTypeProperty()
    {
        // Arrange
        const string request = "POST /submit HTTP/1.1\r\nHost: localhost\r\nContent-Type: application/json\r\n\r\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        var expected = new HttpContentType("application", "json");
        
        // Act
        var httpRequest = await HttpRequestParser.Parse(stream);
        
        // Assert
        Assert.Equal(expected, httpRequest.ContentType);
        //Assert.Equal("application/json", httpRequest.ContentType);
    }

    [Fact]
    public async Task RequestWithQueryParameters_SetQueryParametersCorrectly()
    {
        // Arrange
        const string request = "GET /api/v1/resource?query=param HTTP/1.1\r\nHost: localhost\r\n\r\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        
        // Act
        var httpRequest = await HttpRequestParser.Parse(stream);
        
        // Assert
        Assert.Single(httpRequest.QueryParameters);
        Assert.Equal("param", httpRequest.QueryParameters["query"]);
    }
    
    [Fact]
    public async Task RequestWithMultipleQueryParameters_SetQueryParametersCorrectly()
    {
        // Arrange
        const string request = "GET /api/v1/resource?query=param&filter=123 HTTP/1.1\r\nHost: localhost\r\n\r\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        
        // Act
        var httpRequest = await HttpRequestParser.Parse(stream);
        
        // Assert
        Assert.Equal(2, httpRequest.QueryParameters.Count);
        Assert.Equal("param", httpRequest.QueryParameters["query"]);
        Assert.Equal("123", httpRequest.QueryParameters["filter"]);
    }
    
    [Theory]
    [InlineData("GET /api/v1/resource?query=param&query=123 HTTP/1.1\r\nHost: localhost\r\n\r\n")]
    [InlineData("GET /api/v1/resource?query=param,123 HTTP/1.1\r\nHost: localhost\r\n\r\n")]
    public async Task RequestWithDuplicateQueryParameters_ShouldParseBothParameters(string request)
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        
        // Act
        var httpRequest = await HttpRequestParser.Parse(stream);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.Single(httpRequest.QueryParameters);
            Assert.Equal("param,123", httpRequest.QueryParameters["query"]);
        });
    }
}