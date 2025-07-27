using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using HttpServer;
using HttpServer.Plugins.ResponseCompression.DependencyInjection;
using HttpServer.Response;
using HttpServer.Routing;

namespace Tests.IntegrationTests;

public class HttpResponseCompressionTests : IAsyncLifetime
{
    private readonly IHttpWebServer _server = HttpWebServer.CreateBuilder(0).Build();
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly HttpClient _httpClientWithDecompression = new HttpClient(
        new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli });
    private TcpClient? _tcpClient;
    private NetworkStream? _networkStream;

    public async Task InitializeAsync()
    {
        _server.ConfigureGlobalPipeline(pipeline =>
        {
            pipeline.UseResponseCompression();
        });
        await _server.StartAsync();
        _httpClient.BaseAddress = new Uri($"http://localhost:{_server.Port}");
        _httpClientWithDecompression.BaseAddress = new Uri($"http://localhost:{_server.Port}");
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
    [InlineData("gzip")]
    [InlineData("deflate")]
    [InlineData("br")]
    public async Task HttpResponseCompression_ShouldSetContentEncodingHeader(string expected)
    {
        // Arrange
        _server.MapGet("/test", _ => HttpResponse.Ok("Compressed!"));
        var request = new HttpRequestMessage(HttpMethod.Get, "/test");
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(expected));
        
        // Act
        using var response = await _httpClient.SendAsync(request);
        
        // Assert
        Assert.Contains(expected, response.Content.Headers.ContentEncoding);
    }

    [Theory]
    [InlineData("gzip")]
    [InlineData("deflate")]
    [InlineData("br")]
    public async Task HttpResponseCompression_ShouldSetVaryHeader(string encoding)
    {
        // Arrange
        _server.MapGet("/test", _ => HttpResponse.Ok("Compressed!"));
        var request = new HttpRequestMessage(HttpMethod.Get, "/test");
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(encoding));
        
        // Act
        using var response = await _httpClient.SendAsync(request);
        
        // Assert
        Assert.Contains("Accept-Encoding", response.Headers.Vary);
    }

    [Fact]
    public async Task HttpResponseCompression_WithGZipCompression_ShouldCompressBody()
    {
        // Arrange
        _server.MapGet("/test", _ => HttpResponse.Ok("Compressed!"));
        var request = "GET /test HTTP/1.1\r\nHost: localhost\r\nAccept-Encoding: gzip\r\n\r\n";
        var requestBytes = Encoding.ASCII.GetBytes(request);
        
        // Act
        await _networkStream!.WriteAsync(requestBytes, 0, requestBytes.Length);
        var buffer = new byte[8192];
        var bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
        
        // Assert
        using var memoryStream = new MemoryStream();
        await using var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);
        await gZipStream.WriteAsync("Compressed!"u8.ToArray());
        await gZipStream.FlushAsync();
        var expected = memoryStream.ToArray();

        var bodyStart = buffer.AsSpan(0, bytesRead).IndexOf("\r\n\r\n"u8) + 4;
        var actual = buffer[bodyStart..bytesRead];
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task HttpResponseCompression_WithDeflateCompression_ShouldCompressBody()
    {
        // Arrange
        _server.MapGet("/test", _ => HttpResponse.Ok("Compressed!"));
        var request = new HttpRequestMessage(HttpMethod.Get, "/test");
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        
        // Act
        using var response = await _httpClient.SendAsync(request);
        
        // Assert
        using var memoryStream = new MemoryStream();
        await using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress);
        await deflateStream.WriteAsync("Compressed!"u8.ToArray());
        await deflateStream.FlushAsync();
        
        var expected = memoryStream.ToArray();
        var actual = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task HttpResponseCompression_WithBrotliCompression_ShouldCompressBody()
    {
        // Arrange
        _server.MapGet("/test", _ => HttpResponse.Ok("Compressed!"));
        var request = new HttpRequestMessage(HttpMethod.Get, "/test");
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
        
        // Act
        using var response = await _httpClient.SendAsync(request);
        
        // Assert
        using var memoryStream = new MemoryStream();
        await using var brotliStream = new BrotliStream(memoryStream, CompressionMode.Compress);
        await brotliStream.WriteAsync("Compressed!"u8.ToArray());
        await brotliStream.FlushAsync();
        
        var expected = memoryStream.ToArray();
        var actual = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(expected, actual);
    }
    
    [Theory]
    [InlineData("gzip")]
    [InlineData("deflate")]
    [InlineData("br")]
    public async Task HttpResponseCompression_WithCompression_ShouldBeAbleToDecompressBody(string encoding)
    {
        // Arrange
        _server.MapGet("/test", _ => HttpResponse.Ok("Compressed!"));
        var request = new HttpRequestMessage(HttpMethod.Get, "/test");
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(encoding));
        
        // Act
        using var response = await _httpClientWithDecompression.SendAsync(request);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var actual = await response.Content.ReadAsStringAsync();
        Assert.Equal("Compressed!", actual);
    }
    
    [Fact]
    public async Task HttpResponseCompression_WithGzipCompression_ShouldCorrectlySetContentLengthHeader()
    {
        // Arrange
        _server.MapGet("/test", _ => HttpResponse.Ok("Compressed!"));
        var request = new HttpRequestMessage(HttpMethod.Get, "/test");
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        
        // Act
        using var response = await _httpClient.SendAsync(request);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(27, response.Content.Headers.ContentLength);
    }
}