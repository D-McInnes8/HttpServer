using System.Text;
using HttpServer.Body.Serializers;
using HttpServer.Headers;
using HttpServer.Networking;
using HttpServer.Request;
using HttpServer.Request.Parser;
using NSubstitute;

namespace Tests.UnitTests;

public class HttpRequestParserTests
{
    private readonly HttpRequestParser _httpRequestParser;
    
    public HttpRequestParserTests()
    {
        var serializerProvider = NSubstitute.Substitute.For<IHttpBodyContentSerializerProvider>();
        serializerProvider.GetSerializer(HttpContentType.TextPlain).Returns(new StringBodyContentSerializer());
        serializerProvider.GetSerializer(HttpContentType.ApplicationJson).Returns(new ByteArrayBodyContentSerializer());
        _httpRequestParser = new HttpRequestParser(serializerProvider);
    }
    
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
        var request = $"{method} {path} HTTP/1.1\r\nHost: localhost\r\nContent-Length: {body.Length}{(hasBody ? "\r\nContent-Type: text/plain" : "")}\r\n\r\n{body}";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        using var streamReader = new TcpNetworkStreamReader(stream);
        
        // Act
        var result = await _httpRequestParser.Parse(streamReader);

        // Assert
        var actual = result.Value;
        Assert.Multiple(() =>
        {
            Assert.Equal(Enum.Parse<HttpRequestMethod>(method), actual.Method);
            Assert.Equal(path, actual.Path);
            Assert.Equal(hasBody, actual.HasBody);
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
        using var streamReader = new TcpNetworkStreamReader(stream);

        // Act
        var result = await _httpRequestParser.Parse(streamReader);

        // Assert
        var actual = result.Value;
        Assert.Equal(httpVersion, actual.HttpVersion);
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
        using var streamReader = new TcpNetworkStreamReader(stream);

        // Act
        var result = await _httpRequestParser.Parse(streamReader);

        // Assert
        var actual = result.Value;
        Assert.Equal(path, actual.Path);
    }

    [Fact]
    public async Task RequestWithCommonHeaders_ParsesCorrectly()
    {
        // Arrange
        const string request = "GET / HTTP/1.1\r\nHost: localhost\r\nUser-Agent: xUnit\r\nAccept: #1#*\r\n\r\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        using var streamReader = new TcpNetworkStreamReader(stream);
        
        // Act
        var result = await _httpRequestParser.Parse(streamReader);

        // Assert
        var actual = result.Value;
        Assert.Multiple(() =>
        {
            Assert.Equal(HttpRequestMethod.GET, actual.Method);
            Assert.Equal("/", actual.Path);
            Assert.False(actual.HasBody);
            Assert.Equal("localhost", actual.Headers["Host"]);
            Assert.Equal("xUnit", actual.Headers["User-Agent"]);
            Assert.Equal("#1#*", actual.Headers["Accept"]);
        });
    }
    
    [Fact]
    public async Task RequestWithBody_ParsesCorrectly()
    {
        // Arrange
        const string request = "POST /submit HTTP/1.1\r\nHost: localhost\r\nContent-Length: 11\r\nContent-Type: text/plain\r\n\r\nHello World";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        using var streamReader = new TcpNetworkStreamReader(stream);
        
        // Act
        var result = await _httpRequestParser.Parse(streamReader);

        // Assert
        var actual = result.Value;
        Assert.Multiple(() =>
        {
            Assert.Equal(HttpRequestMethod.POST, actual.Method);
            Assert.Equal("/submit", actual.Path);
            Assert.True(actual.HasBody);
            Assert.Equal("Hello World", actual.Body?.Encoding.GetString(actual.Body.Content));
        });
    }
    
    [Fact]
    public async Task RequestWithEmptyBody_ParsesCorrectly()
    {
        // Arrange
        const string request = "POST /submit HTTP/1.1\r\nHost: localhost\r\nContent-Length: 0\r\n\r\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        using var streamReader = new TcpNetworkStreamReader(stream);

        // Act
        var result = await _httpRequestParser.Parse(streamReader);

        // Assert
        var actual = result.Value;
        Assert.False(actual.HasBody);
    }
    
    [Theory]
    [InlineData("GET / HTTP/1.1\r\nHost: localhost\r\n\r\n", "localhost")]
    [InlineData("GET / HTTP/1.1\nHost: localhost\n\n", "localhost")]
    [InlineData("GET / HTTP/1.1\rHost: localhost\r\r", "localhost")]
    public async Task RequestWithDifferentNewLineCharacters_ParsesCorrectly(string request, string expectedHost)
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        using var streamReader = new TcpNetworkStreamReader(stream);
        
        // Act
        var result = await _httpRequestParser.Parse(streamReader);

        // Assert
        var actual = result.Value;
        Assert.Multiple(() =>
        {
            Assert.Equal(HttpRequestMethod.GET, actual.Method);
            Assert.Equal("/", actual.Path);
            Assert.Equal("HTTP/1.1", actual.HttpVersion);
            Assert.Equal(expectedHost, actual.Headers["Host"]);
        });
    }

    [Fact]
    public async Task RequestWithContentTypeHeader_ShouldSetContentTypeProperty()
    {
        // Arrange
        const string request = "POST /submit HTTP/1.1\r\nHost: localhost\r\nContent-Type: application/json\r\n\r\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        using var streamReader = new TcpNetworkStreamReader(stream);
        var expected = new HttpContentType("application", "json");
        
        // Act
        var result = await _httpRequestParser.Parse(streamReader);
        
        // Assert
        var actual = result.Value;
        Assert.Equal(expected, actual.ContentType);
        //Assert.Equal("application/json", httpRequest.ContentType);
    }

    [Fact]
    public async Task RequestWithQueryParameters_SetQueryParametersCorrectly()
    {
        // Arrange
        const string request = "GET /api/v1/resource?query=param HTTP/1.1\r\nHost: localhost\r\n\r\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        using var streamReader = new TcpNetworkStreamReader(stream);
        
        // Act
        var result = await _httpRequestParser.Parse(streamReader);
        
        // Assert
        var actual = result.Value;
        Assert.Single(actual.QueryParameters);
        Assert.Equal("param", actual.QueryParameters["query"]);
    }
    
    [Fact]
    public async Task RequestWithMultipleQueryParameters_SetQueryParametersCorrectly()
    {
        // Arrange
        const string request = "GET /api/v1/resource?query=param&filter=123 HTTP/1.1\r\nHost: localhost\r\n\r\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        using var streamReader = new TcpNetworkStreamReader(stream);
        
        // Act
        var result = await _httpRequestParser.Parse(streamReader);
        
        // Assert
        var actual = result.Value;
        Assert.Equal(2, actual.QueryParameters.Count);
        Assert.Equal("param", actual.QueryParameters["query"]);
        Assert.Equal("123", actual.QueryParameters["filter"]);
    }
    
    [Theory]
    [InlineData("GET /api/v1/resource?query=param&query=123 HTTP/1.1\r\nHost: localhost\r\n\r\n")]
    [InlineData("GET /api/v1/resource?query=param,123 HTTP/1.1\r\nHost: localhost\r\n\r\n")]
    public async Task RequestWithDuplicateQueryParameters_ShouldParseBothParameters(string request)
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request));
        using var streamReader = new TcpNetworkStreamReader(stream);
        
        // Act
        var result = await _httpRequestParser.Parse(streamReader);
        
        // Assert
        var actual = result.Value;
        Assert.Multiple(() =>
        {
            Assert.Single(actual.QueryParameters);
            Assert.Equal("param,123", actual.QueryParameters["query"]);
        });
    }
}