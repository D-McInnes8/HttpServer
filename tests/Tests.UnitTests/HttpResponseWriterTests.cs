using Application;
using Application.Response;
using Application.Response.Internal;

namespace Tests.UnitTests;

public class HttpResponseWriterTests
{
    [Fact]
    public void WriteResponse_WithStatusCodeOnly_WritesCorrectResponse()
    {
        // Arrange
        var response = new HttpResponse(HttpResponseStatusCode.OK)
        {
            HttpVersion = "HTTP/1.1"
        };

        // Act
        var result = HttpResponseWriter.WriteResponse(response);

        // Assert
        var expected = "HTTP/1.1 200 OK\r\n\r\n";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void WriteResponse_WithHeaders_WritesCorrectResponse()
    {
        // Arrange
        var response = new HttpResponse(HttpResponseStatusCode.OK)
        {
            HttpVersion = "HTTP/1.1",
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/plain" },
                { "Connection", "keep-alive" }
            }
        };

        // Act
        var result = HttpResponseWriter.WriteResponse(response);

        // Assert
        var expected = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nConnection: keep-alive\r\n\r\n";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void WriteResponse_WithBody_WritesCorrectResponse()
    {
        // Arrange
        var body = new HttpBody
        {
            ContentType = "text/plain",
            Content = "Hello, World!"
        };
        var response = new HttpResponse(HttpResponseStatusCode.OK, body)
        {
            HttpVersion = "HTTP/1.1"
        };

        // Act
        var result = HttpResponseWriter.WriteResponse(response);

        // Assert
        var expected = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: 13\r\n\r\nHello, World!";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void WriteResponse_WithDifferentStatusCode_WritesCorrectResponse()
    {
        // Arrange
        var response = new HttpResponse(HttpResponseStatusCode.NotFound)
        {
            HttpVersion = "HTTP/1.1"
        };

        // Act
        var result = HttpResponseWriter.WriteResponse(response);

        // Assert
        var expected = "HTTP/1.1 404 NotFound\r\n\r\n";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void WriteResponse_WithDifferentContentTypes_WritesCorrectResponse()
    {
        // Arrange
        var body = new HttpBody
        {
            ContentType = "application/json",
            Content = "{\"key\":\"value\"}"
        };
        var response = new HttpResponse(HttpResponseStatusCode.OK, body)
        {
            HttpVersion = "HTTP/1.1"
        };

        // Act
        var result = HttpResponseWriter.WriteResponse(response);

        // Assert
        var expected = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: 15\r\n\r\n{\"key\":\"value\"}";
        Assert.Equal(expected, result);
    }
    [Fact]
    public void WriteResponse_WithEmptyHeaders_WritesCorrectResponse()
    {
        // Arrange
        var response = new HttpResponse(HttpResponseStatusCode.OK)
        {
            HttpVersion = "HTTP/1.1",
            Headers = new Dictionary<string, string>()
        };

        // Act
        var result = HttpResponseWriter.WriteResponse(response);

        // Assert
        var expected = "HTTP/1.1 200 OK\r\n\r\n";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void WriteResponse_WithMultipleHeaders_WritesCorrectResponse()
    {
        // Arrange
        var response = new HttpResponse(HttpResponseStatusCode.OK)
        {
            HttpVersion = "HTTP/1.1",
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/html" },
                { "Connection", "close" },
                { "Cache-Control", "no-cache" }
            }
        };

        // Act
        var result = HttpResponseWriter.WriteResponse(response);

        // Assert
        var expected = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\nConnection: close\r\nCache-Control: no-cache\r\n\r\n";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void WriteResponse_WithEmptyBody_WritesCorrectResponse()
    {
        // Arrange
        var response = new HttpResponse(HttpResponseStatusCode.OK)
        {
            HttpVersion = "HTTP/1.1",
            Body = new HttpBody
            {
                ContentType = "text/plain",
                Content = string.Empty
            }
        };

        // Act
        var result = HttpResponseWriter.WriteResponse(response);

        // Assert
        var expected = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: 0\r\n\r\n";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void WriteResponse_WithJsonBody_WritesCorrectResponse()
    {
        // Arrange
        var body = new HttpBody
        {
            ContentType = "application/json",
            Content = "{\"message\":\"success\"}"
        };
        var response = new HttpResponse(HttpResponseStatusCode.OK, body)
        {
            HttpVersion = "HTTP/1.1"
        };

        // Act
        var result = HttpResponseWriter.WriteResponse(response);

        // Assert
        var expected = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: 21\r\n\r\n{\"message\":\"success\"}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void WriteResponse_WithHtmlBody_WritesCorrectResponse()
    {
        // Arrange
        var body = new HttpBody
        {
            ContentType = "text/html",
            Content = "<html><body>Hello</body></html>"
        };
        var response = new HttpResponse(HttpResponseStatusCode.OK, body)
        {
            HttpVersion = "HTTP/1.1"
        };

        // Act
        var result = HttpResponseWriter.WriteResponse(response);

        // Assert
        var expected = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\nContent-Length: 31\r\n\r\n<html><body>Hello</body></html>";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void WriteResponse_WithCustomStatusCode_WritesCorrectResponse()
    {
        // Arrange
        var response = new HttpResponse((HttpResponseStatusCode)418)
        {
            HttpVersion = "HTTP/1.1"
        };

        // Act
        var result = HttpResponseWriter.WriteResponse(response);

        // Assert
        var expected = "HTTP/1.1 418 418\r\n\r\n";
        Assert.Equal(expected, result);
    }
}