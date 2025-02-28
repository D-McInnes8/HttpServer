using System.Net;
using System.Text;
using HttpServer;
using HttpServer.Body;
using HttpServer.Response;
using HttpServer.Routing;

namespace Tests.IntegrationTests;

public class HttpResponseBodyStringTests : IAsyncLifetime
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
    
    [Theory]
    [InlineData("Hello, World!")]
    [InlineData("Hello, World! 1234567890")]
    [InlineData("Hello, World! With Special Characters!@#")]
    [InlineData("Hello, World! With Spaces")]
    [InlineData("Hello, World! With New Line\n")]
    [InlineData("Hello, World! With Carriage Return\r")]
    [InlineData("Hello, World! With Tab\t")]
    [InlineData("Hello, World! With Backspace\b")]
    [InlineData("Hello, World! With Form Feed\f")]
    [InlineData("Hello, World! With Vertical Tab\v")]
    public async Task HttpResponseBodyString_PlainTextContent_ShouldSetBody(string expected)
    {
        // Arrange
        _server.MapGet("/test", _ => HttpResponse.Ok(expected));
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        var actual = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task HttpResponseBodyString_EmptyContent_ShouldReturnEmptyBody()
    {
        // Arrange
        _server.MapGet("/api/test-empty", _ =>
        {
            return HttpResponse.Ok(string.Empty);
        });

        // Act
        var response = await _httpClient.GetAsync("/api/test-empty");
        var actual = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(string.Empty, actual);
    }

    [Theory]
    [InlineData("utf-8", "Hello, World!")]
    [InlineData("utf-8", "Hello, 世界!")]
    [InlineData("utf-8", "こんにちは世界")]
    [InlineData("utf-8", "Привет, мир!")]
    [InlineData("utf-8", "Hola, mundo!")]
    [InlineData("utf-8", "Bonjour, le monde!")]
    [InlineData("utf-8", "Hallo, Welt!")]
    [InlineData("utf-8", "안녕하세요 세계")]
    [InlineData("utf-8", "مرحبا بالعالم")]
    [InlineData("utf-8", "שלום עולם")]
    [InlineData("utf-16", "Hello, World!")]
    [InlineData("utf-16", "Hello, 世界!")]
    [InlineData("utf-16", "こんにちは世界")]
    [InlineData("utf-16", "Привет, мир!")]
    [InlineData("utf-16", "Hola, mundo!")]
    [InlineData("utf-16", "Bonjour, le monde!")]
    [InlineData("utf-16", "Hallo, Welt!")]
    [InlineData("utf-16", "안녕하세요 세계")]
    [InlineData("utf-16", "مرحبا بالعالم")]
    [InlineData("utf-16", "שלום עולם")]
    [InlineData("ascii", "Hello, World!")]
    [InlineData("ascii", "Hola, mundo!")]
    [InlineData("ascii", "Bonjour, le monde!")]
    [InlineData("ascii", "Hallo, Welt!")]
    public async Task HttpResponseBodyString_Encodings_ShouldEncodeBody(string encodingName, string expectedContent)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        _server.MapGet("/api/test-encoding", _ =>
        {
            return HttpResponse.Ok(new StringBodyContent(expectedContent, encoding));
        });

        // Act
        var response = await _httpClient.GetAsync("/api/test-encoding");
        var actualBytes = await response.Content.ReadAsByteArrayAsync();
        var actualContent = encoding.GetString(actualBytes);
        var expectedBytes = encoding.GetBytes(expectedContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedBytes, actualBytes);
        Assert.Equal(expectedContent, actualContent);
    }
    
    [Theory]
    [InlineData("utf-8")]
    [InlineData("utf-16")]
    [InlineData("ascii")]
    public async Task HttpResponseBodyString_Encodings_ShouldSetCharset(string encodingName)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        _server.MapGet("/api/test-encoding", _ =>
        {
            return HttpResponse.Ok(new StringBodyContent("Hello, World!", encoding));
        });

        // Act
        var response = await _httpClient.GetAsync("/api/test-encoding");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(encoding.WebName, response.Content.Headers.ContentType?.CharSet);
    }
}