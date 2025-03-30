using System.Net.Http.Headers;
using HttpServer;
using HttpServer.Response;
using HttpServer.Routing;
using Tests.IntegrationTests.TestExtensions;

namespace Tests.IntegrationTests;

public class HttpRequestCompressionTests: IAsyncLifetime
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
    [InlineData("gzip")]
    [InlineData("deflate")]
    [InlineData("br")]
    public async Task HttpRequestCompression_WithSingleEncoding_ShouldParseAcceptEncodingHeader(string expected)
    {
        // Arrange
        _server.MapGet("/test", _ => HttpResponse.Ok());
        
        // Act
        var originalRequest = new HttpRequestMessage(HttpMethod.Get, "/test");
        originalRequest.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(expected));
        var request = await _server.GetAsyncAndCaptureRequest(originalRequest);
        
        // Assert
        Assert.NotNull(request.AcceptEncoding);
        var actual = Assert.Single(request.AcceptEncoding.Encodings);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("gzip")]
    [InlineData("deflate")]
    [InlineData("br")]
    [InlineData("gzip,deflate")]
    [InlineData("gzip,br")]
    [InlineData("deflate,br")]
    [InlineData("gzip,deflate,br")]
    public async Task HttpRequestCompression_WithMultipleEncodings_ShouldParseAcceptEncodingHeaders(string encodings)
    {
        // Arrange
        _server.MapGet("/test", _ => HttpResponse.Ok());
        
        // Act
        var originalRequest = new HttpRequestMessage(HttpMethod.Get, "/test");
        foreach (var encoding in encodings.Split(","))
        {
            originalRequest.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(encoding));
        }
        var request = await _server.GetAsyncAndCaptureRequest(originalRequest);
        
        // Assert
        Assert.NotNull(request.AcceptEncoding);
        Assert.Equal(encodings.Split(","), request.AcceptEncoding.Encodings);
    }
}