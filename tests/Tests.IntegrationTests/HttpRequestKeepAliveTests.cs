using System.Net.Sockets;
using System.Text;
using HttpServer;
using HttpServer.Response;
using HttpServer.Routing;

namespace Tests.IntegrationTests;

public class HttpRequestKeepAliveTests : IAsyncLifetime
{
    private readonly IHttpWebServer _server = HttpWebServer.CreateBuilder(9999).Build();
    private TcpClient? _tcpClient;
    private NetworkStream? _networkStream;

    public async Task InitializeAsync()
    {
        await _server.StartAsync();
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
        await _server.StopAsync();
    }

    [Theory]
    [InlineData("keep-alive", true)]
    [InlineData("close", false)]
    public async Task HttpRequest_KeepAliveHeader_ResponseShouldHaveConnectionHeader(string connectionHeader, bool shouldKeepAlive)
    {
        // Arrange
        _server.MapGet("/test", _ => HttpResponse.Ok());

        // Act
        var request = $"GET /test HTTP/1.1\r\nHost: localhost\r\nConnection: {connectionHeader}\r\n\r\n";
        var requestBytes = Encoding.ASCII.GetBytes(request);
        await _networkStream!.WriteAsync(requestBytes, 0, requestBytes.Length);

        var response = await ReadResponseAsync();

        // Assert
        Assert.Contains("HTTP/1.1 200 OK", response);
        Assert.Equal(shouldKeepAlive, response.Contains("Connection: keep-alive"));
    }

    [Fact]
    public async Task HttpRequest_NoKeepAliveHeader_ResponseShouldNotHaveConnectionHeader()
    {
        // Arrange
        _server.MapGet("/test", _ => HttpResponse.Ok());

        // Act
        var request = "GET /test HTTP/1.1\r\nHost: localhost\r\n\r\n";
        var requestBytes = Encoding.ASCII.GetBytes(request);
        await _networkStream!.WriteAsync(requestBytes, 0, requestBytes.Length);

        var response = await ReadResponseAsync();

        // Assert
        Assert.Contains("HTTP/1.1 200 OK", response);
        Assert.DoesNotContain("Connection: keep-alive", response);
    }

    [Fact]
    public async Task HttpRequest_KeepAliveHeaderWithMultipleRequests_ShouldKeepConnectionOpen()
    {
        // Arrange
        _server.MapGet("/test", _ => HttpResponse.Ok());

        // Act
        var request = "GET /test HTTP/1.1\r\nHost: localhost\r\nConnection: keep-alive\r\n\r\n";
        var requestBytes = Encoding.ASCII.GetBytes(request);
        await _networkStream!.WriteAsync(requestBytes, 0, requestBytes.Length);

        var response1 = await ReadResponseAsync();

        await _networkStream.WriteAsync(requestBytes, 0, requestBytes.Length);
        var response2 = await ReadResponseAsync();

        // Assert
        Assert.Contains("HTTP/1.1 200 OK", response1);
        Assert.Contains("Connection: keep-alive", response1);

        Assert.Contains("HTTP/1.1 200 OK", response2);
        Assert.Contains("Connection: keep-alive", response2);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    public async Task HttpRequest_KeepAliveHeaderWithTimeout_ShouldSetTimeoutParameter(int timeout)
    {
        // Arrange
        _server.Options.KeepAlive.Timeout = TimeSpan.FromSeconds(timeout);
        _server.MapGet("/test", _ => HttpResponse.Ok());
        
        // Act
        var request = $"GET /test HTTP/1.1\r\nHost: localhost\r\nConnection: keep-alive\r\n\r\n";
        var requestBytes = Encoding.ASCII.GetBytes(request);
        await _networkStream!.WriteAsync(requestBytes, 0, requestBytes.Length);
        
        var response = await ReadResponseAsync();
        
        // Assert
        Assert.Contains("HTTP/1.1 200 OK", response);
        Assert.Contains("Connection: keep-alive", response);
        Assert.Contains($"timeout={timeout}", response);
    }
    
    [Fact]
    public async Task HttpRequest_KeepAliveHeaderWithTimeout_ShouldCloseConnectionAfterTimeout()
    {
        // Arrange
        _server.Options.KeepAlive.Timeout = TimeSpan.FromMilliseconds(100);
        _server.MapGet("/test", _ => HttpResponse.Ok());
        
        // Act
        var request = $"GET /test HTTP/1.1\r\nHost: localhost\r\nConnection: keep-alive\r\n\r\n";
        var requestBytes = Encoding.ASCII.GetBytes(request);
        await _networkStream!.WriteAsync(requestBytes, 0, requestBytes.Length);
        
        var response1 = await ReadResponseAsync();
        
        await Task.Delay(200);
        
        await _networkStream.WriteAsync(requestBytes, 0, requestBytes.Length);
        var response2 = await ReadResponseAsync();
        
        // Assert
        Assert.Contains("HTTP/1.1 200 OK", response1);
        Assert.Contains("Connection: keep-alive", response1);

        Assert.Contains("HTTP/1.1 200 OK", response2);
        Assert.DoesNotContain("Connection: keep-alive", response2);
    }
    
    [Theory]
    [InlineData(5, 100)]
    [InlineData(10, 200)]
    [InlineData(15, 300)]
    public async Task HttpRequest_KeepAliveHeaderWithMaxRequests_ShouldSetParameters(int timeout, int maxRequests)
    {
        // Arrange
        _server.Options.KeepAlive.Timeout = TimeSpan.FromSeconds(timeout);
        _server.Options.KeepAlive.MaxRequests = maxRequests;
        _server.MapGet("/test", _ => HttpResponse.Ok());
        
        // Act
        var request = $"GET /test HTTP/1.1\r\nHost: localhost\r\nConnection: keep-alive\r\n\r\n";
        var requestBytes = Encoding.ASCII.GetBytes(request);
        await _networkStream!.WriteAsync(requestBytes, 0, requestBytes.Length);
        
        var response = await ReadResponseAsync();
        
        // Assert
        Assert.Contains("HTTP/1.1 200 OK", response);
        Assert.Contains("Connection: keep-alive", response);
        Assert.Contains($"timeout={timeout}, max={maxRequests}", response);
    }
    
    [Fact]
    public async Task HttpRequest_KeepAliveHeaderWithMaxRequests_ShouldCloseConnectionAfterMaxRequests()
    {
        // Arrange
        _server.Options.KeepAlive.Timeout = TimeSpan.FromSeconds(5);
        _server.Options.KeepAlive.MaxRequests = 2;
        _server.MapGet("/test", _ => HttpResponse.Ok());
        
        // Act
        const string request = $"GET /test HTTP/1.1\r\nHost: localhost\r\nConnection: keep-alive\r\n\r\n";
        var requestBytes = Encoding.ASCII.GetBytes(request);
        
        await _networkStream!.WriteAsync(requestBytes, 0, requestBytes.Length);
        var response1 = await ReadResponseAsync();
        
        await _networkStream.WriteAsync(requestBytes, 0, requestBytes.Length);
        var response2 = await ReadResponseAsync();
        
        await _networkStream.WriteAsync(requestBytes, 0, requestBytes.Length);
        var response3 = await ReadResponseAsync();
        
        // Assert
        Assert.Contains("HTTP/1.1 200 OK", response1);
        Assert.Contains("Connection: keep-alive", response1);

        Assert.Contains("HTTP/1.1 200 OK", response2);
        Assert.Contains("Connection: keep-alive", response2);

        Assert.Empty(response3);
    }

    private async Task<string> ReadResponseAsync()
    {
        var buffer = new byte[4096];
        var bytesRead = await _networkStream!.ReadAsync(buffer, 0, buffer.Length);
        return Encoding.ASCII.GetString(buffer, 0, bytesRead);
    }
}