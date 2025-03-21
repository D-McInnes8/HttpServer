using System.Net.Sockets;
using System.Text;
using HttpServer;
using HttpServer.Body;
using HttpServer.Request;
using HttpServer.Response;
using HttpServer.Routing;
using Tests.IntegrationTests.TestExtensions;

namespace Tests.IntegrationTests;

public class HttpRequestBodyStringTests : IAsyncLifetime
{
    private readonly IHttpWebServer _server = HttpWebServer.CreateBuilder(0).Build();
    private readonly HttpClient _httpClient = new HttpClient();
    private TcpClient? _tcpClient;
    private NetworkStream? _networkStream;

    public async Task InitializeAsync()
    {
        await _server.StartAsync();
        _httpClient.BaseAddress = new Uri($"http://localhost:{_server.Port}");
        _tcpClient = new TcpClient("localhost", _server.Port);
        _networkStream = _tcpClient.GetStream();
    }

    public async Task DisposeAsync()
    {
        if (_networkStream is not null)
        {
            await _networkStream.DisposeAsync();
        }
        _tcpClient?.Dispose();
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
        using var content = new StringContent(expected, Encoding.UTF8);
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var actual = Assert.IsType<StringBodyContent>(request.Body);
        Assert.Equal(expected, actual.GetStringContent());
    }
    
    [Fact]
    public async Task HttpRequestBodyString_EmptyContent_ShouldReturnEmptyBody()
    {
        // Arrange
        using var content = new StringContent(string.Empty); 

        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/api/test-empty", content);

        // Assert
        var actual = Assert.IsType<StringBodyContent>(request.Body);
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
        using var content = new StringContent(expectedContent, encoding);

        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/api/test-encoding", content);

        // Assert
        var actual = Assert.IsType<StringBodyContent>(request.Body);
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
        using var content = new StringContent("Hello, World!", encoding);

        // Act
        var actual = await _server.PostAsyncAndCaptureRequest("/api/test-encoding", content);

        // Assert
        Assert.NotNull(actual.Body);
        Assert.Equal(encoding.WebName, actual.Body.ContentType.Charset);
    }

    [Fact]
    public async Task HttpRequestBodyString_NoCharset_ShouldDefaultToAscii()
    {
        // Arrange
        HttpRequest? actual = null;
        _server.MapPost("/api/test-encoding", ctx =>
        {
            actual = ctx.Request;
            return HttpResponse.Ok();
        });
        var request = new StringBuilder()
            .AppendLine("POST /api/test-encoding HTTP/1.1")
            .AppendLine("Host: localhost")
            .AppendLine("Content-Type: text/plain")
            .AppendLine("Content-Length: 13")
            .AppendLine()
            .AppendLine("Hello, World!")
            .ToString();
        var requestBytes = Encoding.ASCII.GetBytes(request);
        
        // Act
        await _networkStream!.WriteAsync(requestBytes, 0, requestBytes.Length);
        _ = await ReadResponseAsync();
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.NotNull(actual?.Body);
            Assert.Null(actual?.ContentType?.Charset);
        });
    }
    
    private async Task<string> ReadResponseAsync()
    {
        var buffer = new byte[4096];
        var bytesRead = await _networkStream!.ReadAsync(buffer, 0, buffer.Length);
        return Encoding.ASCII.GetString(buffer, 0, bytesRead);
    }
}