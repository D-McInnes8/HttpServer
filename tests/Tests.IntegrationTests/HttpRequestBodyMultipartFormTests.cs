using System.Net;
using System.Text;
using HttpServer;
using HttpServer.Request;
using HttpServer.Response;
using HttpServer.Response.Body;
using HttpServer.Routing;

namespace Tests.IntegrationTests;

public class HttpRequestBodyMultipartFormTests : IAsyncLifetime
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
    public async Task HttpRequestBodyMultipartFormData_ContentType_ShouldSetContentTypeAndSubType()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new MultipartFormDataContent();
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal("multipart", request?.ContentType?.Type);
            Assert.Equal("form-data", request?.ContentType?.SubType);
        });
    }

    [Fact]
    public async Task HttpRequestBodyMultipartFormData_ContentType_ShouldSetBoundary()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        const string expected = "----WebKitFormBoundary7MA4YWxkTrZu0gW";
        
        // Act
        using var content = new MultipartFormDataContent(boundary: expected);
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        Assert.Equal(expected, request?.ContentType?.Boundary);
    }

    [Fact]
    public async Task HttpRequestBodyMultipartFormData_EmptyBoundary_ShouldReturnBadRequest()
    {
        // Arrange
        _server.MapPost("/test", ctx => HttpResponse.Ok());
        
        // Act
        using var content = new MultipartFormDataContent(boundary: "");
        var actual = await _httpClient.PostAsync("/test", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_EmptyContent_ShouldContainNoParts()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new MultipartFormDataContent();
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        Assert.Empty(body);
    }
    
    [Theory]
    [InlineData("ascii", "Hello, World!")]
    [InlineData("utf-8", "Hello, World!")]
    [InlineData("utf-16", "Hello, World!")]
    [InlineData("utf-32", "Hello, World!")]
    public async Task HttpRequestBodyMultipartFormData_PlainTextContent_ShouldSetBody(string charset, string expected)
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(expected, Encoding.GetEncoding(charset)), "text");
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        var part = Assert.IsType<StringBodyContent>(body["text"]);
        var actual = part.GetStringContent();
        Assert.Equal(expected, actual);
    }
    
    [Theory]
    [InlineData("ascii", "Hello, World!")]
    [InlineData("utf-8", "Hello, World!")]
    [InlineData("utf-16", "Hello, World!")]
    [InlineData("utf-32", "Hello, World!")]
    public async Task HttpRequestBodyMultipartFormData_PlainTextContent_ShouldSetPartCharset(string charset, string expected)
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(expected, Encoding.GetEncoding(charset)), "text");
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        var part = Assert.IsType<StringBodyContent>(body["text"]);
        Assert.Equal(charset, part.ContentType.Charset);
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_MultipleParts_ShouldSetMultipleParts()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Hello, World!", Encoding.UTF8), "text");
        content.Add(new StringContent("Hello, 世界!", Encoding.UTF8), "text2");
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        Assert.Multiple(() =>
        {
            Assert.Equal(2, body.Count);
            Assert.True(body.Contains("text"));
            Assert.True(body.Contains("text2"));
        });
        
        var part = Assert.IsType<StringBodyContent>(body["text"]);
        Assert.Equal("Hello, World!", part.GetStringContent());
        
        var part2 = Assert.IsType<StringBodyContent>(body["text2"]);
        Assert.Equal("Hello, 世界!", part2.GetStringContent());
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_MultipleParts_ShouldSetMultiplePartCharsets()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Hello, World!", Encoding.ASCII), "text");
        content.Add(new StringContent("Hello, 世界!", Encoding.UTF8), "text2");
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        
        var part = Assert.IsType<StringBodyContent>(body["text"]);
        Assert.Equal("us-ascii", part.ContentType.Charset);
        
        var part2 = Assert.IsType<StringBodyContent>(body["text2"]);
        Assert.Equal("utf-8", part2.ContentType.Charset);
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_BinaryContent_ShouldSetBody()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        var expected = new byte[] { 0x01, 0x02, 0x03 };
        
        // Act
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary");
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        var part = Assert.IsType<ByteArrayBodyContent>(body["binary"]);
        var actual = part.Content;
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_BinaryContent_ShouldSetPartContentType()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary");
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        var part = Assert.IsType<ByteArrayBodyContent>(body["binary"]);
        Assert.Multiple(() =>
        {
            Assert.Equal("application", part.ContentType.Type);
            Assert.Equal("octet-stream", part.ContentType.SubType);
        });
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_BinaryContent_ShouldSetPartCharsetToNull()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary");
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        var part = Assert.IsType<ByteArrayBodyContent>(body["binary"]);
        Assert.Null(part.ContentType.Charset);
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_BinaryContent_ShouldSetPartContentDisposition()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary");
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        var part = Assert.IsType<ByteArrayBodyContent>(body["binary"]);
        Assert.Multiple(() =>
        {
            Assert.Equal("binary", part.ContentDisposition?.Name);
            Assert.Null(part.ContentDisposition?.FileName);
        });
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_BinaryContent_ShouldSetPartContentDispositionFileName()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary", "file.bin");
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        var part = Assert.IsType<ByteArrayBodyContent>(body["binary"]);
        Assert.Equal("file.bin", part.ContentDisposition?.FileName);
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_BinaryContent_ShouldSetPartContentDispositionFileNameAndName()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary", "file.bin");
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        var part = Assert.IsType<ByteArrayBodyContent>(body["binary"]);
        Assert.Multiple(() =>
        {
            Assert.Equal("binary", part.ContentDisposition?.Name);
            Assert.Equal("file.bin", part.ContentDisposition?.FileName);
        });
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_MultipleParts_ShouldIncludeAllPartsInBody()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Hello, World!", Encoding.UTF8), "text");
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary", "file.bin");
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        Assert.Multiple(() =>
        {
            Assert.Equal(2, body.Count);
            Assert.True(body.Contains("text"));
            Assert.True(body.Contains("binary"));
        });
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_MultipleParts_ShouldMatchRequestContent()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Hello, World!", Encoding.UTF8), "text");
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary", "file.bin");
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        var stringPart = Assert.IsType<StringBodyContent>(body["text"]);
        var binaryPart = Assert.IsType<ByteArrayBodyContent>(body["binary"]);
        Assert.Multiple(() =>
        {
            Assert.Equal("Hello, World!", stringPart.GetStringContent());
            Assert.Equal(new byte[] { 0x01, 0x02, 0x03 }, binaryPart.Content);
        });
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_MultipleParts_ShouldMatchRequestContentDisposition()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Hello, World!", Encoding.UTF8), "text");
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary", "file.bin");
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        var stringPart = Assert.IsType<StringBodyContent>(body["text"]);
        var binaryPart = Assert.IsType<ByteArrayBodyContent>(body["binary"]);
        Assert.Multiple(() =>
        {
            Assert.Equal("text", stringPart.ContentDisposition?.Name);
            Assert.Null(stringPart.ContentDisposition?.FileName);
            Assert.Equal("binary", binaryPart.ContentDisposition?.Name);
            Assert.Equal("file.bin", binaryPart.ContentDisposition?.FileName);
        });
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_MultipleParts_ShouldMatchRequestContentTypes()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Hello, World!", Encoding.UTF8), "text");
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary", "file.bin");
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        var stringPart = Assert.IsType<StringBodyContent>(body["text"]);
        var binaryPart = Assert.IsType<ByteArrayBodyContent>(body["binary"]);
        Assert.Multiple(() =>
        {
            Assert.Equal("text", stringPart.ContentType.Type);
            Assert.Equal("plain", stringPart.ContentType.SubType);
            Assert.Equal("utf-8", stringPart.ContentType.Charset);
            Assert.Equal("application", binaryPart.ContentType.Type);
            Assert.Equal("octet-stream", binaryPart.ContentType.SubType);
        });
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_NestedMultipartContent_ShouldSetNestedContent()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new MultipartFormDataContent();
        using var nestedContent = new MultipartFormDataContent();
        nestedContent.Add(new StringContent("Hello, World!", Encoding.UTF8), "text");
        content.Add(nestedContent, "nested");
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        var nestedPart = Assert.IsType<MultipartFormDataBodyContent>(body["nested"]);
        var stringPart = Assert.IsType<StringBodyContent>(nestedPart["text"]);
        Assert.Equal("Hello, World!", stringPart.GetStringContent());
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_LargeContent_ShouldSetContent()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        var expected = new byte[1024 * 1024];
        new Random().NextBytes(expected);
        
        // Act
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(expected), "binary");
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request?.Body);
        var part = Assert.IsType<ByteArrayBodyContent>(body["binary"]);
        var actual = part.Content;
        Assert.Equal(expected, actual);
    }
}