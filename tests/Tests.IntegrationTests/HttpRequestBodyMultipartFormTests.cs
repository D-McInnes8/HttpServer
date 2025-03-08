using System.Net;
using System.Text;
using HttpServer;
using HttpServer.Body;
using HttpServer.Response;
using HttpServer.Routing;
using Tests.IntegrationTests.TestExtensions;

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
        using var content = new MultipartFormDataContent();
        
        // Act
        var actual = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal("multipart", actual.ContentType?.Type);
            Assert.Equal("form-data", actual.ContentType?.SubType);
        });
    }

    [Fact]
    public async Task HttpRequestBodyMultipartFormData_ContentType_ShouldSetBoundary()
    {
        // Arrange
        const string expected = "----WebKitFormBoundary7MA4YWxkTrZu0gW";
        using var content = new MultipartFormDataContent(boundary: expected);
        
        // Act
        var actual = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        Assert.Equal(expected, actual.ContentType?.Boundary);
    }

    [Fact]
    public async Task HttpRequestBodyMultipartFormData_EmptyBoundary_ShouldReturnBadRequest()
    {
        // Arrange
        _server.MapPost("/test", _ => HttpResponse.Ok());
        using var content = new MultipartFormDataContent(boundary: "");
        
        // Act
        var actual = await _httpClient.PostAsync("/test", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_EmptyContent_ShouldContainNoParts()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
        Assert.Empty(body);
    }

    [Fact]
    public async Task HttpRequestBodyMultipartFormData_PartNoName()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Hello, World!", Encoding.UTF8));
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
        var actual = Assert.IsType<StringBodyContent>(Assert.Single(body));
        Assert.Equal("Hello, World!", actual.GetStringContent());
    }
    
    [Theory]
    [InlineData("ascii", "Hello, World!")]
    [InlineData("utf-8", "Hello, World!")]
    [InlineData("utf-16", "Hello, World!")]
    [InlineData("utf-32", "Hello, World!")]
    public async Task HttpRequestBodyMultipartFormData_PlainTextContent_ShouldSetBody(string charset, string expected)
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(expected, Encoding.GetEncoding(charset)), "text");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
        var part = Assert.IsType<StringBodyContent>(body["text"]);
        var actual = part.GetStringContent();
        Assert.Equal(expected, actual);
    }
    
    [Theory]
    [InlineData("us-ascii", "Hello, World!")]
    [InlineData("utf-8", "Hello, World!")]
    [InlineData("utf-16", "Hello, World!")]
    [InlineData("utf-32", "Hello, World!")]
    public async Task HttpRequestBodyMultipartFormData_PlainTextContent_ShouldSetPartCharset(string charset, string expected)
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(expected, Encoding.GetEncoding(charset)), "text");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
        var part = Assert.IsType<StringBodyContent>(body["text"]);
        Assert.Equal(charset, part.ContentType.Charset);
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_MultipleParts_ShouldSetMultipleParts()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Hello, World!", Encoding.UTF8), "text");
        content.Add(new StringContent("Hello, 世界!", Encoding.UTF8), "text2");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
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
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Hello, World!", Encoding.ASCII), "text");
        content.Add(new StringContent("Hello, 世界!", Encoding.UTF8), "text2");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
        
        var part = Assert.IsType<StringBodyContent>(body["text"]);
        Assert.Equal("us-ascii", part.ContentType.Charset);
        
        var part2 = Assert.IsType<StringBodyContent>(body["text2"]);
        Assert.Equal("utf-8", part2.ContentType.Charset);
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_BinaryContent_ShouldSetBody()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary");
        var expected = new byte[] { 0x01, 0x02, 0x03 };
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
        var part = Assert.IsType<ByteArrayBodyContent>(body["binary"]);
        var actual = part.Content;
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_BinaryContent_ShouldSetPartContentType()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
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
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
        var part = Assert.IsType<ByteArrayBodyContent>(body["binary"]);
        Assert.Null(part.ContentType.Charset);
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_BinaryContent_ShouldSetPartContentDisposition()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
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
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary", "file.bin");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
        var part = Assert.IsType<ByteArrayBodyContent>(body["binary"]);
        Assert.Equal("file.bin", part.ContentDisposition?.FileName);
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_BinaryContent_ShouldSetPartContentDispositionFileNameAndName()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary", "file.bin");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
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
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Hello, World!", Encoding.UTF8), "text");
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary", "file.bin");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var actual = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
        Assert.Multiple(() =>
        {
            Assert.Equal(2, actual.Count);
            Assert.True(actual.Contains("text"));
            Assert.True(actual.Contains("binary"));
        });
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_MultipleParts_ShouldMatchRequestContent()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Hello, World!", Encoding.UTF8), "text");
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary", "file.bin");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
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
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Hello, World!", Encoding.UTF8), "text");
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary", "file.bin");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
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
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Hello, World!", Encoding.UTF8), "text");
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary", "file.bin");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
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
    public async Task HttpRequestBodyMultipartFormData_MultipleParts_ShouldBeAbleToEnumerateParts()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Hello, World!", Encoding.UTF8), "text");
        content.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary", "file.bin");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
        Assert.Collection(body,
            part => Assert.IsType<StringBodyContent>(part),
            part => Assert.IsType<ByteArrayBodyContent>(part));
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_NestedMultipartContent_ShouldSetNestedContent()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        using var nestedContent = new MultipartFormDataContent();
        nestedContent.Add(new StringContent("Hello, World!", Encoding.UTF8), "text");
        content.Add(nestedContent, "nested");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
        var nestedPart = Assert.IsType<MultipartFormDataBodyContent>(body["nested"]);
        var stringPart = Assert.IsType<StringBodyContent>(nestedPart["text"]);
        Assert.Equal("Hello, World!", stringPart.GetStringContent());
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_NestedMultipartContent_ShouldSupportMultipleNestedParts()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        using var nestedContent = new MultipartFormDataContent();
        nestedContent.Add(new StringContent("Hello, World!", Encoding.UTF8), "text");
        nestedContent.Add(new ByteArrayContent([0x01, 0x02, 0x03]), "binary", "file.bin");
        content.Add(nestedContent, "nested");
        content.Add(new StringContent("Hello, World!", Encoding.UTF8), "text");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
        
        Assert.Equal(2, body.Count);
        var rootPart = Assert.IsType<StringBodyContent>(body["text"]);
        Assert.Equal("Hello, World!", rootPart.GetStringContent());
        
        var nestedPart = Assert.IsType<MultipartFormDataBodyContent>(body["nested"]);
        Assert.Multiple(() =>
        {
            Assert.Equal(2, nestedPart.Count);
            var nestedStringPart = Assert.IsType<StringBodyContent>(nestedPart["text"]);
            Assert.Equal("Hello, World!", nestedStringPart.GetStringContent());
            
            var nestedBinaryPart = Assert.IsType<ByteArrayBodyContent>(nestedPart["binary"]);
            Assert.Equal(new byte[] { 0x01, 0x02, 0x03 }, nestedBinaryPart.Content);
        });
    }
    
    [Fact]
    public async Task HttpRequestBodyMultipartFormData_LargeContent_ShouldSetContent()
    {
        // Arrange
        var expected = new byte[1024 * 1024];
        new Random().NextBytes(expected);
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(expected), "binary");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var body = Assert.IsType<MultipartFormDataBodyContent>(request.Body);
        var part = Assert.IsType<ByteArrayBodyContent>(body["binary"]);
        var actual = part.Content;
        Assert.Equal(expected, actual);
    }
}