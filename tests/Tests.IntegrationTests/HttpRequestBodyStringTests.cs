using System.Text;
using HttpServer;
using HttpServer.Request;
using HttpServer.Response;
using HttpServer.Response.Body;
using HttpServer.Routing;

namespace Tests.IntegrationTests;

public class HttpRequestBodyStringTests : IAsyncLifetime
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
    public async Task HttpRequestBodyString_PlainTextContent_ShouldSetBody(string expected)
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        using var content = new StringContent(expected, Encoding.UTF8);
        _ = await _httpClient.PostAsync("/test", content);
        
        // Assert
        var actual = Assert.IsType<StringBodyContent>(request?.Body);
        Assert.Equal(expected, actual.GetStringContent());
    }
    
    [Fact]
    public async Task HttpRequestBodyString_EmptyContent_ShouldReturnEmptyBody()
    {
        // Arrange
        HttpRequest? request = null;
        _server.MapPost("/api/test-empty", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });

        // Act
        using var content = new StringContent(""); 
        _ = await _httpClient.PostAsync("/api/test-empty", content);

        // Assert
        var actual = Assert.IsType<StringBodyContent>(request?.Body);
        Assert.Equal(0, request.Body.Length);
        Assert.Equal(string.Empty, actual.GetStringContent());
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
    public async Task HttpRequestBodyString_Encodings_ShouldEncodeBody(string encodingName, string expectedContent)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        HttpRequest? request = null;
        _server.MapPost("/api/test-encoding", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });

        // Act
        using var content = new StringContent(expectedContent, encoding);
        _ = await _httpClient.PostAsync("/api/test-encoding", content);

        // Assert
        var actual = Assert.IsType<StringBodyContent>(request?.Body);
        Assert.Equal(expectedContent, actual.GetStringContent());
    }
    
    [Theory]
    [InlineData("utf-8")]
    [InlineData("utf-16")]
    [InlineData("ascii")]
    public async Task HttpRequestBodyString_Encodings_ShouldSetCharset(string encodingName)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        HttpRequest? actual = null;
        _server.MapPost("/api/test-encoding", ctx =>
        {
            actual = ctx.Request;
            return HttpResponse.Ok();
        });

        // Act
        using var content = new StringContent("Hello, World!", encoding);
        _ = await _httpClient.PostAsync("/api/test-encoding", content);

        // Assert
        Assert.NotNull(actual?.Body);
        Assert.Equal(encoding.WebName, actual?.Body.ContentType.Charset);
    }
}