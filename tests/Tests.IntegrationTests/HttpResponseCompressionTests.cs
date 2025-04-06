using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using HttpServer;
using HttpServer.Plugins.ResponseCompression.DependencyInjection;
using HttpServer.Response;
using HttpServer.Routing;

namespace Tests.IntegrationTests;

public class HttpResponseCompressionTests : IAsyncLifetime
{
    private readonly IHttpWebServer _server = HttpWebServer.CreateBuilder(0).Build();
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task InitializeAsync()
    {
        _server.ConfigureGlobalPipeline(pipeline =>
        {
            pipeline.UseResponseCompression();
        });
        await _server.StartAsync();
        _httpClient.BaseAddress = new Uri($"http://localhost:{_server.Port}");
    }

    public async Task DisposeAsync()
    {
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
        var request = new HttpRequestMessage(HttpMethod.Get, "/test");
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        
        // Act
        using var response = await _httpClient.SendAsync(request);
        
        // Assert
        using var memoryStream = new MemoryStream();
        await using var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);
        await gZipStream.WriteAsync("Compressed!"u8.ToArray());
        
        var expected = memoryStream.ToArray();
        var actual = await response.Content.ReadAsByteArrayAsync();
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
        using var response = await _httpClient.SendAsync(request);
        
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
        Assert.Equal(10, response.Content.Headers.ContentLength);
    }
}