using System.Net.Sockets;
using System.Text;
using HttpServer;
using HttpServer.Request;
using HttpServer.Response;
using HttpServer.Routing;

namespace Tests.IntegrationTests;

public class HttpRequestHeaderTests : IAsyncLifetime
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
    [InlineData("Accept-Language", "en-US")]
    [InlineData("Authorization", "Bearer 123456")]
    [InlineData("Accept", "text/html")]
    [InlineData("Host", "localhost:9999")]
    [InlineData("Referer", "http://example.com/")]
    [InlineData("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)")]
    [InlineData("X-Custom-Header", "CustomValue")]
    [InlineData("X-Number-Header", "1234567890")]
    [InlineData("X-Space-Header", "Value with spaces")]
    [InlineData("X-Special-Header", "Value-With-Special_Characters!@#")]
    public async Task HttpRequestHeader_SingleHeader_ShouldParseHeader(string headerName, string headerValue)
    {
        // Arrange
        HttpRequest? actual = null;
        _server.MapGet("/test", ctx =>
        {
            actual = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, "/test");
        message.Headers.Add(headerName, headerValue);
        _ = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Equal(headerValue, actual?.Headers[headerName]);
    }
    
    [Fact]
    public async Task HttpRequestHeader_MultipleDifferentHeaders_ShouldParseHeaders()
    {
        // Arrange
        HttpRequest? actual = null;
        _server.MapGet("/test", ctx =>
        {
            actual = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, "/test");
        message.Headers.Add("Accept-Language", "en-US");
        message.Headers.Add("Authorization", "Bearer 123456");
        _ = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal("en-US", actual?.Headers["Accept-Language"]);
            Assert.Equal("Bearer 123456", actual?.Headers["Authorization"]);
        });
    }
    
    [Fact]
    public async Task HttpRequestHeader_MultipleHeadersWithSameName_ShouldParseHeaders()
    {
        // Arrange
        HttpRequest? actual = null;
        _server.MapGet("/test", ctx =>
        {
            actual = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, "/test");
        message.Headers.Add("Accept-Language", "en-US");
        message.Headers.Add("Accept-Language", "en-GB");
        message.Headers.Add("Accept-Language", "en-CA");
        _ = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Equal("en-US, en-GB, en-CA", actual?.Headers["Accept-Language"]);
    }
    
    [Fact]
    public async Task HttpRequestHeader_MultipleHeadersWithSameNameDifferentCasing_ShouldParseHeaders()
    {
        // Arrange
        HttpRequest? actual = null;
        _server.MapGet("/test", ctx =>
        {
            actual = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Get, "/test");
        message.Headers.Add("Accept-Language", "en-US");
        message.Headers.Add("accept-language", "en-GB");
        message.Headers.Add("ACCEPT-LANGUAGE", "en-CA");
        _ = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.Equal("en-US, en-GB, en-CA", actual?.Headers["Accept-Language"]);
    }

    [Fact]
    public async Task HttpRequestHeader_MultipleHeaderLines_ShouldParseHeaders()
    {
        // Arrange
        HttpRequest? actual = null;
        _server.MapGet("/test", ctx =>
        {
            actual = ctx.Request;
            return HttpResponse.Ok();
        });

        var request = new StringBuilder()
            .AppendLine("GET /test HTTP/1.1")
            .AppendLine("Host: localhost")
            .AppendLine("Accept-Language: en-US")
            .AppendLine("Accept-Language: en-GB")
            .AppendLine("Accept-Language: en-CA")
            .AppendLine()
            .ToString();
        var bytes = Encoding.ASCII.GetBytes(request);
        
        // Act
        await _networkStream!.WriteAsync(bytes, 0, bytes.Length);
        _ = await ReadResponseAsync();
        
        // Assert
        Assert.Equal("en-US,en-GB,en-CA", actual?.Headers["Accept-Language"]);
    }
    
    private async Task<string> ReadResponseAsync()
    {
        var buffer = new byte[4096];
        var bytesRead = await _networkStream!.ReadAsync(buffer, 0, buffer.Length);
        return Encoding.ASCII.GetString(buffer, 0, bytesRead);
    }
}